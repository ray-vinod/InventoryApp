using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace InventoryApp.Models
{
    public class MailRequest
    {
        public string ToEmail { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public IList<IBrowserFile> Attachments { get; set; }

        public string FileName { get; set; }
        public byte[] bytes { get; set; }
        public string ContentType { get; set; }
    }
}
