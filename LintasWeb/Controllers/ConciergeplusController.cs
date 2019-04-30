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
                                                <th>Qty</th>
                                                <th>Amount</th>
                                                <th>Notes</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (OrderItemsModels item in items)
            {
                message += @"<tr>
                                <td>" + item.Description + @"</td>
                                <td>" + item.Qty.ToString("#,##0") + @"</td>
                                <td>" + item.Amount.ToString("#,##0.00") + @"</td>
                                <td>" + item.Notes + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInvoiceItems(Guid id)
        {
            var invoice = db.Invoices.Where(x => x.Id == id).FirstOrDefault();
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

        public JsonResult GetPayments(Guid id)
        {
            var payments = db.Payments.Where(x => x.Invoices_Id == id).ToList();
            var invoice = db.Invoices.Where(x => x.Id == id).FirstOrDefault();
            List<InvoiceItemsModels> items = db.InvoiceItems.Where(x => x.Invoices_Id == invoice.Id).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Date</th>
                                                <th>Amount</th>
                                                <th>Payment Info</th>
                                                <th>Notes</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (PaymentsModels item in payments)
            {
                message += @"<tr>
                                <td>" + item.Timestamp.ToString("yyyy/MM/dd") + @"</td>
                                <td>" + item.Amount.ToString("#,##0.00") + @"</td>
                                <td>" + item.PaymentInfo + @"</td>
                                <td>" + item.Notes + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLogs(Guid id)
        {
            var payments = db.Payments.Where(x => x.Invoices_Id == id).ToList();
            var invoice = db.Invoices.Where(x => x.Id == id).FirstOrDefault();
            List<OrderItemLogModels> itemLogs = db.OrderItemLog.Where(x => x.OrderItems_Id == id).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Timestamp</th>
                                                <th>Description</th>
                                                <th>Created By</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (OrderItemLogModels item in itemLogs)
            {
                message += @"<tr>
                                <td>" + item.Timestamp.ToString("yyyy/MM/dd") + @"</td>
                                <td>" + item.Description + @"</td>
                                <td>" + db.User.Where(x => x.Id == item.UserAccounts_Id).FirstOrDefault().Fullname + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPackage(Guid id)
        {
            var list = (from sic in db.ShippingItemContents
                        join oi in db.OrderItems on sic.OrderItems_Id equals oi.Id
                        where sic.ShippingItems_Id == id
                        select new { sic, oi }).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Item</th>
                                                <th>Qty</th>
                                                <th>Notes</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>" + item.oi.Description + @"</td>
                                <td>" + item.sic.Qty + @"</td>
                                <td>" + item.sic.Notes + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShippingItem(Guid id)
        {
            var list = db.ShippingItems.Where(x => x.Shippings_Id == id).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>No</th>
                                                <th>Dimension (cm)</th>
                                                <th>Weight (gr)</th>
                                                <th>Notes</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>" + item.No + @"</td>
                                <td>" + item.Length + " x " + item.Width + " x " + item.Height + @"</td>
                                <td>" + item.Weight.ToString("#,##0") + @"</td>
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
            List<InvoicesModels> listInvoice;
            List<PaymentsIndexViewModels> listPayment;
            InvoicesModels invoicesModels;
            PaymentsModels paymentsModels;
            List<OrderItemsModels> listOrderItem;
            List<ShippingItemsModels> listShippingItem;
            List<ShippingsViewModels> listShipping;
            ConciergeplusModels conciergeplusModels = new ConciergeplusModels();
            if (id == null || id == Guid.Empty)
            {
                ordersModels = new OrdersModels();
                listInvoice = new List<InvoicesModels>();
                listPayment = new List<PaymentsIndexViewModels>();
                invoicesModels = new InvoicesModels();
                paymentsModels = new PaymentsModels();
                listOrderItem = new List<OrderItemsModels>();
                listShippingItem = new List<ShippingItemsModels>();
                listShipping = new List<ShippingsViewModels>();
                ViewBag.startIndex = 0;
            }
            else
            {
                ordersModels = await db.Orders.Where(x => x.Id == id).FirstOrDefaultAsync();
                ViewBag.listItems = db.OrderItems.Where(x => x.Orders_Id == id).ToList();

                listInvoice = await db.Invoices.Where(x => x.Orders_Id == id).OrderBy(x => x.No).ToListAsync();
                listPayment = await (from pay in db.Payments
                                     join i in db.Invoices on pay.Invoices_Id equals i.Id
                                     where i.Orders_Id == id
                                     select new PaymentsIndexViewModels
                                     {
                                         Id = pay.Id,
                                         Timestamp = pay.Timestamp,
                                         InvoiceNo = i.No,
                                         Amount = pay.Amount,
                                         Info = pay.PaymentInfo,
                                         Notes = pay.Notes
                                     }).ToListAsync();

                invoicesModels = await db.Invoices.Where(x => x.Orders_Id == id).FirstOrDefaultAsync();
                ViewBag.listInv = invoicesModels != null ? db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList() : null;

                paymentsModels = invoicesModels == null ? null : await db.Payments.Where(x => x.Invoices_Id == invoicesModels.Id).FirstOrDefaultAsync();

                listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id).ToListAsync();

                //var list_si = await (from si in db.ShippingItems
                //                     join sic in db.ShippingItemContents on si.Id equals sic.ShippingItems_Id
                //                     join oi in db.OrderItems on sic.OrderItems_Id equals oi.Id
                //                     where oi.Orders_Id == id
                //                     select new { si }).ToListAsync();

                var list_si = await db.ShippingItems.OrderBy(x => x.No).ToListAsync();
                
                listShippingItem = new List<ShippingItemsModels>();
                foreach (var item in list_si)
                {
                    var shipping = await db.ShippingItemContents.Where(x => x.ShippingItems_Id == item.Id).FirstOrDefaultAsync();
                    var order = await (from o in db.Orders
                                       join oi in db.OrderItems on o.Id equals oi.Orders_Id
                                       where oi.Id == shipping.OrderItems_Id
                                       select new { o }).FirstOrDefaultAsync();

                    if (order.o.Id == id)
                    {
                        listShippingItem.Add(item);
                    }
                }

                //listShipping = await (from s in db.Shippings
                //                      join o in db.Stations on s.Origin_Stations_Id equals o.Id
                //                      join d in db.Stations on s.Destination_Stations_Id equals d.Id
                //                      select new ShippingsViewModels
                //                      {
                //                          Id = s.Id,
                //                          No = s.No,
                //                          Timestamp = s.Timestamp,
                //                          Origin = o.Name,
                //                          Destination = d.Name,
                //                          Notes = s.Notes
                //                      }).ToListAsync();

                var list_shipping = await db.Shippings.OrderBy(x => x.Timestamp).ToListAsync();
                listShipping = new List<ShippingsViewModels>();
                foreach (var item in list_shipping)
                {
                    Guid shipping_item_id = db.ShippingItems.Where(x => x.Shippings_Id == item.Id).FirstOrDefault().Id;
                    Guid order_item_id = db.ShippingItemContents.Where(x => x.ShippingItems_Id == shipping_item_id).FirstOrDefault().OrderItems_Id;
                    if (id == db.OrderItems.Where(x => x.Id == order_item_id).FirstOrDefault().Orders_Id)
                    {
                        ShippingsViewModels svm = new ShippingsViewModels();
                        svm.Id = item.Id;
                        svm.No = item.No;
                        svm.Timestamp = item.Timestamp;
                        svm.Origin = db.Stations.Where(x => x.Id == item.Origin_Stations_Id).FirstOrDefault().Name;
                        svm.Destination = db.Stations.Where(x => x.Id == item.Destination_Stations_Id).FirstOrDefault().Name;
                        svm.Notes = item.Notes;
                        listShipping.Add(svm);
                    }
                }

                ViewBag.startIndex = (int)ordersModels.Status_enumid + 1;
            }
            conciergeplusModels.Order = ordersModels;
            conciergeplusModels.ListInvoice = listInvoice;
            conciergeplusModels.ListPayment = listPayment;
            conciergeplusModels.Invoice = invoicesModels;
            conciergeplusModels.Payment = paymentsModels;
            conciergeplusModels.ListOrderItem = listOrderItem;
            conciergeplusModels.ListShippingItem = listShippingItem;
            conciergeplusModels.ListShipping = listShipping;

            return View(conciergeplusModels);
        }

        public async Task<JsonResult> SaveOrder(Guid? order_id, DateTime order_date, Guid customer_id, Guid origin_id, Guid destination_id, string notes, string order_items)
        {
            string status;
            OrdersModels ordersModels;

            if (order_id == null || order_id == Guid.Empty)
            {
                status = "new";
                ordersModels = new OrdersModels();
                Common.Master m = new Common.Master();
                ordersModels.Id = Guid.NewGuid();
                ordersModels.Timestamp = order_date;
                ordersModels.No = m.GetLastNo(ordersModels.Timestamp).ToString("000");
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
                    oi.Qty = item.qty;
                    oi.Amount = item.cost;
                    oi.Notes = item.note;
                    oi.Status_enumid = OrderItemStatusEnum.Pending;
                    db.OrderItems.Add(oi);
                }
            }
            else
            {
                status = "edit";
                ordersModels = await db.Orders.FindAsync(order_id);

                //List<OrderItemsModels> lOrderItem_before = db.OrderItems.Where(x => x.Orders_Id == ordersModels.Id).ToList();
                //foreach (var item in lOrderItem_before)
                //{
                //    db.OrderItems.Remove(item);
                //}

                //ordersModels.Customers_Id = customer_id;
                //ordersModels.Origin_Stations_Id = origin_id;
                //ordersModels.Destination_Stations_Id = destination_id;
                //ordersModels.Notes = notes;
                //db.Entry(ordersModels).State = EntityState.Modified;

                //List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(order_items);
                //foreach (var item in lOrderItem)
                //{
                //    OrderItemsModels oi = new OrderItemsModels();
                //    oi.Id = Guid.NewGuid();
                //    oi.Orders_Id = ordersModels.Id;
                //    oi.Description = item.desc;
                //    oi.Qty = item.qty;
                //    oi.Amount = item.cost;
                //    oi.Notes = item.note;
                //    oi.Status_enumid = OrderItemStatusEnum.Pending;
                //    db.OrderItems.Add(oi);
                //}
            }

            await db.SaveChangesAsync();

            return Json(new { status = status, id = ordersModels.Id, no_order = order_date.ToString("yyyyMMdd") + ordersModels.No }, JsonRequestBehavior.AllowGet);
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
            if (invoicesModels != null)
            {
                db.Invoices.Remove(invoicesModels);

                List<InvoiceItemsModels> listInvoice = await db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToListAsync();
                foreach (var item in listInvoice)
                {
                    db.InvoiceItems.Remove(item);
                }
                
                List<PaymentsModels> listPayment = await db.Payments.Where(x => x.Invoices_Id == invoicesModels.Id).ToListAsync();
                if (listPayment.Count > 0)
                {
                    foreach(var item in listPayment)
                    {
                        db.Payments.Remove(item);
                    }
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Invoice(Guid? id)
        {
            var order = await (from o in db.Orders
                               join c in db.Customers on o.Customers_Id equals c.Id
                               where o.Status_enumid != OrderStatusEnum.Completed && o.Id == id
                               select new { o, c }).FirstOrDefaultAsync();
            string fullName = order.c.FirstName;
            if (!string.IsNullOrEmpty(order.c.MiddleName)) { fullName += " " + order.c.MiddleName; }
            if (!string.IsNullOrEmpty(order.c.LastName)) { fullName += " " + order.c.LastName; }

            ViewBag.OrderId = id;
            ViewBag.Order = fullName + " (" + order.o.Timestamp.ToString("yyyyMMdd") + order.o.No + ")";

            InvoicesModels invoicesModels = new InvoicesModels();
            return View(invoicesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Invoice([Bind(Include = "Id,Orders_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items, bool ItemValid)
        {
            if (!ItemValid)
            {
                ModelState.AddModelError("Items", "The Invoice Item Description and Amount field is required.");
            }

            if (ModelState.IsValid)
            {
                OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
                int counter = db.Invoices.AsNoTracking().Where(x => x.Orders_Id == invoicesModels.Orders_Id).Count();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.No = ordersModels.Timestamp.ToString("yyyyMMdd") + ordersModels.No + counter;
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

                ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Orders_Id });
            }

            var order = await (from o in db.Orders
                               join c in db.Customers on o.Customers_Id equals c.Id
                               where o.Status_enumid != OrderStatusEnum.Completed && o.Id == invoicesModels.Orders_Id
                               select new { o, c }).FirstOrDefaultAsync();
            string fullName = order.c.FirstName;
            if (!string.IsNullOrEmpty(order.c.MiddleName)) { fullName += " " + order.c.MiddleName; }
            if (!string.IsNullOrEmpty(order.c.LastName)) { fullName += " " + order.c.LastName; }

            ViewBag.OrderId = invoicesModels.Orders_Id;
            ViewBag.Order = fullName + " (" + order.o.Timestamp.ToString("yyyyMMdd") + order.o.No + ")";
            return View(invoicesModels);
        }

        public async Task<ActionResult> DeleteInvoice(Guid? id)
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

        [HttpPost, ActionName("DeleteInvoice")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteInvoiceConfirmed(Guid id)
        {
            List<InvoiceItemsModels> lInvoiceItem = await db.InvoiceItems.Where(x => x.Invoices_Id == id).ToListAsync();
            foreach (var item in lInvoiceItem)
            {
                db.InvoiceItems.Remove(item);
            }

            InvoicesModels invoicesModels = await db.Invoices.FindAsync(id);
            db.Invoices.Remove(invoicesModels);

            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
            List<InvoicesModels> invoices = await db.Invoices.AsNoTracking().Where(x => x.Orders_Id == invoicesModels.Orders_Id && x.Id != id).ToListAsync();
            if (invoices.Count == 0)
            {
                ordersModels.Status_enumid = OrderStatusEnum.Ordered;
            }
            else
            {
                decimal total_amount = 0; decimal total_paid = 0;
                foreach (InvoicesModels invoice in invoices)
                {
                    total_amount += invoice.TotalAmount;
                    total_paid += invoice.TotalPaid;
                }

                if (total_amount == total_paid) { ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted; }
                else { ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment; }
            }
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Orders_Id });
        }

        public async Task<ActionResult> Payment(Guid? id)
        {
            var invoices = await db.Invoices.Where(x => x.Orders_Id == id && x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToListAsync();
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
            ViewBag.OrderId = id;
            return View();

            //InvoicesModels invoicesModels = await db.Invoices.FindAsync(id);
            //ViewBag.InvoiceId = id;
            //ViewBag.Invoice = invoicesModels.No + " - " + invoicesModels.Notes + " (" + string.Format("{0:N2}", invoicesModels.TotalAmount) + ")";

            //PaymentsModels paymentsModels = new PaymentsModels();
            //return View(paymentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment([Bind(Include = "Id,Invoices_Id,Timestamp,Amount,PaymentInfo,Notes")] PaymentsModels paymentsModels, Guid Order_Id)
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

                decimal sum_invoice_amount = db.Invoices.Where(x => x.Orders_Id == invoice.Orders_Id).Sum(x => x.TotalAmount);
                decimal sum_invoice_paid = db.Invoices.Where(x => x.Orders_Id == invoice.Orders_Id).Sum(x => x.TotalPaid) + paymentsModels.Amount;

                if (sum_invoice_amount == sum_invoice_paid)
                {
                    OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoice.Orders_Id).FirstOrDefaultAsync();
                    ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted;
                    db.Entry(ordersModels).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Conciergeplus", new { id = invoice.Orders_Id });
            }

            //ViewBag.InvoiceId = paymentsModels.Invoices_Id;
            //ViewBag.Invoice = invoice.No + " - " + invoice.Notes + " (" + string.Format("{0:N2}", invoice.TotalAmount) + ")";
            
            var invoices = await db.Invoices.Where(x => x.Orders_Id == Order_Id && x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToListAsync();
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
            ViewBag.OrderId = Order_Id;
            return View(paymentsModels);
        }

        public async Task<ActionResult> DeletePayment(Guid? id)
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
            //var data = (from pay in db.Payments
            //            join i in db.Invoices on pay.Invoices_Id equals i.Id
            //            where pay.Invoices_Id == id
            //            select new PaymentsIndexViewModels
            //            {
            //                Id = pay.Id,
            //                Timestamp = pay.Timestamp,
            //                InvoiceNo = i.No,
            //                Amount = pay.Amount,
            //                Info = pay.PaymentInfo,
            //                Notes = pay.Notes
            //            }).ToListAsync();
            //return View(await data);
        }

        [HttpPost, ActionName("DeletePayment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePaymentConfirmed(Guid id)
        {
            //List<PaymentsModels> listPayment = await db.Payments.Where(x => x.Invoices_Id == id).ToListAsync();
            //foreach (var item in listPayment)
            //{
            //    db.Payments.Remove(item);
            //}

            //InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Id == id).FirstOrDefaultAsync();
            //invoicesModels.TotalPaid = 0;
            //db.Entry(invoicesModels).State = EntityState.Modified;

            //OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
            //ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
            //db.Entry(ordersModels).State = EntityState.Modified;

            PaymentsModels paymentsModels = await db.Payments.FindAsync(id);
            db.Payments.Remove(paymentsModels);

            InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefaultAsync();
            invoicesModels.TotalPaid -= paymentsModels.Amount;
            db.Entry(invoicesModels).State = EntityState.Modified;

            OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoicesModels.Orders_Id).FirstOrDefaultAsync();
            ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Orders_Id });
        }

        public async Task<ActionResult> OrderItemLog(Guid? id)
        {
            OrderItemsModels orderItemsModels = await db.OrderItems.FindAsync(id);
            List<OrderItemLogModels> listOrderItemLog = await db.OrderItemLog.Where(x => x.OrderItems_Id == orderItemsModels.Id).OrderByDescending(x => x.Timestamp).ToListAsync();
            OrderItemLogViewModels orderItemLogViewModels = new OrderItemLogViewModels();
            orderItemLogViewModels.OrderItem = orderItemsModels;
            orderItemLogViewModels.ListOrderItemLog = listOrderItemLog;
            return View(orderItemLogViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OrderItemLog([Bind(Include = "OrderItem")] OrderItemLogViewModels orderItemLogViewModels, string DescriptionLog)
        {
            OrderItemsModels orderItemsModels = await db.OrderItems.AsNoTracking().Where(x => x.Id == orderItemLogViewModels.OrderItem.Id).FirstOrDefaultAsync();
            if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Purchased)
            {
                orderItemsModels.PurchaseTimestamp = DateTime.Now;
            }
            else if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Received)
            {
                orderItemsModels.ReceiveTimestamp = DateTime.Now;
            }
            orderItemsModels.Status_enumid = orderItemLogViewModels.OrderItem.Status_enumid;
            db.Entry(orderItemsModels).State = EntityState.Modified;

            OrderItemLogModels orderItemLogModels = new OrderItemLogModels();
            orderItemLogModels.Id = Guid.NewGuid();
            orderItemLogModels.OrderItems_Id = orderItemLogViewModels.OrderItem.Id;
            orderItemLogModels.Timestamp = DateTime.Now;
            if (string.IsNullOrEmpty(DescriptionLog))
            {
                orderItemLogModels.Description = "Status changed to " + Enum.GetName(typeof(OrderItemStatusEnum), orderItemLogViewModels.OrderItem.Status_enumid);
            }
            else
            {
                orderItemLogModels.Description = DescriptionLog;
            }
            orderItemLogModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            db.OrderItemLog.Add(orderItemLogModels);

            await db.SaveChangesAsync();

            return RedirectToAction("Create", "Conciergeplus", new { id = orderItemsModels.Orders_Id });
        }

        public async Task<ActionResult> Package(Guid id)
        {
            ViewBag.OrderId = id;
            ViewBag.listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Package([Bind(Include = "Id,Shippings_Id,No,Length,Width,Height,Weight,Notes,Status_enumid")] ShippingItemsModels shippingItemsModels, Guid Order_Id, string Items_Selected, string Items_Content)
        {
            if (string.IsNullOrEmpty(Items_Selected))
            {
                ModelState.AddModelError("Items", "No items selected for this package.");
            }

            if (ModelState.IsValid)
            {
                shippingItemsModels.Id = Guid.NewGuid();
                shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Open;
                db.ShippingItems.Add(shippingItemsModels);

                string[] arrID = Items_Selected.Split(',');
                foreach (string s in arrID)
                {
                    ShippingItemContentsModels shippingItemContentsModels = new ShippingItemContentsModels();
                    shippingItemContentsModels.Id = Guid.NewGuid();
                    shippingItemContentsModels.ShippingItems_Id = shippingItemsModels.Id;
                    shippingItemContentsModels.OrderItems_Id = new Guid(s);
                    List<ShippingItemContentDetails> listContent = JsonConvert.DeserializeObject<List<ShippingItemContentDetails>>(Items_Content);
                    foreach (var item in listContent)
                    {
                        if (item.Id == shippingItemContentsModels.OrderItems_Id)
                        {
                            shippingItemContentsModels.Qty = item.qty;
                            shippingItemContentsModels.Notes = item.notes;
                        }
                    }
                    db.ShippingItemContents.Add(shippingItemContentsModels);
                }

                OrdersModels ordersModels = await db.Orders.Where(x => x.Id == Order_Id).FirstOrDefaultAsync();
                ordersModels.Status_enumid = OrderStatusEnum.Purchasing;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Conciergeplus", new { id = Order_Id });
            }

            ViewBag.OrderId = Order_Id;
            ViewBag.listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == Order_Id).ToListAsync();
            return View(shippingItemsModels);
        }

        public async Task<ActionResult> Shipping(Guid id)
        {
            ViewBag.OrderId = id;
            ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<ShippingItemsModels> listShippingItem = new List<ShippingItemsModels>();
            var list = await db.ShippingItems.Where(x => x.Status_enumid == ShippingItemStatusEnum.Open).ToListAsync();
            foreach (var item in list)
            {
                Guid order_item_id = db.ShippingItemContents.Where(x => x.ShippingItems_Id == item.Id).FirstOrDefault().OrderItems_Id;
                if (id == db.OrderItems.Where(x => x.Id == order_item_id).FirstOrDefault().Orders_Id)
                {
                    listShippingItem.Add(item);
                }
            }
            ViewBag.listShippingItem = listShippingItem;

            OrdersModels ordersModels = await db.Orders.Where(x => x.Id == id).FirstOrDefaultAsync();
            ShippingsModels shippingsModels = new ShippingsModels();
            shippingsModels.Origin_Stations_Id = ordersModels.Origin_Stations_Id;
            shippingsModels.Destination_Stations_Id = ordersModels.Destination_Stations_Id;
            return View(shippingsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Shipping([Bind(Include = "Id,No,Timestamp,Origin_Stations_Id,Destination_Stations_Id,Notes")] ShippingsModels shippingsModels, Guid Order_Id, string Items_Selected)
        {
            if (string.IsNullOrEmpty(Items_Selected))
            {
                ModelState.AddModelError("Items", "No package selected for this shipment.");
            }

            if (ModelState.IsValid)
            {
                shippingsModels.Id = Guid.NewGuid();
                shippingsModels.Timestamp = DateTime.Now;
                db.Shippings.Add(shippingsModels);

                string[] arrID = Items_Selected.Split(',');
                foreach (string s in arrID)
                {
                    Guid _id = new Guid(s);
                    ShippingItemsModels shippingItemsModels = await db.ShippingItems.FindAsync(_id);
                    shippingItemsModels.Shippings_Id = shippingsModels.Id;
                    shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                    db.Entry(shippingItemsModels).State = EntityState.Modified;
                }

                OrdersModels ordersModels = await db.Orders.FindAsync(Order_Id);
                ordersModels.Status_enumid = OrderStatusEnum.Shipping;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Create", "Conciergeplus", new { id = Order_Id });
            }

            ViewBag.OrderId = Order_Id;
            ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            List<ShippingItemsModels> listShippingItem = new List<ShippingItemsModels>();
            var list = await db.ShippingItems.Where(x => x.Status_enumid == ShippingItemStatusEnum.Open).ToListAsync();
            foreach (var item in list)
            {
                Guid order_item_id = db.ShippingItemContents.Where(x => x.ShippingItems_Id == item.Id).FirstOrDefault().OrderItems_Id;
                if (Order_Id == db.OrderItems.Where(x => x.Id == order_item_id).FirstOrDefault().Orders_Id)
                {
                    listShippingItem.Add(item);
                }
            }
            ViewBag.listShippingItem = listShippingItem;
            return View(shippingsModels);
        }
    }
}