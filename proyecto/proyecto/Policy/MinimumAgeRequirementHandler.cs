using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace proyecto.Policy
{
    public class MinimumAgeRequirementHandler : AuthorizationHandler<MinimumAgeRequirement>
    {

        private readonly ILogger<MinimumAgeRequirementHandler> _logger;

        public MinimumAgeRequirementHandler(ILogger<MinimumAgeRequirementHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            // Obtener el claim de la edad del usuario
            //var ageClaim = context.User.FindFirst("Age");
            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
            }

            //var ageClaim = context.User.FindFirst(c => c.Type == "Age");
            var ageClaim = context.User.FindFirst(c => c.Type.EndsWith("Age"));
            if (ageClaim == null)
            {
                _logger.LogWarning("El usuario no tiene el claim de edad.");
                return Task.CompletedTask; // No tiene el claim de edad, no pasa la validación
            }

            if (!int.TryParse(ageClaim.Value, out int userAge))
            {
                _logger.LogWarning("El claim de edad no es un número válido.");
                return Task.CompletedTask; // El claim no es un número válido
            }

            // Verificar si el usuario es mayor o igual a la edad mínima
            if (userAge < requirement.MinimumAge)
            {
                _logger.LogWarning($"El usuario tiene {userAge} años, pero se requieren {requirement.MinimumAge}.");
                return Task.CompletedTask; // No cumple con la edad requerida
            }

            /*
            // Verificar si el usuario tiene el rol de "Admin"
            if (!context.User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals("ADMIN", StringComparison.OrdinalIgnoreCase)))
            {
                return Task.CompletedTask; // No tiene el rol necesario
            }
            */

            // Extraer todos los roles del usuario
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToList();

            if (!roles.Contains("ADMIN", StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("El usuario no tiene el rol de Admin.");
                return Task.CompletedTask;
            }

            // Si cumple con la edad y el rol, conceder el permiso
            // Si cumple con la edad y el rol, conceder el permiso
            _logger.LogInformation("El usuario cumple con la edad y el rol de Admin. Autorizado.");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
