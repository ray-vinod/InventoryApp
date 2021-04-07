using InventoryApp.Models;
using InventoryApp.Models.Enums;
using InventoryApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace InventoryApp.Pages
{
    public partial class Email
    {
        private MailRequest mailRequest;
        [Inject]
        private IMailService MailService { get; set; }
        [Inject]
        private AlertService AlertService { get; set; }
        [Inject]
        private NavigationManager PageNavigation { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        [Inject]
        private ILogger<Email> Logger { get; set; }



        protected override void OnInitialized()
        {
            mailRequest = new MailRequest();
            if (!IsConnectedToInternet())
            {
                Logger.LogWarning("You are not connected to internet!");
                AlertService.AddMessage(new Alert("Your Not Connected to the Internet!",
                    AlertType.Error));
                return;
            }
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs evnt)
        {
            await Task.Delay(0);
            var files = evnt.GetMultipleFiles();
            if (files.Count > 0)
            {
                mailRequest.Attachments = new List<IBrowserFile>();
                foreach (var file in files)
                {
                    mailRequest.Attachments.Add(file);
                }

                Logger.LogInformation("File(s) are loaded!");
            }
        }

        private async Task CallSendEmail()
        {
            if (!IsConnectedToInternet())
            {
                Logger.LogWarning("You are not connected to internet!");
                AlertService.AddMessage(new Alert("Your Not Connected to the Internet!", AlertType.Error));
                return;
            }

            await Task.Run(async () => await MailService.SendEmailAsync(mailRequest));
            AlertService.AddMessage(new Alert("Email has been sent to : " + mailRequest.ToEmail, 
                AlertType.Success));

            PageNavigation.NavigateTo("/dashboard", false);
            Logger.LogInformation("Email sent!");
        }

        private async Task Delete(string file)
        {
            var f = mailRequest.Attachments.Where(x => x.Name == file).FirstOrDefault();
            var result = mailRequest.Attachments.Remove(f);
            if (result)
            {
                await InvokeAsync(() => StateHasChanged());
                Logger.LogWarning("File(s) are removed form list!");
            }
        }

        private bool IsConnectedToInternet()
        {
            string host = "www.google.com";
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success)
                {
                    result = true;
                }
            }
            catch
            {
            }

            Logger.LogInformation("Pinging google.com for testing network!");
            return result;
        }
    }
}