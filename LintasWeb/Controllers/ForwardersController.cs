using LintasMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class ForwardersController : Controller
    {
        private LintasContext db = new LintasContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.Forwarders.OrderBy(x => x.Name).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Address,Phone1,Phone2,Notes,Active")] ForwardersModels forwardersModels)
        {
            if (ModelState.IsValid)
            {
                forwardersModels.Id = Guid.NewGuid();
                forwardersModels.Active = true;
                db.Forwarders.Add(forwardersModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(forwardersModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ForwardersModels ForwardersModels = await db.Forwarders.FindAsync(id);
            if (ForwardersModels == null)
            {
                return HttpNotFound();
            }
            return View(ForwardersModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Address,Phone1,Phone2,Notes,Active")] ForwardersModels forwardersModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(forwardersModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(forwardersModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.Forwarders.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ForwardersModels forwardersModels = await db.Forwarders.FindAsync(id);
            db.Forwarders.Remove(forwardersModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}