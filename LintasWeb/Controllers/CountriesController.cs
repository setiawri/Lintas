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
    public class CountriesController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Countries
        public async Task<ActionResult> Index()
        {
            return View(await db.Countries.OrderBy(x => x.Name).ToListAsync());
        }

        // GET: Countries/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Countries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Notes")] CountriesModels countriesModels)
        {
            if (ModelState.IsValid)
            {
                countriesModels.Id = Guid.NewGuid();
                db.Countries.Add(countriesModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(countriesModels);
        }

        // GET: Countries/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CountriesModels countriesModels = await db.Countries.FindAsync(id);
            if (countriesModels == null)
            {
                return HttpNotFound();
            }
            return View(countriesModels);
        }

        // POST: Countries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Notes")] CountriesModels countriesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(countriesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(countriesModels);
        }

        // GET: Countries/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CountriesModels countriesModels = await db.Countries.FindAsync(id);
            if (countriesModels == null)
            {
                return HttpNotFound();
            }
            return View(countriesModels);
        }

        // POST: Countries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            CountriesModels countriesModels = await db.Countries.FindAsync(id);
            db.Countries.Remove(countriesModels);
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
