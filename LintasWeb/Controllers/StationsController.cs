using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LintasMVC.Models;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class StationsController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Stations
        public async Task<ActionResult> Index()
        {
            var data = (from s in db.Stations
                        join c in db.Countries on s.Countries_Id equals c.Id
                        select new StationsIndexViewModels
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Countries = c.Name,
                            Address = s.Address
                        }).ToListAsync();
            return View(await data);
        }

        // GET: Stations/Create
        public ActionResult Create()
        {
            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        // POST: Stations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Countries_Id,Address,Phone1,Phone2,Notes")] StationsModels stationsModels)
        {
            if (ModelState.IsValid)
            {
                stationsModels.Id = Guid.NewGuid();
                db.Stations.Add(stationsModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(stationsModels);
        }

        // GET: Stations/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StationsModels stationsModels = await db.Stations.FindAsync(id);
            if (stationsModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(stationsModels);
        }

        // POST: Stations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Countries_Id,Address,Phone1,Phone2,Notes")] StationsModels stationsModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stationsModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(stationsModels);
        }

        // GET: Stations/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from s in db.Stations
                        join c in db.Countries on s.Countries_Id equals c.Id
                        where s.Id == id
                        select new StationsIndexViewModels
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Countries = c.Name,
                            Address = s.Address
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        // POST: Stations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            StationsModels stationsModels = await db.Stations.FindAsync(id);
            db.Stations.Remove(stationsModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
