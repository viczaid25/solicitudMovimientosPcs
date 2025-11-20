namespace solicitudMovimientosPcs.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; } = 25;
        public string FromEmail { get; set; } = "";
        public bool EnableSsl { get; set; } = false;
    }
}
