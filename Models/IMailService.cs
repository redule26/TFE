namespace VWA_TFE.Models
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
