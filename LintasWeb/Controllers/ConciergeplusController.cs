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
            List<OrderItemsModels> items = db.OrderItems.Where(x => x.Orders_Id == id).OrderBy(x => x.RowNo).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>Description</th>
                                                <th>Qty</th>
                                                <th>Amount</th>
                                                <th>Notes</th>
                                                <th>Tracking No</th>
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
                                <td><a href='" + Url.Content("~") + "Tracking/?no=" + item.TrackingNo + "' target='_blank'>" + item.TrackingNo + @"</a></td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCsPlusLogs(Guid id)
        {
            List<ActivityLogsViewModels> list = new List<ActivityLogsViewModels>();
            #region CS PLUS
            List<ActivityLogsModels> csplus = db.ActivityLogs.Where(x => x.RefId == id && x.Description != "Modified").ToList();
            foreach (var item in csplus)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.Timestamp,
                    TableName = item.TableName,
                    Description = item.Description,
                    User_Id = item.UserAccounts_Id
                });
            }
            #endregion
            #region ORDER ITEMS
            var order_item = (from oi in db.OrderItems
                              join al in db.ActivityLogs on oi.Id equals al.RefId
                              where oi.Orders_Id == id && al.Description == "Added"
                              select new { al, oi }).ToList();
            foreach (var item in order_item)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.al.Timestamp,
                    TableName = item.oi.Description,
                    Description = item.al.Description,
                    User_Id = item.al.UserAccounts_Id
                });
            }
            #endregion
            #region INVOICES
            var invoice = (from o in db.Orders
                           join inv in db.Invoices on o.Id equals inv.Ref_Id
                           join al in db.ActivityLogs on inv.Id equals al.RefId
                           where o.Id == id && al.Description == "Added"
                           select new { al, inv }).ToList();
            foreach (var item in invoice)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.al.Timestamp,
                    TableName = item.al.TableName,
                    Description = item.al.Description,
                    User_Id = item.al.UserAccounts_Id
                });
            }
            #endregion
            #region PAYMENT
            var payment = (from o in db.Orders
                           join inv in db.Invoices on o.Id equals inv.Ref_Id
                           join p in db.Payments on inv.Id equals p.Invoices_Id
                           join al in db.ActivityLogs on p.Id equals al.RefId
                           where o.Id == id && al.Description == "Added"
                           select new { al, inv }).ToList();
            foreach (var item in payment)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.al.Timestamp,
                    TableName = item.al.TableName,
                    Description = item.al.Description,
                    User_Id = item.al.UserAccounts_Id
                });
            }
            #endregion
            #region PACKAGING
            var package = (from si in db.ShippingItems
                           join sic in db.ShippingItemContents on si.Id equals sic.ShippingItems_Id
                           join oi in db.OrderItems on sic.OrderItems_Id equals oi.Id
                           join al in db.ActivityLogs on si.Id equals al.RefId
                           where oi.Orders_Id == id && al.Description == "Added"
                           select new { al }).ToList();
            foreach (var item in package)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.al.Timestamp,
                    TableName = "Package",
                    Description = item.al.Description,
                    User_Id = item.al.UserAccounts_Id
                });
            }
            #endregion
            #region SHIPPING
            var shipping = (from s in db.Shippings
                            join si in db.ShippingItems on s.Id equals si.Shippings_Id
                            join sic in db.ShippingItemContents on si.Id equals sic.ShippingItems_Id
                            join oi in db.OrderItems on sic.OrderItems_Id equals oi.Id
                            join al in db.ActivityLogs on s.Id equals al.RefId
                            where oi.Orders_Id == id && al.Description == "Added"
                            select new { al }).ToList();
            foreach (var item in shipping)
            {
                list.Add(new ActivityLogsViewModels()
                {
                    Timestamp = item.al.Timestamp,
                    TableName = "Shipping",
                    Description = item.al.Description,
                    User_Id = item.al.UserAccounts_Id
                });
            }
            #endregion

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
            foreach (var item in list.OrderByDescending(x => x.Timestamp))
            {
                message += @"<tr>
                                <td>" + string.Format("{0:yyyy-MM-dd HH:mm}", item.Timestamp) + @"</td>
                                <td>" + item.TableName + " " + item.Description + @"</td>
                                <td>" + db.User.Where(x => x.Id == item.User_Id).FirstOrDefault().Fullname + @"</td>
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
            List<OrderItemLogModels> itemLogs = db.OrderItemLog.Where(x => x.OrderItems_Id == id).OrderByDescending(x => x.Timestamp).ToList();
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
                                <td>" + item.Timestamp.ToString("yyyy/MM/dd HH:mm") + @"</td>
                                <td>" + item.Description + @"</td>
                                <td>" + db.User.Where(x => x.Id == item.UserAccounts_Id).FirstOrDefault().Fullname + @"</td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult StopPurchasing(Guid id)
        {
            OrderItemsModels orderItemsModels = db.OrderItems.Where(x => x.Id == id).FirstOrDefault();

            OrderItemLogModels orderItemLogModels = new OrderItemLogModels();
            orderItemLogModels.Id = Guid.NewGuid();
            orderItemLogModels.OrderItems_Id = id;
            orderItemLogModels.Timestamp = DateTime.Now;
            orderItemLogModels.Description = "[Stopped] Order Qty changed from " + orderItemsModels.Qty + " to " + orderItemsModels.ReceivedQty;
            orderItemLogModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            db.OrderItemLog.Add(orderItemLogModels);

            orderItemsModels.Qty = orderItemsModels.ReceivedQty;
            orderItemsModels.ReceiveTimestamp = DateTime.Now;
            orderItemsModels.Status_enumid = OrderItemStatusEnum.Received;
            db.Entry(orderItemsModels).State = EntityState.Modified;

            db.SaveChanges();

            return Json(new { result = "200" }, JsonRequestBehavior.AllowGet);
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
                                                <th>Tracking No</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                message += @"<tr>
                                <td>PKG" + item.No + @"</td>
                                <td>" + item.Length + " x " + item.Width + " x " + item.Height + @"</td>
                                <td>" + item.Weight.ToString("#,##0") + @"</td>
                                <td>" + item.Notes + @"</td>
                                <td><a href='" + Url.Content("~") + "Tracking/?no=" + item.TrackingNo + "' target='_blank'>" + item.TrackingNo + @"</a></td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPriceList(string key)
        {
            if (key == "order")
            {
                var ddl = new SelectList(db.OrderPrices.OrderBy(x => x.Description), "Id", "Description");
                return Json(new { ddl }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var ddl = new SelectList(db.ShippingPrices.OrderBy(x => x.Description), "Id", "Description");
                return Json(new { ddl }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPrices(string key, Guid id)
        {
            decimal amount = 0; string notes;
            if (key == "order")
            {
                amount = db.OrderPrices.Where(x => x.Id == id).FirstOrDefault().Amount;
                notes = string.IsNullOrEmpty(db.OrderPrices.Where(x => x.Id == id).FirstOrDefault().Notes) ? "" 
                    : db.OrderPrices.Where(x => x.Id == id).FirstOrDefault().Notes;
            }
            else
            {
                amount = db.ShippingPrices.Where(x => x.Id == id).FirstOrDefault().Amount;
                notes = string.IsNullOrEmpty(db.ShippingPrices.Where(x => x.Id == id).FirstOrDefault().Notes) ? "" 
                    : db.ShippingPrices.Where(x => x.Id == id).FirstOrDefault().Notes;
            }

            return Json(new { amount = amount, notes = notes }, JsonRequestBehavior.AllowGet);
        }

        // GET: Conciergeplus
        public async Task<ActionResult> Index()
        {
            var data = await (from o in db.Orders
                              join c in db.Customers on o.Customers_Id equals c.Id
                              join os in db.Stations on o.Origin_Stations_Id equals os.Id
                              join ds in db.Stations on o.Destination_Stations_Id equals ds.Id
                              where o.Service == "CS+"
                              select new OrdersIndexViewModels
                              {
                                  Id = o.Id,
                                  Service = o.Service,
                                  No = o.No,
                                  Timestamp = o.Timestamp,
                                  Customer = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                                  Origin = os.Name,
                                  Destination = ds.Name,
                                  Notes = o.Notes,
                                  Status = o.Status_enumid
                              }).ToListAsync();

            List<OrdersIndexViewModels> list = new List<OrdersIndexViewModels>();
            foreach (var item in data)
            {
                var list_order_item = db.OrderItems.Where(x => x.Orders_Id == item.Id).ToList();
                #region Check Received Order Item & Order Status
                bool isPending = true;
                foreach (var a in list_order_item)
                {
                    if (a.ReceivedQty != a.Qty) { isPending = true; break; }
                    else //order item sudah diterima semua
                    {
                        int qtyUsed = 0; bool isOpen = true;
                        var list_shipping_item_content = db.ShippingItemContents.Where(x => x.OrderItems_Id == a.Id).ToList();
                        foreach (var sic in list_shipping_item_content)
                        {
                            qtyUsed += sic.Qty;
                            var list_shipping_item = db.ShippingItems.Where(x => x.Id == sic.ShippingItems_Id).ToList();
                            foreach (var si in list_shipping_item)
                            {
                                if (si.Status_enumid == ShippingItemStatusEnum.Open) { isOpen = true; break; }
                                else { isOpen = false; }
                            }
                        }
                        int remaining = a.Qty - qtyUsed;

                        if (remaining > 0 && a.Status_enumid == OrderItemStatusEnum.Received) //masih ada order item yang belum di packaging
                        {
                            isPending = true; break;
                        }
                        else //semua order item sudah di buat packaging
                        {
                            if ((int)item.Status < 5) { isPending = true; } //jika order status belum sampai shipping
                            else { isPending = false; }
                        }

                        if (isOpen) { isPending = true; break; } //ada packaging yg blum di ship
                    }
                }
                #endregion
                #region Check Shipment Completed
                bool isShipmentComplete = false;
                foreach (var b in list_order_item)
                {
                    var list_sic = db.ShippingItemContents.Where(x => x.OrderItems_Id == b.Id).ToList();
                    foreach (var sic in list_sic)
                    {
                        var si = db.ShippingItems.Where(x => x.Id == sic.ShippingItems_Id).FirstOrDefault();
                        if (si.Shippings_Id.HasValue)
                        {
                            if ((int)db.Shippings.Where(x => x.Id == si.Shippings_Id.Value).FirstOrDefault().Status_enumid < 5) //status shipping blm Shipment Completed
                            {
                                isShipmentComplete = false; break;
                            }
                            else { isShipmentComplete = true; }
                        }
                    }
                    if (!isShipmentComplete) { break; }
                }
                #endregion

                OrdersIndexViewModels oivm = new OrdersIndexViewModels();
                oivm.Id = item.Id;
                oivm.Service = item.Service;
                oivm.No = item.No;
                oivm.Timestamp = item.Timestamp;
                oivm.Customer = item.Customer;
                oivm.Origin = item.Origin;
                oivm.Destination = item.Destination;
                oivm.Notes = item.Notes;
                oivm.Pending = isPending;
                oivm.Status = (!isShipmentComplete) ? item.Status : OrderStatusEnum.ShipmentCompleted;
                list.Add(oivm);
            }

            return View(list);
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

                listInvoice = await db.Invoices.Where(x => x.Ref_Id == id).OrderBy(x => x.No).ToListAsync();
                listPayment = await (from pay in db.Payments
                                     join i in db.Invoices on pay.Invoices_Id equals i.Id
                                     where i.Ref_Id == id
                                     select new PaymentsIndexViewModels
                                     {
                                         Id = pay.Id,
                                         Timestamp = pay.Timestamp,
                                         InvoiceNo = i.No,
                                         Amount = pay.Amount,
                                         Info = pay.PaymentInfo,
                                         Notes = pay.Notes
                                     }).ToListAsync();

                invoicesModels = await db.Invoices.Where(x => x.Ref_Id == id).FirstOrDefaultAsync();
                ViewBag.listInv = invoicesModels != null ? db.InvoiceItems.Where(x => x.Invoices_Id == invoicesModels.Id).ToList() : null;

                paymentsModels = invoicesModels == null ? null : await db.Payments.Where(x => x.Invoices_Id == invoicesModels.Id).FirstOrDefaultAsync();

                listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id).OrderBy(x => x.RowNo).ToListAsync();
                
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
                    if (shipping != null)
                    {
                        var order = await (from o in db.Orders
                                           join oi in db.OrderItems on o.Id equals oi.Orders_Id
                                           where oi.Id == shipping.OrderItems_Id
                                           select new { o }).FirstOrDefaultAsync();

                        if (order.o.Id == id)
                        {
                            listShippingItem.Add(item);
                        }
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

                var list_shipping = await db.Shippings.OrderByDescending(x => x.Timestamp).ToListAsync();
                listShipping = new List<ShippingsViewModels>();
                foreach (var item in list_shipping)
                {
                    //Guid shipping_item_id = db.ShippingItems.Where(x => x.Shippings_Id == item.Id).FirstOrDefault().Id;
                    //var shipping_item_content = db.ShippingItemContents.Where(x => x.ShippingItems_Id == shipping_item_id).FirstOrDefault();
                    //if (shipping_item_content != null)
                    //{
                    //    if (id == db.OrderItems.Where(x => x.Id == shipping_item_content.OrderItems_Id).FirstOrDefault().Orders_Id)
                    //    {
                    //        ShippingsViewModels svm = new ShippingsViewModels();
                    //        svm.Id = item.Id;
                    //        svm.No = item.No;
                    //        svm.Timestamp = item.Timestamp;
                    //        svm.Origin = db.Stations.Where(x => x.Id == item.Origin_Stations_Id).FirstOrDefault().Name;
                    //        svm.Destination = db.Stations.Where(x => x.Id == item.Destination_Stations_Id).FirstOrDefault().Name;
                    //        svm.Notes = item.Notes;
                    //        listShipping.Add(svm);
                    //    }
                    //}

                    bool isCsPlus = false;
                    var ship_item = db.ShippingItems.Where(x => x.Shippings_Id == item.Id).ToList();
                    foreach (var itm in ship_item)
                    {
                        if (!isCsPlus)
                        {
                            var ship_item_content = db.ShippingItemContents.Where(x => x.ShippingItems_Id == itm.Id).ToList();
                            foreach (var i in ship_item_content)
                            {
                                if (!isCsPlus)
                                {
                                    if (id == db.OrderItems.Where(x => x.Id == i.OrderItems_Id).FirstOrDefault().Orders_Id)
                                    {
                                        ShippingsViewModels svm = new ShippingsViewModels();
                                        svm.Id = item.Id;
                                        svm.No = item.No;
                                        svm.Timestamp = item.Timestamp;
                                        svm.Origin = db.Stations.Where(x => x.Id == item.Origin_Stations_Id).FirstOrDefault().Name;
                                        svm.Destination = db.Stations.Where(x => x.Id == item.Destination_Stations_Id).FirstOrDefault().Name;
                                        svm.Notes = item.Notes;
                                        listShipping.Add(svm);
                                        isCsPlus = true;
                                    }
                                }
                            }
                        }
                    }
                }
                
                //bool isPurchasingCompleted = false;
                //foreach (var item in db.OrderItems.Where(x => x.Orders_Id == id).ToList())
                //{
                //    if (item.Status_enumid == OrderItemStatusEnum.Received && item.Qty == item.ReceivedQty)
                //    {
                //        isPurchasingCompleted = true;
                //    }
                //    else { isPurchasingCompleted = false; }

                //    if (!isPurchasingCompleted)
                //        break;
                //}

                ViewBag.startIndex = (ordersModels.Status_enumid == OrderStatusEnum.Ordered
                    || ordersModels.Status_enumid == OrderStatusEnum.WaitingPayment
                    || ordersModels.Status_enumid == OrderStatusEnum.PaymentCompleted) 
                    ? (int)ordersModels.Status_enumid + 1 : (int)ordersModels.Status_enumid;

                //ViewBag.startIndex = (ordersModels.Status_enumid == OrderStatusEnum.Shipping)
                //    ? (int)ordersModels.Status_enumid //last step is shipping
                //    : (ordersModels.Status_enumid != OrderStatusEnum.Purchasing)
                //        ? (int)ordersModels.Status_enumid + 1 //normal
                //        : (isPurchasingCompleted) 
                //            ? (int)ordersModels.Status_enumid + 1 //purchasing completed
                //            : (int)ordersModels.Status_enumid; //purchasing not completed
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

        public async Task<JsonResult> SaveOrder(Guid? order_id, DateTime order_date, Guid customer_id, Guid origin_id, Guid destination_id, string receiver, string address, string address2, string city, string state, string country, string country_code, string postal_code, string mobile, string phone, string fax, string email, string company, string tax_number, string notes, string order_items)
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
                ordersModels.Service = "CS+";
                ordersModels.No = m.GetLastHexAllTime("CS+"); //m.GetLastNo(ordersModels.Timestamp).ToString("00000");
                ordersModels.Customers_Id = customer_id;
                ordersModels.Origin_Stations_Id = origin_id;
                ordersModels.Destination_Stations_Id = destination_id;
                ordersModels.ReceiverName = receiver;
                ordersModels.Address = address;
                ordersModels.Address2 = address2;
                ordersModels.City = city;
                ordersModels.State = state;
                ordersModels.PostalCode = postal_code;
                ordersModels.Country = country;
                ordersModels.CountryCode = country_code;
                ordersModels.Phone1 = mobile;
                ordersModels.Phone2 = phone;
                ordersModels.Fax = fax;
                ordersModels.Email = email;
                ordersModels.Company = company;
                ordersModels.TaxNumber = tax_number;
                ordersModels.Notes = notes;
                ordersModels.Status_enumid = OrderStatusEnum.Ordered;
                db.Orders.Add(ordersModels);

                int line_no = 1;
                List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(order_items);
                foreach (var item in lOrderItem)
                {
                    OrderItemsModels oi = new OrderItemsModels();
                    oi.Id = Guid.NewGuid();
                    oi.Orders_Id = ordersModels.Id;
                    oi.RowNo = line_no;
                    oi.Description = item.desc;
                    oi.Qty = item.qty;
                    oi.Amount = item.cost;
                    oi.Notes = item.note;
                    oi.Status_enumid = OrderItemStatusEnum.Pending;
                    oi.Invoiced = false;
                    oi.TrackingNo = Common.Master.GetTrackingNo(Common.Master.GetRandomHexNumber(10));
                    db.OrderItems.Add(oi);
                    line_no++;

                    TrackingModels tr = new TrackingModels();
                    tr.Id = Guid.NewGuid();
                    tr.Ref_Id = oi.Id; //Order Items Id
                    tr.Timestamp = DateTime.Now;
                    tr.Description = "Orders Created";
                    db.Tracking.Add(tr);
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

            return Json(new { status = status, id = ordersModels.Id, no_order = ordersModels.Service + ordersModels.No }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SaveInvoice(Guid order_id, DateTime inv_date, decimal total_amount, string notes, string inv_items)
        {
            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == order_id).FirstOrDefaultAsync();
            InvoicesModels invoicesModels;

            if (ordersModels.Status_enumid == OrderStatusEnum.Ordered)
            {
                invoicesModels = new InvoicesModels();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.Ref_Id = order_id;
                int counter = db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id).Count();
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
                invoicesModels = await db.Invoices.Where(x => x.Ref_Id == order_id).FirstOrDefaultAsync();

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

            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
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

            InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Ref_Id == ordersModels.Id).FirstOrDefaultAsync();
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
            ViewBag.ListOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id && x.Invoiced == false).OrderBy(x => x.Description).ToListAsync();
            ViewBag.ListPrice = new SelectList(db.OrderPrices.OrderBy(x => x.Description).ToList(), "Id", "Description");

            InvoicesModels invoicesModels = new InvoicesModels();
            return View(invoicesModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Invoice([Bind(Include = "Id,Ref_Id,No,Timestamp,TotalAmount,TotalPaid,Notes")] InvoicesModels invoicesModels, string Items, bool ItemValid)
        {
            if (!ItemValid)
            {
                ModelState.AddModelError("Items", "The Invoice Item Description and Amount field is required.");
            }

            if (ModelState.IsValid)
            {
                OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
                int counter = db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id).Count();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.No = ordersModels.Timestamp.ToString("yyyyMMdd") + ordersModels.No + counter;
                db.Invoices.Add(invoicesModels);

                List<OrderItemsModels> listOrderItems = await db.OrderItems.Where(x => x.Orders_Id == invoicesModels.Ref_Id && x.Invoiced == false).ToListAsync();
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

                    foreach (var oi in listOrderItems)
                    {
                        if (item.desc == oi.Description)
                        {
                            oi.Invoiced = true;
                            db.Entry(oi).State = EntityState.Modified;
                        }
                    }
                }

                ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();
                return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Ref_Id });
            }

            var order = await (from o in db.Orders
                               join c in db.Customers on o.Customers_Id equals c.Id
                               where o.Status_enumid != OrderStatusEnum.Completed && o.Id == invoicesModels.Ref_Id
                               select new { o, c }).FirstOrDefaultAsync();
            string fullName = order.c.FirstName;
            if (!string.IsNullOrEmpty(order.c.MiddleName)) { fullName += " " + order.c.MiddleName; }
            if (!string.IsNullOrEmpty(order.c.LastName)) { fullName += " " + order.c.LastName; }

            ViewBag.OrderId = invoicesModels.Ref_Id;
            ViewBag.Order = fullName + " (" + order.o.Timestamp.ToString("yyyyMMdd") + order.o.No + ")";
            ViewBag.ListOrderItem = await db.OrderItems.Where(x => x.Orders_Id == invoicesModels.Ref_Id && x.Invoiced == false).OrderBy(x => x.Description).ToListAsync();
            ViewBag.ListPrice = new SelectList(db.OrderPrices.OrderBy(x => x.Description).ToList(), "Id", "Description");
            return View(invoicesModels);
        }

        public async Task<ActionResult> DeleteInvoice(Guid? id)
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

            OrdersModels ordersModels = await db.Orders.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
            List<InvoicesModels> invoices = await db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id && x.Id != id).ToListAsync();
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
            return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Ref_Id });
        }

        public async Task<ActionResult> Payment(Guid? id)
        {
            var invoices = await db.Invoices.Where(x => x.Ref_Id == id && x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToListAsync();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " " + inv.Notes + " [" + string.Format("{0:N2}", inv.TotalAmount) + "] Due " + string.Format("{0:N2}", inv.TotalAmount - inv.TotalPaid)
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

                decimal sum_invoice_amount = db.Invoices.Where(x => x.Ref_Id == invoice.Ref_Id).Sum(x => x.TotalAmount);
                decimal sum_invoice_paid = db.Invoices.Where(x => x.Ref_Id == invoice.Ref_Id).Sum(x => x.TotalPaid) + paymentsModels.Amount;

                if (sum_invoice_amount == sum_invoice_paid)
                {
                    OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoice.Ref_Id).FirstOrDefaultAsync();
                    ordersModels.Status_enumid = OrderStatusEnum.PaymentCompleted;
                    db.Entry(ordersModels).State = EntityState.Modified;

                    List<OrderItemsModels> list_item = db.OrderItems.Where(x => x.Orders_Id == ordersModels.Id).ToList();
                    foreach (var item in list_item)
                    {
                        TrackingModels tr = new TrackingModels();
                        tr.Id = Guid.NewGuid();
                        tr.Ref_Id = item.Id;
                        tr.Timestamp = DateTime.Now;
                        tr.Description = "Payment Completed";
                        db.Tracking.Add(tr);
                    }
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Conciergeplus", new { id = invoice.Ref_Id });
            }

            //ViewBag.InvoiceId = paymentsModels.Invoices_Id;
            //ViewBag.Invoice = invoice.No + " - " + invoice.Notes + " (" + string.Format("{0:N2}", invoice.TotalAmount) + ")";
            
            var invoices = await db.Invoices.Where(x => x.Ref_Id == Order_Id && x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToListAsync();
            List<object> newList = new List<object>();
            foreach (var inv in invoices)
            {
                newList.Add(new
                {
                    Id = inv.Id,
                    Name = inv.No + " " + inv.Notes + " [" + string.Format("{0:N2}", inv.TotalAmount) + "] Due " + string.Format("{0:N2}", inv.TotalAmount - inv.TotalPaid)
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

            OrdersModels ordersModels = await db.Orders.Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
            ordersModels.Status_enumid = OrderStatusEnum.WaitingPayment;
            db.Entry(ordersModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Conciergeplus", new { id = invoicesModels.Ref_Id });
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
        public async Task<ActionResult> OrderItemLog([Bind(Include = "OrderItem")] OrderItemLogViewModels orderItemLogViewModels, int Remaining, string DescriptionLog)
        {
            OrderItemsModels orderItemsModels = await db.OrderItems.AsNoTracking().Where(x => x.Id == orderItemLogViewModels.OrderItem.Id).FirstOrDefaultAsync();
            if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Received)
            {
                if (Remaining > orderItemsModels.Qty - orderItemsModels.ReceivedQty)
                {
                    ModelState.AddModelError("Received", "Max Received Qty is " + (orderItemsModels.Qty - orderItemsModels.ReceivedQty));
                }
            }

            if (ModelState.IsValid)
            {
                string qtyReceived = "";
                if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Purchased)
                {
                    orderItemsModels.PurchaseTimestamp = DateTime.Now;
                    orderItemsModels.Status_enumid = orderItemLogViewModels.OrderItem.Status_enumid;
                }
                else if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Conflict)
                {
                    orderItemsModels.Status_enumid = orderItemLogViewModels.OrderItem.Status_enumid;
                }
                else if (orderItemLogViewModels.OrderItem.Status_enumid == OrderItemStatusEnum.Received)
                {
                    qtyReceived = " " + Remaining.ToString();

                    if (orderItemsModels.Qty == orderItemsModels.ReceivedQty + Remaining)
                    {
                        orderItemsModels.ReceiveTimestamp = DateTime.Now;
                        orderItemsModels.Status_enumid = orderItemLogViewModels.OrderItem.Status_enumid;
                    }
                    orderItemsModels.ReceivedQty += Remaining;

                    TrackingModels tr = new TrackingModels();
                    tr.Id = Guid.NewGuid();
                    tr.Ref_Id = orderItemsModels.Id;
                    tr.Timestamp = DateTime.Now;
                    tr.Description = Remaining + " Item Received from Supplier";
                    db.Tracking.Add(tr);
                }
                db.Entry(orderItemsModels).State = EntityState.Modified;

                OrderItemLogModels orderItemLogModels = new OrderItemLogModels();
                orderItemLogModels.Id = Guid.NewGuid();
                orderItemLogModels.OrderItems_Id = orderItemLogViewModels.OrderItem.Id;
                orderItemLogModels.Timestamp = DateTime.Now;
                if (string.IsNullOrEmpty(DescriptionLog))
                {
                    orderItemLogModels.Description = "[" + Enum.GetName(typeof(OrderItemStatusEnum), orderItemLogViewModels.OrderItem.Status_enumid) + qtyReceived + "]";
                }
                else
                {
                    orderItemLogModels.Description = "[" + Enum.GetName(typeof(OrderItemStatusEnum), orderItemLogViewModels.OrderItem.Status_enumid) + qtyReceived + "] " + DescriptionLog;
                }
                orderItemLogModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
                db.OrderItemLog.Add(orderItemLogModels);

                OrdersModels ordersModels = db.Orders.Where(x => x.Id == orderItemsModels.Orders_Id).FirstOrDefault();
                ordersModels.Status_enumid = OrderStatusEnum.Purchasing;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Conciergeplus", new { id = orderItemsModels.Orders_Id });
            }

            List<OrderItemLogModels> listOrderItemLog = await db.OrderItemLog.Where(x => x.OrderItems_Id == orderItemsModels.Id).OrderByDescending(x => x.Timestamp).ToListAsync();
            orderItemLogViewModels.OrderItem = orderItemsModels;
            orderItemLogViewModels.ListOrderItemLog = listOrderItemLog;
            return View(orderItemLogViewModels);
        }

        public async Task<ActionResult> Package(Guid id)
        {
            ViewBag.OrderId = id;
            ViewBag.listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id && x.Qty == x.ReceivedQty).ToListAsync();
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
                Common.Master m = new Common.Master();
                shippingItemsModels.No = m.GetLastHexAllTime("PKG");
                shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Open;
                shippingItemsModels.Invoiced = false;

                int row = 1; string description = "";
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

                    if (row == 1) { description += db.OrderItems.Where(x => x.Id.ToString() == s).FirstOrDefault().Description; }
                    else { description += ", " + db.OrderItems.Where(x => x.Id.ToString() == s).FirstOrDefault().Description; }
                    row++;
                }

                shippingItemsModels.Description = description;
                db.ShippingItems.Add(shippingItemsModels);

                OrdersModels ordersModels = await db.Orders.Where(x => x.Id == Order_Id).FirstOrDefaultAsync();
                ordersModels.Status_enumid = OrderStatusEnum.Packaging;
                db.Entry(ordersModels).State = EntityState.Modified;

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Conciergeplus", new { id = Order_Id });
            }

            ViewBag.OrderId = Order_Id;
            ViewBag.listOrderItem = await db.OrderItems.Where(x => x.Orders_Id == Order_Id && x.Qty == x.ReceivedQty).ToListAsync();
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
                    shippingItemsModels.Invoiced = true;
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