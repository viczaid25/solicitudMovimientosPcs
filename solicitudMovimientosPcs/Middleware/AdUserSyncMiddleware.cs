using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

/// <summary>
/// Middleware para sincronizar automáticamente usuarios de Active Directory con el sistema de Identity
/// durante el procesamiento de cada solicitud HTTP autenticada.
/// </summary>
public class AdUserSyncMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Inicializa una nueva instancia del middleware de sincronización de usuarios.
    /// </summary>
    /// <param name="next">El siguiente delegado en la pipeline de solicitudes HTTP.</param>
    public AdUserSyncMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Método invocado por el runtime de ASP.NET Core para procesar las solicitudes HTTP.
    /// </summary>
    /// <param name="context">El contexto HTTP para la solicitud actual.</param>
    /// <param name="adUserManagerService">Servicio de gestión de usuarios de AD inyectado por DI.</param>
    /// <returns>Una tarea que representa la ejecución del middleware.</returns>
    /// <remarks>
    /// Este middleware verifica si el usuario está autenticado y, en ese caso,
    /// invoca el servicio de sincronización de usuarios antes de pasar la solicitud
    /// al siguiente middleware en la pipeline.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context, AdUserManagerService adUserManagerService)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            await adUserManagerService.SyncAdUserAsync(context.User);
        }

        await _next(context);
    }
}