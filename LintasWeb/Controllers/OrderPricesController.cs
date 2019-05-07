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
    public class OrderPricesController : Controller
    {
        private LintasContext db = new LintasContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.OrderPrices.OrderBy(x => x.Description).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Amount,Notes")] OrderPricesModels orderPricesModels)
        {
            if (ModelState.IsValid)
            {
                orderPricesModels.Id = Guid.NewGuid();
                db.OrderPrices.Add(orderPricesModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View(orderPricesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderPricesModels orderPricesModels = await db.OrderPrices.FindAsync(id);
            if (orderPricesModels == null)
            {
                return HttpNotFound();
            }
            return View(orderPricesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Amount,Notes")] OrderPricesModels orderPricesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderPricesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            
            return View(orderPricesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.OrderPrices.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            OrderPricesModels orderPricesModels = await db.OrderPrices.FindAsync(id);
            db.OrderPrices.Remove(orderPricesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}