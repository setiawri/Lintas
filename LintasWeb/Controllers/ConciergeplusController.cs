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
    public class ConciergeplusController : Controller
    {
        private LintasContext db = new LintasContext();

        public JsonResult GetOrderItems(Guid id)
        {
            List<OrderItemsModels> items = db.OrderItems.Where(x => x.Orders_Id == id).ToList();
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
            foreach (OrderItemsModels item in items)
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

        public JsonResult GetInvoiceItems(Guid id)
        {
            var invoice = db.Invoices.Where(x => x.Orders_Id == id).FirstOrDefault();
            List<InvoiceItemsModels> items = db.InvoiceItems.Where(x => x.Invoices_Id == invoice.Id).ToList();
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

        // GET: Conciergeplus
        public async Task<ActionResult> Index()
        {
            var data = (from o in db.Orders
                        join c in db.Customers on o.Customers_Id equals c.Id
                        join os in db.Stations on o.Origin_Stations_Id equals os.Id
                        join ds in db.Stations on o.Destination_Stations_Id equals ds.Id
                        select new OrdersIndexViewModels
                        {
                            Id = o.Id,
                            No = o.No,
                            Timestamp = o.Timestamp,
                            Customer = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                            Origin = os.Name,
                            Destination = ds.Name,
                            Notes = o.Notes,
                            Status = o.Status_enumid
                        }).ToListAsync();
            return View(await data);
        }

        public async Task<ActionResult> Create(Guid? id)
        {
            List<CustomersModels> customers = await db.Customers.OrderBy(x => x.FirstName).ToListAsync();
            List<object> newList = new List<object>();
            foreach (var customer in customers)
            {
                var fName = customer.FirstName;
                var mName = customer.MiddleName;
                var lName = customer.LastName;
                string fullName = fName;
                if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
                if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

                newList.Add(new
                {
                    Id = customer.Id,
                    Name = fullName
                });
            }

            ViewBag.listCustomers = new SelectList(newList, "Id", "Name");
            ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");

            OrdersModels ordersModels;
            InvoicesModels invoicesModels;
            PaymentsModels paymentsModels;
            ConciergeplusModels conciergeplusModels = new ConciergeplusModels();
            if (id == null || id == Guid.Empty)
            {
                ordersModels = new OrdersModels();
                invoicesModels = new InvoicesModels();
                paymentsModels = new PaymentsModels();
                ViewBag.startIndex = 0;
            }
            else
            {
                ordersModels = await db.Orders.Where(x => x.Id == id).FirstOrDefaultAsync();
                ViewBag.listItems = db.OrderItems.Where(x => x.Orders_Id == id).ToList();

                invoicesModels = await db.Invoices.Where(x => x.Orders_Id == id).FirstOrDefaultAsync();
                ViewBag.listInv = invoicesModels != null ? db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList() : null;

                paymentsModels = invoicesModels == null ? null : await db.Payments.Where(x => x.Invoices_Id == invoicesModels.Id).FirstOrDefaultAsync();

                ViewBag.startIndex = (int)ordersModels.Status_enumid + 1;
            }
            conciergeplusModels.Order = ordersModels;
            conciergeplusModels.Invoice = invoicesModels;
            conciergeplusModels.Payment = paymentsModels;

            return View(conciergeplusModels);
        }

        public async Task<JsonResult> SaveOrder(Guid? order_id, DateTime order_date, Guid customer_id, Guid origin_id, Guid destination_id, string notes, string order_items)
        {
            OrdersModels ordersModels;

            if (order_id == null || order_id == Guid.Empty)
            {
                ordersModels = new OrdersModels();
                Common.Master m = new Common.Master();
                ordersModels.Id = Guid.NewGuid();
                ordersModels.No = m.GetLastNo(ordersModels.Timestamp).ToString("000");
                ordersModels.Timestamp = order_date;
                ordersModels.Customers_Id = customer_id;
                ordersModels.Origin_Stations_Id = origin_id;
                ordersModels.Destination_Stations_Id = destination_id;
                ordersModels.Notes = notes;
                ordersModels.Status_enumid = OrderStatusEnum.Ordered;
                db.Orders.Add(ordersModels);

                List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(order_items);
                foreach (var item in lOrderItem)
                {
                    OrderItemsModels oi = new OrderItemsModels();
                    oi.Id = Guid.NewGuid();
                    oi.Orders_Id = ordersModels.Id;
                    oi.Description = item.desc;
                    oi.Amount = item.cost;
                    oi.Notes = item.note;
                    db.OrderItems.Add(oi);
                }
            }
            else
            {
                ordersModels = await db.Orders.FindAsync(order_id);

                List<OrderItemsModels> lOrderItem_before = db.OrderItems.Where(x => x.Orders_Id == ordersModels.Id).ToList();
                foreach (var item in lOrderItem_before)
                {
                    db.OrderItems.Remove(item);
                }

                ordersModels.Customers_Id = customer_id;
                ordersModels.Origin_Stations_Id = origin_id;
                ordersModels.Destination_Stations_Id = destination_id;
                ordersModels.Notes = notes;
                db.Entry(ordersModels).State = EntityState.Modified;

                List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(order_items);
                foreach (var item in lOrderItem)
                {
                    OrderItemsModels oi = new OrderItemsModels();
                    oi.Id = Guid.NewGuid();
                    oi.Orders_Id = ordersModels.Id;
                    oi.Description = item.desc;
                    oi.Amount = item.cost;
                    oi.Notes = item.note;
                    db.OrderItems.Add(oi);
                }
            }

            await db.SaveChangesAsync();

            return Json(new { id = ordersModels.Id, no_order = order_date.ToString("yyyyMMdd") + ordersModels.No }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SaveInvoice(Guid order_id, DateTime inv_date, decimal total_amount, string notes, string inv_items)
        {
            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == order_id).FirstOrDefaultAsync();
            InvoicesModels invoicesModels;

            if (ordersModels.Status_enumid == OrderStatusEnum.Ordered)
            {
                invoicesModels = new InvoicesModels();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.Orders_Id = order_id;
                int counter = db.Invoices.AsNoTracking().Where(x => x.Orders_Id == invoicesModels.Orders_Id).Count();
                invoicesModels.No = ordersModels.Timestamp.ToString("yyyyMMdd") + ordersModels.No + counter;
                invoicesModels.Timestamp = inv_date;
                invoicesModels.TotalAmount = total_amount;
                invoicesModels.TotalPaid = 0;
                invoicesModels.Notes = notes;
                db.Invoices.Add(invoicesModels);

                List<OrderItemDetails> lInvoiceItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(inv_items);
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

                ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
                db.Entry(ordersModels).State = EntityState.Modified;
            }
            else
            {
                invoicesModels = await db.Invoices.Where(x => x.Orders_Id == order_id).FirstOrDefaultAsync();

                List<InvoiceItemsModels> lInvoiceItem_before = db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList();
                foreach (var item in lInvoiceItem_before)
                {
                    db.InvoiceItems.Remove(item);
                }

                invoicesModels.TotalAmount = total_amount;
                invoicesModels.Notes = notes;
                db.Entry(invoicesModels).State = EntityState.Modified;

                List<OrderItemDetails> lInvoiceItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(inv_items);
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
            }

            await db.SaveChangesAsync();

            return Json(new { invoice_id = invoicesModels.Id }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SavePayment(Guid inv_id, DateTime pay_date, decimal amount, string info, string notes)
        {
            PaymentsModels paymentsModels = new PaymentsModels();
            paymentsModels.Id = Guid.NewGuid();
            paymentsModels.Invoices_Id = inv_id;
            paymentsModels.Timestamp = pay_date;
            paymentsModels.Amount = amount;
            paymentsModels.PaymentInfo = info;
            paymentsModels.Notes = notes;
            db.Payments.Add(paymentsModels);

            InvoicesModels invoicesModels = await db.Invoices.AsNoTracking().Where(x => x.Id == inv_id).FirstOrDefaultAsync();
            invoicesModels.TotalPaid = amount;
            db.Entry(invoicesModels).State = EntityState.Modified;

            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
            ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted;
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return Json(new { RedirectUrl = Url.Action("Index", "Conciergeplus") }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from o in db.Orders
                        join c in db.Customers on o.Customers_Id equals c.Id
                        join os in db.Stations on o.Origin_Stations_Id equals os.Id
                        join ds in db.Stations on o.Destination_Stations_Id equals ds.Id
                        where o.Id == id
                        select new OrdersIndexViewModels
                        {
                            Id = o.Id,
                            No = o.No,
                            Timestamp = o.Timestamp,
                            Customer = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                            Origin = os.Name,
                            Destination = ds.Name,
                            Notes = o.Notes
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            OrdersModels ordersModels = await db.Orders.FindAsync(id);
            db.Orders.Remove(ordersModels);

            List<OrderItemsModels> listOrder = await db.OrderItems.Where(x => x.Orders_Id == ordersModels.Id).ToListAsync();
            foreach (var item in listOrder)
            {
                db.OrderItems.Remove(item);
            }

            InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Orders_Id == ordersModels.Id).FirstOrDefaultAsync();
            db.Invoices.Remove(invoicesModels);

            List<InvoiceItemsModels> listInvoice = await db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToListAsync();
            foreach (var item in listInvoice)
            {
                db.InvoiceItems.Remove(item);
            }

            PaymentsModels paymentsModels = await db.Payments.Where(x => x.Invoices_Id == invoicesModels.Id).FirstOrDefaultAsync();
            db.Payments.Remove(paymentsModels);

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}