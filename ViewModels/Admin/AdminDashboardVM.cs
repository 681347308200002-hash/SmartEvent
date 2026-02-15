namespace SmartEvent.ViewModels.Admin
{
    public class AdminDashboardVM
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        // KPI cards
        public decimal TotalRevenueAllTime { get; set; }
        public int TotalTicketsAllTime { get; set; }
        public int TotalPurchasesAllTime { get; set; }

        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueThisYear { get; set; }

        public int TotalMembers { get; set; }
        public int ActiveMembersLast90Days { get; set; }

   


        // Charts / series
        public List<MonthlySalesPoint> MonthlySales { get; set; } = new();
        public List<YearlySalesPoint> YearlySales { get; set; } = new();

        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyRevenueSeries { get; set; } = new();

        public List<string> TopEventLabels { get; set; } = new();
        public List<decimal> TopEventRevenueSeries { get; set; } = new();

        // Tables
        public List<TopEventRow> TopEvents { get; set; } = new();
        public List<TopMemberRow> TopMembers { get; set; } = new();
    }

    public class MonthlySalesPoint
    {
        public int Year { get; set; }
        public int Month { get; set; }   // 1..12
        public decimal Revenue { get; set; }
        public int Tickets { get; set; }
    }

    public class YearlySalesPoint
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int Tickets { get; set; }
    }

    public class TopEventRow
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Revenue { get; set; }
        public int Tickets { get; set; }
    }

    public class TopMemberRow
    {
        public string UserId { get; set; } = "";
        public string UserNameOrEmail { get; set; } = "";
        public decimal Revenue { get; set; }
        public int Tickets { get; set; }
        public int Purchases { get; set; }
    }
}
