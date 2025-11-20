using Microsoft.AspNetCore.Mvc;
using solicitudMovimientosPcs.Services;

[Route("dev")]
public class DevController : Controller
{
    private readonly IEmailService _email;
    public DevController(IEmailService email) => _email = email;

    // POST /dev/test-email?to=Zaid.Garcia@meax.mx
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromQuery] string to = "Zaid.Garcia@meax.mx")
    {
        try
        {
            var subject = "PRUEBA SMTP - cambiosPCS";
            var body = $@"<p>Este es un correo de <strong>prueba</strong> enviado el {DateTime.Now:yyyy-MM-dd HH:mm:ss}.</p>
                          <p>Servidor: {Environment.MachineName}</p>";
            await _email.SendAsync(to, subject, body);
            return Ok(new { ok = true, to, subject });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }
}
