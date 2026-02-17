using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QRCoder;

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

        [HttpGet]
        public async Task<IActionResult> DownloadQrPdf(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Make sure the ticket exists and belongs to the logged-in user
            var ticket = await _context.TicketPurchases
                .Include(t => t.Event)
                .Include(t => t.SeatType)
                .FirstOrDefaultAsync(t => t.TicketPurchaseId == id && t.UserId == user!.Id);

            if (ticket == null)
                return NotFound();

            // Build QR code image (PNG bytes)
            using var qrGenerator = new QRCodeGenerator();

            var qrText = Url.Action(
                "Verify",
                "Tickets",
                new { code = ticket.QrCodeValue },
                protocol: Request.Scheme
            ) ?? "";

            using var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(20);

            // Create PDF
            var document = new PdfDocument();
            document.Info.Title = $"SmartEvent Ticket #{ticket.TicketPurchaseId}";

            var page = document.AddPage();
            page.Width = 595;   // A4 width (points)
            page.Height = 842;  // A4 height (points)

            using var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var textFont = new XFont("Arial", 12, XFontStyle.Regular);

            // Title + details
            gfx.DrawString("SmartEvent - Ticket", titleFont, XBrushes.Black, new XRect(40, 40, page.Width - 80, 30), XStringFormats.TopLeft);

            gfx.DrawString($"Event: {ticket.Event?.EventName}", textFont, XBrushes.Black, new XRect(40, 90, page.Width - 80, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Seat: {ticket.SeatType?.TypeName}", textFont, XBrushes.Black, new XRect(40, 110, page.Width - 80, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Qty: {ticket.Quantity}", textFont, XBrushes.Black, new XRect(40, 130, page.Width - 80, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Total: {ticket.TotalPrice} USD", textFont, XBrushes.Black, new XRect(40, 150, page.Width - 80, 20), XStringFormats.TopLeft);
            gfx.DrawString($"Purchased: {ticket.PurchasedAt.ToLocalTime():yyyy-MM-dd HH:mm}", textFont, XBrushes.Black, new XRect(40, 170, page.Width - 80, 20), XStringFormats.TopLeft);

            // Draw QR image
            using var imgStream = new MemoryStream(qrBytes);
            using var xImg = XImage.FromStream(() => imgStream);

            // Position + size
            gfx.DrawImage(xImg, 40, 220, 220, 220);

            gfx.DrawString("Scan this QR code at entry.", textFont, XBrushes.Black, new XRect(40, 460, page.Width - 80, 20), XStringFormats.TopLeft);

            // Return PDF
            using var ms = new MemoryStream();
            document.Save(ms);
            var pdfBytes = ms.ToArray();

            var fileName = $"Ticket_{ticket.TicketPurchaseId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
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


    
