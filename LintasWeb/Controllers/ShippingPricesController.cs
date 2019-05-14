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
    public class ShippingPricesController : Controller
    {
        private LintasContext db = new LintasContext();

        public async Task<ActionResult> Index()
        {
            return View(await db.ShippingPrices.OrderBy(x => x.Description).ToListAsync());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,Amount,Notes")] ShippingPricesModels shippingPricesModels)
        {
            if (ModelState.IsValid)
            {
                shippingPricesModels.Id = Guid.NewGuid();
                db.ShippingPrices.Add(shippingPricesModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(shippingPricesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShippingPricesModels shippingPricesModels = await db.ShippingPrices.FindAsync(id);
            if (shippingPricesModels == null)
            {
                return HttpNotFound();
            }
            return View(shippingPricesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Description,Amount,Notes")] ShippingPricesModels shippingPricesModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shippingPricesModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(shippingPricesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            return View(await db.ShippingPrices.Where(x => x.Id == id).FirstOrDefaultAsync());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ShippingPricesModels shippingPricesModels = await db.ShippingPrices.FindAsync(id);
            db.ShippingPrices.Remove(shippingPricesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}