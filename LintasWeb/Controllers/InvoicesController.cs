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

        public JsonResult GetItems(Guid id)
        {
            List<InvoiceItemsModels> items = db.InvoiceItems.Where(x => x.Invoices_Id == id).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Description</th>
                                                <th>Amount</th>
                                                <th>Notes</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (InvoiceItemsModels item in items)
            {
                message += @"<tr>
                                <td>" + item.Description + @"</td>
                                <td>" + item.Amount.ToString("#,##0.00") + @"</td>
                                <td>" + item.Notes + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        // GET: Invoices
        public async Task<ActionResult> Index()
        {
            var data = (from i in db.Invoices
                        join o in db.Orders on i.Ref_Id equals o.Id
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
        public async Task<ActionResult> Create([Bind(Include = "Id,Orders_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items, bool ItemValid)
        {
            if (!ItemValid)
            {
                ModelState.AddModelError("Items", "The Invoice Item Description and Amount field is required.");
            }

            if (ModelState.IsValid)
            {
                OrdersModels order = db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefault();
                int counter = db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id).Count();
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
                          where o.Id == invoicesModels.Ref_Id
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
        public async Task<ActionResult> Edit([Bind(Include = "Id,Orders_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items, bool ItemValid)
        {
            if (!ItemValid)
            {
                ModelState.AddModelError("Items", "The Invoice Item Description and Amount field is required.");
            }

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
                          where o.Id == invoicesModels.Ref_Id
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

        public async Task<ActionResult> Payment(Guid? id)
        {
            InvoicesModels invoicesModels = await db.Invoices.FindAsync(id);
            ViewBag.InvoiceId = id;
            ViewBag.Invoice = invoicesModels.No + " - " + invoicesModels.Notes + " (" + string.Format("{0:N2}", invoicesModels.TotalAmount) + ")";

            PaymentsModels paymentsModels = new PaymentsModels();
            return View(paymentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment([Bind(Include = "Id,Invoices_Id,Timestamp,Amount,PaymentInfo,Notes")] PaymentsModels paymentsModels)
        {
            var invoice = db.Invoices.AsNoTracking().Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefault();
            if (invoice != null)
            {
                decimal remaining = invoice.TotalAmount - invoice.TotalPaid;
                if (paymentsModels.Amount > remaining)
                {
                    ModelState.AddModelError("Max", "The Maximum payment is " + string.Format("{0:N2}", remaining));
                }
            }

            if (ModelState.IsValid)
            {
                paymentsModels.Id = Guid.NewGuid();
                db.Payments.Add(paymentsModels);

                invoice.TotalPaid += paymentsModels.Amount;
                db.Entry(invoice).State = System.Data.Entity.EntityState.Modified;

                if (invoice.TotalPaid == invoice.TotalAmount)
                {
                    OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoice.Ref_Id).FirstOrDefaultAsync();
                    ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted;
                    db.Entry(ordersModels).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewBag.InvoiceId = paymentsModels.Invoices_Id;
            ViewBag.Invoice = invoice.No + " - " + invoice.Notes + " (" + string.Format("{0:N2}", invoice.TotalAmount) + ")";
            return View(paymentsModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from i in db.Invoices
                        join o in db.Orders on i.Ref_Id equals o.Id
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

            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
            List<InvoicesModels> invoices = await db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id && x.Id != id).ToListAsync();
            if (invoices.Count == 0)
            {
                ordersModels.Status_enumid = OrderStatusEnum.Ordered;
            }
            else
            {
                decimal total_amount = 0; decimal total_paid = 0;
                foreach(InvoicesModels invoice in invoices)
                {
                    total_amount += invoice.TotalAmount;
                    total_paid += invoice.TotalPaid;
                }

                if (total_amount == total_paid) { ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted; }
                else { ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment; }
            }
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}