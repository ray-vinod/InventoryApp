using InventoryApp.Models;
using System.Threading.Tasks;

namespace InventoryApp.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
