using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class Works1Controller : Controller
    {
        private readonly AgileRap_Process_Software_Context _context;

        public Works1Controller(AgileRap_Process_Software_Context context)
        {
            _context = context;
        }

        // GET: Works1
        public async Task<IActionResult> Index()
        {
            var agileRap_Process_Software_Context = _context.Work.Include(w => w.Status);
            return View(await agileRap_Process_Software_Context.ToListAsync());
        }

        // GET: Works1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var work = await _context.Work
                .Include(w => w.Status)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (work == null)
            {
                return NotFound();
            }

            return View(work);
        }

        // GET: Works1/Create
        public IActionResult Create()
        {
            ViewData["StatusID"] = new SelectList(_context.Status, "ID", "ID");
            return View();
        }

        // POST: Works1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,HeaderID,Project,Name,DueDate,StatusID,Remark,CreateBy,CreateDate,UpdateBy,UpdateDate,IsDelete")] Work work)
        {
            if (ModelState.IsValid)
            {
                _context.Add(work);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusID"] = new SelectList(_context.Status, "ID", "ID", work.StatusID);
            return View(work);
        }

        // GET: Works1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var work = await _context.Work.FindAsync(id);
            if (work == null)
            {
                return NotFound();
            }
            ViewData["StatusID"] = new SelectList(_context.Status, "ID", "ID", work.StatusID);
            return View(work);
        }

        // POST: Works1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,HeaderID,Project,Name,DueDate,StatusID,Remark,CreateBy,CreateDate,UpdateBy,UpdateDate,IsDelete")] Work work)
        {
            if (id != work.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(work);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkExists(work.ID))
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
            ViewData["StatusID"] = new SelectList(_context.Status, "ID", "ID", work.StatusID);
            return View(work);
        }

        // GET: Works1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var work = await _context.Work
                .Include(w => w.Status)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (work == null)
            {
                return NotFound();
            }

            return View(work);
        }

        // POST: Works1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var work = await _context.Work.FindAsync(id);
            if (work != null)
            {
                _context.Work.Remove(work);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkExists(int id)
        {
            return _context.Work.Any(e => e.ID == id);
        }
    }
}
