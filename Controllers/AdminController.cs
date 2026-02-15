using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;
using SmartEvent.ViewModels.Admin;

namespace SmartEvent.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return await Reports(null, null);
        }


        // REPORTS page (Charts + Date Filter + Export)
        [HttpGet]
        public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
        {
            // Default range: last 30 days
            var toDate = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);   // end of day
            var fromDate = (from ?? DateTime.UtcNow.AddDays(-30)).Date;         // start of day

            // Filtered purchases
            var purchasesQuery = _context.TicketPurchases
                .Where(x => x.PurchasedAt >= fromDate && x.PurchasedAt <= toDate);

            // KPI totals (filtered)
            var totalRevenue = await purchasesQuery.SumAsync(x => (decimal?)x.TotalPrice) ?? 0m;
            var totalTickets = await purchasesQuery.SumAsync(x => (int?)x.Quantity) ?? 0;
            var totalPurchases = await purchasesQuery.CountAsync();

            // Active members in range (filtered)
            var activeMembersInRange = await purchasesQuery
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync();

            // Total members (all time) - optional, keep it as system total
            var totalMembers = await _userManager.Users.CountAsync();

            // Monthly buckets between from-to (for chart)
            // We’ll group by Year/Month but only within range
            var monthlyRaw = await purchasesQuery
                .GroupBy(x => new { x.PurchasedAt.Year, x.PurchasedAt.Month })
                .Select(g => new MonthlySalesPoint
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TotalPrice),
                    Tickets = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // Create continuous month list between fromDate and toDate
            var startMonth = new DateTime(fromDate.Year, fromDate.Month, 1);
            var endMonth = new DateTime(toDate.Year, toDate.Month, 1);

            var monthly = new List<MonthlySalesPoint>();
            for (var d = startMonth; d <= endMonth; d = d.AddMonths(1))
            {
                var found = monthlyRaw.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
                monthly.Add(found ?? new MonthlySalesPoint { Year = d.Year, Month = d.Month, Revenue = 0m, Tickets = 0 });
            }

            // Yearly sales (filtered range)
            var yearly = await purchasesQuery
                .GroupBy(x => x.PurchasedAt.Year)
                .Select(g => new YearlySalesPoint
                {
                    Year = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    Tickets = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            // Top events (filtered range)
            var topEventsAgg = await purchasesQuery
                .GroupBy(x => x.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    Tickets = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var eventIds = topEventsAgg.Select(x => x.EventId).ToList();
            var events = await _context.Events
                .Where(e => eventIds.Contains(e.EventId))
                .ToDictionaryAsync(e => e.EventId);

            var topEventRows = topEventsAgg.Select(x =>
            {
                var e = events[x.EventId];
                return new TopEventRow
                {
                    EventId = x.EventId,
                    EventName = e.EventName,
                    Category = e.Category,
                    Revenue = x.Revenue,
                    Tickets = x.Tickets
                };
            }).ToList();

            // Top members (filtered range)
            var topMembersAgg = await purchasesQuery
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Revenue = g.Sum(x => x.TotalPrice),
                    Tickets = g.Sum(x => x.Quantity),
                    Purchases = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToListAsync();

            var userIds = topMembersAgg.Select(x => x.UserId).ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var topMemberRows = topMembersAgg.Select(x =>
            {
                var u = users[x.UserId];
                return new TopMemberRow
                {
                    UserId = x.UserId,
                    UserNameOrEmail = u.Email ?? u.UserName ?? x.UserId,
                    Revenue = x.Revenue,
                    Tickets = x.Tickets,
                    Purchases = x.Purchases
                };
            }).ToList();

            // Chart series
            var monthlyLabels = monthly.Select(m => $"{m.Year}-{m.Month:00}").ToList();
            var monthlyRevenueSeries = monthly.Select(m => m.Revenue).ToList();

            var topEventLabels = topEventRows.Select(e => e.EventName).ToList();
            var topEventRevenueSeries = topEventRows.Select(e => e.Revenue).ToList();

            var vm = new AdminDashboardVM
            {
                From = fromDate,
                To = toDate,

                TotalRevenueAllTime = totalRevenue,
                TotalTicketsAllTime = totalTickets,
                TotalPurchasesAllTime = totalPurchases,

                TotalMembers = totalMembers,
                ActiveMembersLast90Days = activeMembersInRange, // reuse label (optional rename later)

                MonthlySales = monthly,
                YearlySales = yearly,
                TopEvents = topEventRows,
                TopMembers = topMemberRows,

                MonthlyLabels = monthlyLabels,
                MonthlyRevenueSeries = monthlyRevenueSeries,
                TopEventLabels = topEventLabels,
                TopEventRevenueSeries = topEventRevenueSeries
            };


            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> ExportSalesCsv(DateTime? from, DateTime? to)
        {
            var toDate = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
            var fromDate = (from ?? DateTime.UtcNow.AddDays(-30)).Date;

            var rows = await _context.TicketPurchases
                .Where(x => x.PurchasedAt >= fromDate && x.PurchasedAt <= toDate)
                .OrderByDescending(x => x.PurchasedAt)
                .Select(x => new
                {
                    x.PurchasedAt,
                    x.UserId,
                    x.EventId,
                    x.SeatTypeId,
                    x.Quantity,
                    x.UnitPrice,
                    x.TotalPrice
                })
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("PurchasedAt,UserId,EventId,SeatTypeId,Quantity,UnitPrice,TotalPrice");

            foreach (var r in rows)
            {
                sb.AppendLine($"{r.PurchasedAt:yyyy-MM-dd HH:mm:ss},{r.UserId},{r.EventId},{r.SeatTypeId},{r.Quantity},{r.UnitPrice},{r.TotalPrice}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"sales_{fromDate:yyyyMMdd}_to_{toDate:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }

    }
}
