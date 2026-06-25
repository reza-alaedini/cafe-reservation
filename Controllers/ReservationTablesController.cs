using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cafe.Helpers;
using Cafe.Data;
using Cafe.Models;

namespace Cafe.Controllers
{

    [Authorize]

    public class ReservationTablesController : Controller
    {
        private static readonly TimeSpan ReservationDuration = TimeSpan.FromMinutes(90);
        private readonly DataContext _context;

        public ReservationTablesController(DataContext context)
        {
            _context = context;
        }

        // GET: ReservationTables
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = (User?)HttpContext.Items["User"];
            ViewBag.AvailableTables = await _context.ReserveTables
                .Where(table => table.IsAvalible)
                .OrderBy(table => table.Id)
                .ToListAsync();

            var dataContext = _context.Reservations.Include(r => r.Table).Include(r => r.User);
            if (user.Role != Enums.Role.ADMIN)
            {
                return View(await dataContext.Where(x=>x.UserId == user.Id).ToListAsync());
            }
            return View(await dataContext.ToListAsync());

        }

        // GET: ReservationTables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationTables = await _context.Reservations
                .Include(r => r.Table)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservationTables == null)
            {
                return NotFound();
            }

            return View(reservationTables);
        }

        // GET: ReservationTables/Create
        
        public IActionResult Create(int? tableId)
        {
            var availableTables = _context.ReserveTables
                .Where(table => table.IsAvalible)
                .OrderBy(table => table.Id)
                .ToList();
            var selectedTable = tableId.HasValue
                ? availableTables.FirstOrDefault(table => table.Id == tableId.Value)
                : null;

            ViewData["TableId"] = BuildTableSelectList(availableTables, tableId);
            ViewBag.SelectedTable = selectedTable;
            ViewData["ReserveDateJalali"] = JalaliDate.ToInputValue(DateTime.Now.AddHours(1));
            return View(new ReservationTables { TableId = selectedTable?.Id ?? 0 });
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ReservationTables reservationTables, string? ReserveDateJalali)
        {
            var user = (User?)HttpContext.Items["User"];
            ViewData["ReserveDateJalali"] = ReserveDateJalali;

            if (!JalaliDate.TryParseInput(ReserveDateJalali, out var reserveDate))
            {
                ModelState.AddModelError("ReserveDateJalali", "تاریخ رزرو را با فرمت ۱۴۰۳/۰۳/۳۰ ۱۸:۳۰ وارد کنید.");
                PrepareCreateViewData(reservationTables.TableId, ReserveDateJalali);
                return View(reservationTables);
            }

            var selectedTable = await _context.ReserveTables.FirstOrDefaultAsync(table => table.Id == reservationTables.TableId);
            if (selectedTable == null || !selectedTable.IsAvalible)
            {
                ModelState.AddModelError(string.Empty, "این میز در حال حاضر قابل رزرو نیست.");
                PrepareCreateViewData(reservationTables.TableId, ReserveDateJalali);
                return View(reservationTables);
            }

            reservationTables.ReserveDate = reserveDate;

            var requestedStart = reserveDate;
            var requestedEnd = requestedStart.Add(ReservationDuration);
            var conflictingReservation = await _context.Reservations
                .Where(reservation =>
                    reservation.TableId == reservationTables.TableId &&
                    reservation.ReserveDate < requestedEnd &&
                    reservation.ReserveDate.AddMinutes(ReservationDuration.TotalMinutes) > requestedStart)
                .OrderBy(reservation => reservation.ReserveDate)
                .FirstOrDefaultAsync();

            if (conflictingReservation != null)
            {
                SetReservationConflictMessage(conflictingReservation);
                ModelState.AddModelError(string.Empty, ViewBag.ReservationConflictMessage);
                PrepareCreateViewData(reservationTables.TableId, ReserveDateJalali);
                return View(reservationTables);
            }

            reservationTables.UserId = user.Id;
            _context.Add(reservationTables);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        // GET: ReservationTables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationTables = await _context.Reservations.FindAsync(id);
            if (reservationTables == null)
            {
                return NotFound();
            }
            ViewData["TableId"] = BuildTableSelectList(
                await _context.ReserveTables.OrderBy(table => table.Id).ToListAsync(),
                reservationTables.TableId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", reservationTables.UserId);
            ViewData["ReserveDateJalali"] = JalaliDate.ToInputValue(reservationTables.ReserveDate);
            return View(reservationTables);
        }

        // POST: ReservationTables/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,TableId")] ReservationTables reservationTables, string? ReserveDateJalali)
        {
            if (id != reservationTables.Id)
            {
                return NotFound();
            }

            if (!JalaliDate.TryParseInput(ReserveDateJalali, out var reserveDate))
            {
                ModelState.AddModelError("ReserveDateJalali", "تاریخ رزرو را با فرمت ۱۴۰۳/۰۳/۳۰ ۱۸:۳۰ وارد کنید.");
            }
            else
            {
                reservationTables.ReserveDate = reserveDate;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservationTables);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationTablesExists(reservationTables.Id))
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
            ViewData["TableId"] = BuildTableSelectList(
                await _context.ReserveTables.OrderBy(table => table.Id).ToListAsync(),
                reservationTables.TableId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", reservationTables.UserId);
            ViewData["ReserveDateJalali"] = ReserveDateJalali;
            return View(reservationTables);
        }

        // GET: ReservationTables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservationTables = await _context.Reservations
                .Include(r => r.Table)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservationTables == null)
            {
                return NotFound();
            }

            return View(reservationTables);
        }

        // POST: ReservationTables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservationTables = await _context.Reservations.FindAsync(id);
            if (reservationTables != null)
            {
                _context.Reservations.Remove(reservationTables);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationTablesExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

        private void PrepareCreateViewData(int tableId, string? reserveDateJalali)
        {
            var availableTables = _context.ReserveTables
                .Where(table => table.IsAvalible)
                .OrderBy(table => table.Id)
                .ToList();

            ViewData["TableId"] = BuildTableSelectList(availableTables, tableId);
            ViewBag.SelectedTable = availableTables.FirstOrDefault(table => table.Id == tableId);
            ViewData["ReserveDateJalali"] = reserveDateJalali;
        }

        private static List<SelectListItem> BuildTableSelectList(IEnumerable<ReserveTable> tables, int? selectedTableId)
        {
            return tables.Select(table => new SelectListItem
            {
                Value = table.Id.ToString(),
                Text = $"میز {table.Id} - هزینه رزرو: {table.Price}",
                Selected = selectedTableId == table.Id
            }).ToList();
        }

        private void SetReservationConflictMessage(ReservationTables reservation)
        {
            var reservedFrom = JalaliDate.ToShortDateTime(reservation.ReserveDate);
            var reservedUntil = JalaliDate.ToShortDateTime(reservation.ReserveDate.Add(ReservationDuration));

            ViewBag.ReservationConflictTitle = "این زمان قابل رزرو نیست";
            ViewBag.ReservationConflictMessage = $"این میز از {reservedFrom} تا {reservedUntil} رزرو است. لطفا زمانی خارج از این بازه انتخاب کنید.";
        }
    }
}
