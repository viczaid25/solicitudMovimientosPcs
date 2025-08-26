using System.DirectoryServices;

namespace solicitudMovimientosPcs.Services
{
    /// <summary>
    /// Proporciona servicios para interactuar con Active Directory, incluyendo validación de credenciales
    /// y recuperación de información de usuario.
    /// </summary>
    public class ActiveDirectoryService
    {
        private readonly string _domain;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de Active Directory.
        /// </summary>
        /// <param name="domain">El nombre de dominio del Active Directory al que conectarse.</param>
        public ActiveDirectoryService(string domain)
        {
            _domain = domain;
        }

        /// <summary>
        /// Valida las credenciales de un usuario contra el Active Directory.
        /// </summary>
        /// <param name="username">Nombre de usuario (sAMAccountName) a validar.</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <returns>
        /// True si las credenciales son válidas y el usuario existe en el directorio,
        /// False en caso contrario o si ocurre un error.
        /// </returns>
        /// <remarks>
        /// Este método intenta autenticarse con el dominio usando las credenciales proporcionadas
        /// y busca el usuario por su sAMAccountName.
        /// </remarks>
        public bool ValidateCredentials(string username, string password)
        {
            try
            {
                using (var entry = new DirectoryEntry($"LDAP://{_domain}", username, password))
                {
                    using (var searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = $"(sAMAccountName={username})";
                        searcher.PropertiesToLoad.Add("displayName"); // Campo que quieres traer

                        var result = searcher.FindOne();

                        // Si se encuentra el usuario y las credenciales son válidas, retorna true
                        return result != null;
                    }
                }
            }
            catch
            {
                // Si hay un error (credenciales incorrectas, etc.), retorna false
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre para mostrar (displayName) de un usuario desde Active Directory.
        /// </summary>
        /// <param name="username">Nombre de usuario (sAMAccountName).</param>
        /// <param name="password">Contraseña del usuario.</param>
        /// <returns>
        /// El nombre para mostrar del usuario si existe y las credenciales son válidas,
        /// o string.Empty si no se encuentra, no tiene displayName o ocurre un error.
        /// </returns>
        /// <remarks>
        /// Requiere credenciales válidas para realizar la consulta.
        /// </remarks>
        public string GetDisplayName(string username, string password)
        {
            try
            {
                using (var entry = new DirectoryEntry($"LDAP://{_domain}", username, password))
                {
                    using (var searcher = new DirectorySearcher(entry))
                    {
                        searcher.Filter = $"(sAMAccountName={username})";
                        searcher.PropertiesToLoad.Add("displayName");

                        var result = searcher.FindOne();

                        if (result != null && result.Properties["displayName"].Count > 0)
                        {
                            return result.Properties["displayName"][0].ToString();
                        }
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
}