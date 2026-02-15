using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;
using System.Diagnostics;



namespace SmartEvent.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(
    string? q,
    string? category,
    string? location,
    decimal? minPrice,
    decimal? maxPrice)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(e => e.EventName.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                location = location.Trim();
                query = query.Where(e => e.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(e => e.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(e => e.BasePrice <= maxPrice.Value);
            }

            var events = await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Events
                .Select(e => e.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.SelectedCategory = category;
            ViewBag.Location = location;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(events);
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
