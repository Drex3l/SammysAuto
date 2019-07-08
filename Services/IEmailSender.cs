using System.Threading.Tasks;

namespace SammysAuto.Services
{
    public interface IEmailSender
    {
         Task SendEmailAsync(string email, string subject, string message);
    }
}