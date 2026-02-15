using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;

namespace SmartEvent.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Anyone can open, but  will show limited info unless Admin
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return NotFound();

            var ticket = await _context.TicketPurchases
                .Include(t => t.Event)
                .Include(t => t.SeatType)
                .FirstOrDefaultAsync(t => t.QrCodeValue == code);

            if (ticket == null)
                return NotFound();

            // Admin sees full detail, others see limited detail
            if (!User.IsInRole("Admin"))
            {
                // show limited info to normal viewers
                ViewBag.Limited = true;
            }

            return View(ticket);
        }
    }
}
