using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;
using SmartEvent.ViewModels;

namespace SmartEvent.Controllers
{
    public class InquiriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquiriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Inquiries/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? eventId)
        {
            // Optional: verify event exists if eventId provided
            if (eventId.HasValue)
            {
                var exists = await _context.Events.AnyAsync(e => e.EventId == eventId.Value);
                if (!exists) return NotFound();
            }

            var vm = new InquiryCreateVM { EventId = eventId };
            return View(vm);
        }

        // POST: /Inquiries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InquiryCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var inquiry = new Inquiry
            {
                Name = string.IsNullOrWhiteSpace(vm.Name) ? null : vm.Name.Trim(),
                Email = vm.Email.Trim(),
                Subject = vm.Subject.Trim(),
                Message = vm.Message.Trim(),
                EventId = vm.EventId,
                CreatedAt = DateTime.UtcNow,
                Status = InquiryStatus.Pending
            };

            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou));
        }

        // GET: /Inquiries/ThankYou
        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
