using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;

namespace SmartEvent.Controllers
{
    [Authorize(Roles = "Member")]
    public class PurchaseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseController(ApplicationDbContext context,
                                  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Purchase/Buy/5  (5 = EventId)
        public async Task<IActionResult> Buy(int id)
        {
            var eventData = await _context.Events
                .Include(e => e.SeatTypes)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventData == null)
                return NotFound();

            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int eventId, int seatTypeId, int quantity)
        {
            // Start transaction to avoid overbooking
            using var tx = await _context.Database.BeginTransactionAsync();

            var seatType = await _context.SeatTypes
                .FirstOrDefaultAsync(s => s.SeatTypeId == seatTypeId && s.EventId == eventId);


            if (seatType == null)
                return NotFound();

            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Invalid quantity.");
                return RedirectToAction("Buy", new { id = eventId });
            }

            if (seatType.QuantityAvailable < quantity)
            {
                TempData["Error"] = "Not enough seats available!";
                return RedirectToAction("Buy", new { id = eventId });
            }

            var user = await _userManager.GetUserAsync(User);

            var totalPrice = seatType.Price * quantity;

            // Create unique QR string
            var code = $"TICKET-{Guid.NewGuid()}";


            var purchase = new TicketPurchase
            {
                UserId = user!.Id,
                EventId = eventId,
                SeatTypeId = seatTypeId,
                Quantity = quantity,
                UnitPrice = seatType.Price,
                TotalPrice = totalPrice,
                PurchasedAt = DateTime.UtcNow,
                QrCodeValue = code
            };


            // Reduce seats
            seatType.QuantityAvailable -= quantity;

            _context.TicketPurchases.Add(purchase);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction("Success", new { id = purchase.TicketPurchaseId });

        }

        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);

            var tickets = await _context.TicketPurchases
                .Include(t => t.Event)
                .Include(t => t.SeatType)
                .Where(t => t.UserId == user!.Id)
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();

            return View(tickets);
        }

        public async Task<IActionResult> Success(int id)
        {
            var ticket = await _context.TicketPurchases
                .Include(t => t.Event)
                .Include(t => t.SeatType)
                .FirstOrDefaultAsync(t => t.TicketPurchaseId == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

    }
}


    
