using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartEvent.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
       


        // ✅ Only ONE constructor (fixes "Multiple constructors..." DI error)
        public EventsController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventData = await _context.Events
                .Include(e => e.Reviews)
                .Include(e => e.SeatTypes)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventData == null) return NotFound();


            if (User.IsInRole("Member"))
            {
                var userId = _userManager.GetUserId(User);

                ViewBag.AlreadyReviewed = await _context.Reviews
                    .AnyAsync(r => r.EventId == eventData.EventId && r.UserId == userId);
            }

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Member"))
            {
                var userId = _userManager.GetUserId(User);

                ViewBag.HasPurchased = await _context.TicketPurchases
                    .AnyAsync(t => t.EventId == eventData.EventId && t.UserId == userId);

                ViewBag.AlreadyReviewed = await _context.Reviews
                    .AnyAsync(r => r.EventId == eventData.EventId && r.UserId == userId);
            }

            return View(eventData);

            


        }


        // GET: Events/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("EventId,EventName,Category,EventDate,Location,BasePrice,Description")] Event @event,
            IFormFile poster)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() })
            .ToList();

                TempData["ModelErrors"] = System.Text.Json.JsonSerializer.Serialize(errors);
                return View(@event);
            }
                

            // Handle poster upload (optional)
            if (poster != null && poster.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(poster.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Please upload a JPG, PNG, or WEBP image.");
                    return View(@event);
                }

                // Optional size limit: 2MB
                if (poster.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Poster image must be 2MB or less.");
                    return View(@event);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(_env.WebRootPath, "uploads", "events");
                Directory.CreateDirectory(saveDir);

                var savePath = Path.Combine(saveDir, fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }

                @event.PosterPath = $"/uploads/events/{fileName}";
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "SeatTypes", new { eventId = @event.EventId });
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("EventId,EventName,Category,EventDate,Location,BasePrice,Description,PosterPath")] Event @event,
            IFormFile poster)
        {
            if (id != @event.EventId) return NotFound();

            if (!ModelState.IsValid)
                return View(@event);

            // Keep old poster unless a new one is uploaded
            var existing = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
            if (existing == null) return NotFound();

            if (poster != null && poster.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(poster.FileName).ToLowerInvariant();

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Please upload a JPG, PNG, or WEBP image.");
                    return View(@event);
                }

                if (poster.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Poster image must be 2MB or less.");
                    return View(@event);
                }

                // Optional: delete old file if exists
                if (!string.IsNullOrWhiteSpace(existing.PosterPath))
                {
                    var oldPath = existing.PosterPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                    var fullOldPath = Path.Combine(_env.WebRootPath, oldPath);
                    if (System.IO.File.Exists(fullOldPath))
                        System.IO.File.Delete(fullOldPath);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveDir = Path.Combine(_env.WebRootPath, "uploads", "events");
                Directory.CreateDirectory(saveDir);

                var savePath = Path.Combine(saveDir, fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }

                @event.PosterPath = $"/uploads/events/{fileName}";
            }
            else
            {
                // No new upload → keep old poster
                @event.PosterPath = existing.PosterPath;
            }

            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.EventId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Events/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return RedirectToAction(nameof(Index));

            // Optional: delete poster file
            if (!string.IsNullOrWhiteSpace(@event.PosterPath))
            {
                var rel = @event.PosterPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var full = Path.Combine(_env.WebRootPath, rel);
                if (System.IO.File.Exists(full))
                    System.IO.File.Delete(full);
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
