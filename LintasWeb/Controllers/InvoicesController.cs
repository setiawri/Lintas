using LintasMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Invoices
        public async Task<ActionResult> Index()
        {
            var data = (from i in db.Invoices
                        join o in db.Orders on i.Orders_Id equals o.Id
                        join c in db.Customers on o.Customers_Id equals c.Id
                        select new InvoicesIndexViewModels
                        {
                            Id = i.Id,
                            Timestamp = i.Timestamp,
                            No = i.No.Substring(0,11),
                            Customer = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                            Amount = i.TotalAmount,
                            Paid = i.TotalPaid,
                            Notes = i.Notes
                        }).ToListAsync();
            return View(await data);
        }

        public ActionResult Create()
        {
            var orders = (from o in db.Orders
                          join c in db.Customers on o.Customers_Id equals c.Id
                          where o.Status_enumid == OrderStatusEnum.Ordered
                          orderby c.FirstName
                          select new { o, c }).ToList();
            List<object> newList = new List<object>();
            foreach (var order in orders)
            {
                string no_order = order.o.Timestamp.ToString("yyyyMMdd") + order.o.No;
                var fName = order.c.FirstName;
                var mName = order.c.MiddleName;
                var lName = order.c.LastName;
                string fullName = fName;
                if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
                if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

                newList.Add(new
                {
                    Id = order.o.Id,
                    Name = fullName + " (" + no_order + ")"
                });
            }

            ViewBag.listOrders = new SelectList(newList, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Orders_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items)
        {
            if (ModelState.IsValid)
            {
                OrdersModels order = db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefault();
                int counter = db.Invoices.AsNoTracking().Where(x => x.Orders_Id == invoicesModels.Orders_Id).Count();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.No = order.Timestamp.ToString("yyyyMMdd") + order.No + counter;
                db.Invoices.Add(invoicesModels);

                List<OrderItemDetails> lInvoiceItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(Items);
                foreach (var item in lInvoiceItem)
                {
                    InvoiceItemsModels ii = new InvoiceItemsModels();
                    ii.Id = Guid.NewGuid();
                    ii.Invoices_Id = invoicesModels.Id;
                    ii.Description = item.desc;
                    ii.Amount = item.cost;
                    ii.Notes = item.note;
                    db.InvoiceItems.Add(ii);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var orders = (from o in db.Orders
                          join c in db.Customers on o.Customers_Id equals c.Id
                          where o.Status_enumid == OrderStatusEnum.Ordered
                          orderby c.FirstName
                          select new { o, c }).ToList();
            List<object> newList = new List<object>();
            foreach (var order in orders)
            {
                string no_order = order.o.Timestamp.ToString("yyyyMMdd") + order.o.No;
                var fName = order.c.FirstName;
                var mName = order.c.MiddleName;
                var lName = order.c.LastName;
                string fullName = fName;
                if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
                if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

                newList.Add(new
                {
                    Id = order.o.Id,
                    Name = fullName + " (" + no_order + ")"
                });
            }

            ViewBag.listOrders = new SelectList(newList, "Id", "Name");
            return View(invoicesModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            InvoicesModels invoicesModels = await db.Invoices.FindAsync(id);
            
            var orders = (from o in db.Orders
                          join c in db.Customers on o.Customers_Id equals c.Id
                          where o.Status_enumid == OrderStatusEnum.Ordered
                          orderby c.FirstName
                          select new { o, c }).ToList();
            List<object> newList = new List<object>();
            foreach (var order in orders)
            {
                string no_order = order.o.Timestamp.ToString("yyyyMMdd") + order.o.No;
                var fName = order.c.FirstName;
                var mName = order.c.MiddleName;
                var lName = order.c.LastName;
                string fullName = fName;
                if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
                if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

                newList.Add(new
                {
                    Id = order.o.Id,
                    Name = fullName + " (" + no_order + ")"
                });
            }

            ViewBag.listOrders = new SelectList(newList, "Id", "Name");
            ViewBag.listItems = db.InvoiceItems.Where(x => x.Invoices_Id == id).ToList();
            return View(invoicesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Orders_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items)
        {
            if (ModelState.IsValid)
            {
                List<InvoiceItemsModels> lInvoiceItem_before = db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList();
                foreach (var item in lInvoiceItem_before)
                {
                    db.InvoiceItems.Remove(item);
                }

                db.Entry(invoicesModels).State = EntityState.Modified;

                List<OrderItemDetails> lInvoiceItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(Items);
                foreach (var item in lInvoiceItem)
                {
                    InvoiceItemsModels ii = new InvoiceItemsModels();
                    ii.Id = Guid.NewGuid();
                    ii.Invoices_Id = invoicesModels.Id;
                    ii.Description = item.desc;
                    ii.Amount = item.cost;
                    ii.Notes = item.note;
                    db.InvoiceItems.Add(ii);
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var orders = (from o in db.Orders
                          join c in db.Customers on o.Customers_Id equals c.Id
                          where o.Status_enumid == OrderStatusEnum.Ordered
                          orderby c.FirstName
                          select new { o, c }).ToList();
            List<object> newList = new List<object>();
            foreach (var order in orders)
            {
                string no_order = order.o.Timestamp.ToString("yyyyMMdd") + order.o.No;
                var fName = order.c.FirstName;
                var mName = order.c.MiddleName;
                var lName = order.c.LastName;
                string fullName = fName;
                if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
                if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

                newList.Add(new
                {
                    Id = order.o.Id,
                    Name = fullName + " (" + no_order + ")"
                });
            }

            ViewBag.listOrders = new SelectList(newList, "Id", "Name");
            ViewBag.listItems = db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList();
            return View(invoicesModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from i in db.Invoices
                        join o in db.Orders on i.Orders_Id equals o.Id
                        join c in db.Customers on o.Customers_Id equals c.Id
                        where i.Id == id
                        select new InvoicesIndexViewModels
                        {
                            Id = i.Id,
                            Timestamp = i.Timestamp,
                            No = i.No.Substring(0, 11),
                            Customer = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                            Amount = i.TotalAmount,
                            Paid = i.TotalPaid,
                            Notes = i.Notes
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            List<InvoiceItemsModels> lInvoiceItem = await db.InvoiceItems.Where(x => x.Invoices_Id == id).ToListAsync();
            foreach (var item in lInvoiceItem)
            {
                db.InvoiceItems.Remove(item);
            }
            InvoicesModels invoicesModels = await db.Invoices.FindAsync(id);
            db.Invoices.Remove(invoicesModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}