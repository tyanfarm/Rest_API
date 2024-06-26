namespace Rest_API.Services;

public interface IEmailSender {
    Task SendEmailAsync(string email, string subject, string message);
}