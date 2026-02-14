using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Data;
using SmartEvent.Models;

namespace SmartEvent.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SeatTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // A–E options
        private static readonly string[] SeatTypeOptions = new[] { "A", "B", "C", "D", "E" };

        private void LoadSeatTypeOptions(string? selected = null)
        {
            ViewBag.SeatTypeOptions = new SelectList(SeatTypeOptions, selected);
        }

        public SeatTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SeatTypes
        public async Task<IActionResult> Index(int? eventId)
        {
            var query = _context.SeatTypes.Include(s => s.Event).AsQueryable();

            if (eventId.HasValue)
                query = query.Where(s => s.EventId == eventId.Value);

            ViewBag.EventId = eventId;
            return View(await query.ToListAsync());
        }

        // GET: SeatTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var seatType = await _context.SeatTypes
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.SeatTypeId == id);

            if (seatType == null) return NotFound();

            return View(seatType);
        }

        // GET: SeatTypes/Create?eventId=5
        public async Task<IActionResult> Create(int eventId)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null) return NotFound();

            ViewBag.EventName = evt.EventName;
            LoadSeatTypeOptions(); // ✅ dropdown A–E

            var seatType = new SeatType { EventId = eventId };
            return View(seatType);
        }

        // POST: SeatTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SeatTypeId,TypeName,Price,QuantityAvailable,EventId")] SeatType seatType)
        {
            // ✅ prevent duplicate A–E for the same event
            bool exists = await _context.SeatTypes.AnyAsync(s =>
                s.EventId == seatType.EventId && s.TypeName == seatType.TypeName);

            if (exists)
                ModelState.AddModelError("TypeName", "This seat type already exists for this event.");

            if (ModelState.IsValid)
            {
                _context.Add(seatType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { eventId = seatType.EventId });
            }

            var evt = await _context.Events.FindAsync(seatType.EventId);
            ViewBag.EventName = evt?.EventName;

            LoadSeatTypeOptions(seatType.TypeName); // ✅ keep selected value
            return View(seatType);
        }

        // GET: SeatTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var seatType = await _context.SeatTypes.FindAsync(id);
            if (seatType == null) return NotFound();

            LoadSeatTypeOptions(seatType.TypeName); // ✅ dropdown selected
            return View(seatType);
        }

        // POST: SeatTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SeatTypeId,TypeName,Price,QuantityAvailable,EventId")] SeatType seatType)
        {
            if (id != seatType.SeatTypeId) return NotFound();

            // ✅ prevent duplicate A–E for the same event (excluding itself)
            bool exists = await _context.SeatTypes.AnyAsync(s =>
                s.EventId == seatType.EventId &&
                s.TypeName == seatType.TypeName &&
                s.SeatTypeId != seatType.SeatTypeId);

            if (exists)
                ModelState.AddModelError("TypeName", "This seat type already exists for this event.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seatType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatTypeExists(seatType.SeatTypeId)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index), new { eventId = seatType.EventId });
            }

            LoadSeatTypeOptions(seatType.TypeName); // ✅ keep selected value
            return View(seatType);
        }

        // GET: SeatTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var seatType = await _context.SeatTypes
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.SeatTypeId == id);

            if (seatType == null) return NotFound();

            return View(seatType);
        }

        // POST: SeatTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seatType = await _context.SeatTypes.FindAsync(id);
            if (seatType == null) return RedirectToAction(nameof(Index));

            var eventId = seatType.EventId;

            _context.SeatTypes.Remove(seatType);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { eventId });
        }

        private bool SeatTypeExists(int id)
        {
            return _context.SeatTypes.Any(e => e.SeatTypeId == id);
        }
    }
}
