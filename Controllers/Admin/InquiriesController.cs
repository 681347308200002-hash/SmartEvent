using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;

namespace SmartEvent.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class InquiriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InquiriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Inquiries/Index?status=Pending&q=abc
        [HttpGet]
        public async Task<IActionResult> Index(string? q, InquiryStatus? status)
        {
            var query = _context.Inquiries
                .Include(i => i.Event)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(i =>
                    i.Email.Contains(q) ||
                    i.Subject.Contains(q) ||
                    (i.Name != null && i.Name.Contains(q)));
            }

            var list = await query
                .OrderBy(i => i.Status)            // Pending first
                .ThenByDescending(i => i.CreatedAt)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Status = status?.ToString() ?? "";
            return View("~/Views/Admin/Inquiries/Index.cshtml", list);

        }

        // GET: /Admin/Inquiries/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var inquiry = await _context.Inquiries
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.InquiryId == id);

            if (inquiry == null) return NotFound();
            return View("~/Views/Admin/Inquiries/Details.cshtml", inquiry);

        }

        // POST: /Admin/Inquiries/MarkReplied/5
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReplied(int id)
        {
            var inquiry = await _context.Inquiries.FindAsync(id);
            if (inquiry == null) return NotFound();

            inquiry.Status = InquiryStatus.Replied;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Inquiries/MarkPending/5
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPending(int id)
        {
            var inquiry = await _context.Inquiries.FindAsync(id);
            if (inquiry == null) return NotFound();

            inquiry.Status = InquiryStatus.Pending;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/Inquiries/Delete/5
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inquiry = await _context.Inquiries.FindAsync(id);
            if (inquiry == null) return NotFound();

            _context.Inquiries.Remove(inquiry);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
