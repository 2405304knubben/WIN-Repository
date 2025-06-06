using System;
using System.Collections.Generic;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }  // We gebruiken alleen deze voor het totaal aantal klanten
        public List<(DateTime Date, int Count)> DailyOrderCounts { get; set; }
        public List<(DateTime Date, decimal Revenue)> DailyRevenue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
