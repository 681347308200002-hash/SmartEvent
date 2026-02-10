using System;
using System.Collections.Generic;
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

        public SeatTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SeatTypes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SeatTypes.Include(s => s.Event);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SeatTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seatType = await _context.SeatTypes
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.SeatTypeId == id);
            if (seatType == null)
            {
                return NotFound();
            }

            return View(seatType);
        }

        // GET: SeatTypes/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Category");
            return View();
        }

        // POST: SeatTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SeatTypeId,TypeName,Price,QuantityAvailable,EventId")] SeatType seatType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(seatType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Category", seatType.EventId);
            return View(seatType);
        }

        // GET: SeatTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seatType = await _context.SeatTypes.FindAsync(id);
            if (seatType == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Category", seatType.EventId);
            return View(seatType);
        }

        // POST: SeatTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SeatTypeId,TypeName,Price,QuantityAvailable,EventId")] SeatType seatType)
        {
            if (id != seatType.SeatTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(seatType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SeatTypeExists(seatType.SeatTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Category", seatType.EventId);
            return View(seatType);
        }

        // GET: SeatTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var seatType = await _context.SeatTypes
                .Include(s => s.Event)
                .FirstOrDefaultAsync(m => m.SeatTypeId == id);
            if (seatType == null)
            {
                return NotFound();
            }

            return View(seatType);
        }

        // POST: SeatTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seatType = await _context.SeatTypes.FindAsync(id);
            if (seatType != null)
            {
                _context.SeatTypes.Remove(seatType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SeatTypeExists(int id)
        {
            return _context.SeatTypes.Any(e => e.SeatTypeId == id);
        }
    }
}
