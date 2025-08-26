using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

/// <summary>
/// Servicio para sincronizar usuarios de Active Directory con el sistema de Identity.
/// </summary>
public class AdUserManagerService
{
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    /// Inicializa una nueva instancia del servicio AdUserManagerService.
    /// </summary>
    /// <param name="userManager">El UserManager de ASP.NET Core Identity para gestionar usuarios.</param>
    public AdUserManagerService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Sincroniza un usuario de Active Directory con el sistema de Identity.
    /// </summary>
    /// <param name="principal">ClaimsPrincipal que representa al usuario autenticado.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="System.Exception">
    /// Se lanza cuando no se puede crear el usuario en el sistema de Identity.
    /// </exception>
    /// <remarks>
    /// Este método verifica si el usuario existe en Identity. Si no existe, crea un nuevo usuario
    /// utilizando el nombre de usuario de AD y genera un correo electrónico predeterminado.
    /// </remarks>
    public async Task SyncAdUserAsync(ClaimsPrincipal principal)
    {
        var userName = principal.Identity.Name; // Obtiene el nombre de usuario de AD

        // Busca el usuario en la base de datos de Identity
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            // Si el usuario no existe, créalo
            user = new IdentityUser
            {
                UserName = userName,
                Email = $"{userName}@dominio.com" // Puedes ajustar el correo electrónico según tu necesidad
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                // Maneja el error si no se puede crear el usuario
                throw new System.Exception("No se pudo crear el usuario en Identity.");
            }
        }
    }
}