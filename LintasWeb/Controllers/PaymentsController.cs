using LintasMVC.Models;
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
    public class PaymentsController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Payments
        public ActionResult Index()
        {
            var data = (from pay in db.Payments
                        join i in db.Invoices on pay.Invoices_Id equals i.Id
                        select new PaymentsIndexViewModels
                        {
                            Id = pay.Id,
                            Timestamp = pay.Timestamp,
                            InvoiceNo = i.No,
                            Amount = pay.Amount,
                            Info = pay.PaymentInfo,
                            Notes = pay.Notes
                        }).ToList();
            return View(data);
        }

        public ActionResult Create()
        {
            var invoices = db.Invoices.Where(x => x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToList();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ")" 
                });
            }

            ViewBag.listInvoice = new SelectList(newList, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Invoices_Id,Timestamp,Amount,PaymentInfo,Notes")] PaymentsModels paymentsModels)
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

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var invoices = db.Invoices.Where(x => x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToList();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ")"
                });
            }

            ViewBag.listInvoice = new SelectList(newList, "Id", "Name");
            return View(paymentsModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            PaymentsModels paymentsModels = await db.Payments.FindAsync(id);

            var invoices = db.Invoices.Where(x => x.Id == paymentsModels.Invoices_Id).ToList();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ")"
                });
            }

            ViewBag.listInvoice = new SelectList(newList, "Id", "Name");
            return View(paymentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Invoices_Id,Timestamp,Amount,PaymentInfo,Notes")] PaymentsModels paymentsModels)
        {
            decimal payment_before = db.Payments.AsNoTracking().Where(x => x.Id == paymentsModels.Id).FirstOrDefault().Amount;
            var invoice = db.Invoices.AsNoTracking().Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefault();
            if (invoice != null)
            {
                decimal remaining = invoice.TotalAmount - invoice.TotalPaid + payment_before;
                if (paymentsModels.Amount > remaining)
                {
                    ModelState.AddModelError("Max", "The Maximum payment is " + string.Format("{0:N2}", remaining));
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(paymentsModels).State = System.Data.Entity.EntityState.Modified;
                
                invoice.TotalPaid = (invoice.TotalPaid - payment_before) + paymentsModels.Amount;
                db.Entry(invoice).State = System.Data.Entity.EntityState.Modified;

                await db.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var invoices = db.Invoices.Where(x => x.Id == paymentsModels.Invoices_Id).ToList();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ")"
                });
            }

            ViewBag.listInvoice = new SelectList(newList, "Id", "Name");
            return View(paymentsModels);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from pay in db.Payments
                        join i in db.Invoices on pay.Invoices_Id equals i.Id
                        where pay.Id == id
                        select new PaymentsIndexViewModels
                        {
                            Id = pay.Id,
                            Timestamp = pay.Timestamp,
                            InvoiceNo = i.No,
                            Amount = pay.Amount,
                            Info = pay.PaymentInfo,
                            Notes = pay.Notes
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            PaymentsModels paymentsModels = await db.Payments.FindAsync(id);
            db.Payments.Remove(paymentsModels);

            InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefaultAsync();
            invoicesModels.TotalPaid -= paymentsModels.Amount;
            db.Entry(invoicesModels).State = EntityState.Modified;

            OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
            ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}