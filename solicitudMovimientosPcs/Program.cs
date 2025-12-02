using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using solicitudMovimientosPcs.Data;
using solicitudMovimientosPcs.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

// Excepciones EF en dev
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Caching (para StageAccessService)
builder.Services.AddMemoryCache();

// Servicios propios
builder.Services.AddScoped<IStageAccessService, StageAccessService>();

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// AD (si lo usas)
builder.Services.AddSingleton(new ActiveDirectoryService("ad.meax.mx"));

// Destinatarios por etapa (UNA sola vez y como IReadOnlyDictionary)
var destinos =
    builder.Configuration.GetSection("Aprobaciones:Destinatarios")
        .Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
builder.Services.AddSingleton<IReadOnlyDictionary<string, string>>(destinos);

// Autenticación por cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "PCS.Auth";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// MVC + autorización global
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    Debug.WriteLine($"Autenticado: {context.User.Identity?.IsAuthenticated}");
    Debug.WriteLine($"Usuario: {context.User.Identity?.Name}");
    await next();
});
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Redirigir raíz a Login
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

app.Run();
