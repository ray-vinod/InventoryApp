using System;

namespace InventoryApp.Models.ViewModels
{
    public class ReceiveReportViewModel
    {
        public DateTime Receive_Date { get; set; }
        public int Batch { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public DateTime Manufacture_Date { get; set; }
        public DateTime Expiry_Date { get; set; }
        public int Qty { get; set; }
        public string Remarks { get; set; }
        public string Received_By { get; set; }
    }
}
