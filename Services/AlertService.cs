using InventoryApp.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace InventoryApp.Services
{
    public class AlertService
    {
        public AlertService()
        {
            Messages = new List<Alert>();
        }

        public List<Alert> Messages { get; set; }
        public event Action OnRefreshRequested;

        public void AddMessage(Alert alert, int sec = 5)
        {
            Messages.Add(alert);
            Console.WriteLine("Message Count : {0}", Messages.Count);
            OnRefreshRequested?.Invoke();

            new Timer((_) =>
            {
                if (Messages.Count > 0)
                {
                    Messages.RemoveAt(0);
                    Console.WriteLine("Message Removed at 0 position!");
                    OnRefreshRequested?.Invoke();
                }
            }, null, sec * 1000, Timeout.Infinite);
        }
    }
}
