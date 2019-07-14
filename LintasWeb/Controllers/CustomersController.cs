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
    public class CustomersController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Customers
        public async Task<ActionResult> Index()
        {
            var data = (from cst in db.Customers
                        join cty in db.Countries on cst.Countries_Id equals cty.Id
                        select new CustomersIndexViewModels
                        {
                            Id = cst.Id,
                            FirstName = cst.FirstName,
                            MiddleName = cst.MiddleName,
                            LastName = cst.LastName,
                            Address = cst.Address,
                            City = cst.City,
                            Countries = cty.Name
                        }).ToListAsync();
            return View(await data);
        }

        // GET: Customers/Create
        public ActionResult Create()
        {
            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,FirstName,MiddleName,LastName,Address,Address2,City,State,Phone1,Phone2,Zipcode,Countries_Id,Fax,Email,Notes")] CustomersModels customersModels)
        {
            if (ModelState.IsValid)
            {
                customersModels.Id = Guid.NewGuid();
                db.Customers.Add(customersModels);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(customersModels);
        }

        // GET: Customers/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomersModels customersModels = await db.Customers.FindAsync(id);
            if (customersModels == null)
            {
                return HttpNotFound();
            }
            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(customersModels);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,MiddleName,LastName,Address,Address2,City,State,Phone1,Phone2,Zipcode,Countries_Id,Fax,Email,Notes")] CustomersModels customersModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customersModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(customersModels);
        }

        // GET: Customers/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from cst in db.Customers
                        join cty in db.Countries on cst.Countries_Id equals cty.Id
                        where cst.Id == id
                        select new CustomersIndexViewModels
                        {
                            Id = cst.Id,
                            FirstName = cst.FirstName,
                            MiddleName = cst.MiddleName,
                            LastName = cst.LastName,
                            Address = cst.Address,
                            City = cst.City,
                            Countries = cty.Name
                        }).FirstOrDefaultAsync();

            return View(await data);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            CustomersModels customersModels = await db.Customers.FindAsync(id);
            db.Customers.Remove(customersModels);
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
