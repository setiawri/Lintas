using LintasMVC.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class ShippingController : Controller
    {
        private LintasContext db = new LintasContext();

        public async Task<ActionResult> Index()
        {
            //var data = (from s in db.Shippings
            //            join o in db.Stations on s.Origin_Stations_Id equals o.Id
            //            join d in db.Stations on s.Destination_Stations_Id equals d.Id
            //            select new ShippingsViewModels
            //            {
            //                Id = s.Id,
            //                No = s.No,
            //                Timestamp = s.Timestamp,
            //                Origin = o.Name,
            //                Destination = d.Name,
            //                Notes = s.Notes,
            //                Status_enumid = s.Status_enumid
            //            }).ToListAsync();

            var data = await (from s in db.Shippings
                              join o in db.Stations on s.Origin_Stations_Id equals o.Id
                              join d in db.Stations on s.Destination_Stations_Id equals d.Id
                              select new { s, o, d }).ToListAsync();
            List<ShippingsViewModels> list = new List<ShippingsViewModels>();
            foreach (var item in data)
            {
                list.Add(new ShippingsViewModels
                {
                    Id = item.s.Id,
                    No = item.o.Code + item.d.Code + item.s.Timestamp.ToString("MM") + item.s.Timestamp.ToString("dd") + item.s.No,
                    Timestamp = item.s.Timestamp,
                    Origin = item.o.Name,
                    Destination = item.d.Name,
                    Notes = item.s.Notes,
                    Status_enumid = item.s.Status_enumid
                });
            }

            return View(list);
        }

        public JsonResult GetItems(Guid customerId)
        {
            List<ShippingItemsModels> list = new List<ShippingItemsModels>();
            var shippingItem = db.ShippingItems.Where(x => x.Status_enumid == ShippingItemStatusEnum.Open).ToList();
            foreach (var item in shippingItem)
            {
                Guid order_item_id = db.ShippingItemContents.Where(x => x.ShippingItems_Id == item.Id).FirstOrDefault().OrderItems_Id;
                Guid order_id = db.OrderItems.Where(x => x.Id == order_item_id).FirstOrDefault().Orders_Id;
                if (customerId == db.Orders.Where(x => x.Id == order_id).FirstOrDefault().Customers_Id)
                {
                    list.Add(item);
                }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLogs(Guid id)
        {
            List<DeliveryLogModels> devLogs = db.DeliveryLog.Where(x => x.ShippingItem_Id == id).OrderByDescending(x => x.Timestamp).ToList();
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
            foreach (DeliveryLogModels item in devLogs)
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

        public JsonResult CancelShipping(Guid id)
        {
            ShippingsModels shippingsModels = db.Shippings.Where(x => x.Id == id).FirstOrDefault();
            shippingsModels.Status_enumid = ShippingStatusEnum.Cancelled;
            db.Entry(shippingsModels).State = EntityState.Modified;
            db.SaveChanges();
            string status = "200";

            return Json(status, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShippingLogs(Guid id)
        {
            List<ActivityLogsViewModels> list = new List<ActivityLogsViewModels>();
            #region SHIPPING
            List<ActivityLogsModels> shipping = db.ActivityLogs.Where(x => x.RefId == id && x.Description != "Modified").ToList();
            foreach (var item in shipping)
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
            #region INVOICES
            var invoice = (from s in db.Shippings
                           join inv in db.Invoices on s.Id equals inv.Ref_Id
                           join al in db.ActivityLogs on inv.Id equals al.RefId
                           where s.Id == id && al.Description == "Added"
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
            var payment = (from s in db.Shippings
                           join inv in db.Invoices on s.Id equals inv.Ref_Id
                           join p in db.Payments on inv.Id equals p.Invoices_Id
                           join al in db.ActivityLogs on p.Id equals al.RefId
                           where s.Id == id && al.Description == "Added"
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
            #region SHIPMENT
            var shipment = (from smnt in db.Shipments
                           join si in db.ShippingItems on smnt.Id equals si.Shipments_Id
                           join s in db.Shippings on si.Shippings_Id equals s.Id
                           join al in db.ActivityLogs on smnt.Id equals al.RefId
                           where s.Id == id && al.Description == "Added"
                           select new { al }).ToList();
            foreach (var item in shipment)
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

        public JsonResult AutofillingCustomer(Guid customer_id)
        {
            var cust_model = db.Customers.Where(x => x.Id == customer_id).FirstOrDefault();
            var country_model = db.Countries.Where(x => x.Id == cust_model.Countries_Id).FirstOrDefault();

            return Json(new { cust_model, country_model }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Create(Guid? id, Guid? order)
        {
            var customers = await db.Customers.OrderBy(x => x.FirstName).ToListAsync();
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

            ShippingsModels shipping;
            List<InvoicesModels> listInvoice;
            List<PaymentsIndexViewModels> listPayment;
            List<ShippingDocumentsViewModels> listDocument;
            List<ShipmentsIndexViewModels> listShipment;
            List<ShippingItemsModels> listPackage;
            ShippingServicesModels ShippingServices = new ShippingServicesModels();
            if (id == null || id == Guid.Empty)
            {
                shipping = new ShippingsModels();
                listInvoice = new List<InvoicesModels>();
                listPayment = new List<PaymentsIndexViewModels>();
                listDocument = new List<ShippingDocumentsViewModels>();
                listShipment = new List<ShipmentsIndexViewModels>();
                listPackage = new List<ShippingItemsModels>();
                ViewBag.startIndex = 0;

                if (order != null)
                {
                    OrdersModels ordersModels = db.Orders.Where(x => x.Id == order).FirstOrDefault();
                    //shipping.Customers_Id = ordersModels.Customers_Id;
                    shipping.Origin_Stations_Id = ordersModels.Origin_Stations_Id;
                    shipping.Destination_Stations_Id = ordersModels.Destination_Stations_Id;
                    shipping.Company = ordersModels.Company;
                    shipping.ReceiverName = ordersModels.ReceiverName;
                    shipping.Address = ordersModels.Address;
                    shipping.Address2 = ordersModels.Address2;
                    shipping.City = ordersModels.City;
                    shipping.State = ordersModels.State;
                    shipping.Country = ordersModels.Country;
                    shipping.CountryCode = ordersModels.CountryCode;
                    shipping.PostalCode = ordersModels.PostalCode;
                    shipping.Phone1 = ordersModels.Phone1;
                    shipping.Phone2 = ordersModels.Phone2;
                    shipping.Fax = ordersModels.Fax;
                    shipping.Email = ordersModels.Email;
                    shipping.TaxNumber = ordersModels.TaxNumber;
                    shipping.Notes = ordersModels.Notes;

                    List<ShippingItemsModels> list_si = new List<ShippingItemsModels>();
                    var shippingItem = db.ShippingItems.Where(x => x.Status_enumid == ShippingItemStatusEnum.Open).ToList();
                    foreach (var item in shippingItem)
                    {
                        Guid order_item_id = db.ShippingItemContents.Where(x => x.ShippingItems_Id == item.Id).FirstOrDefault().OrderItems_Id;
                        Guid order_id = db.OrderItems.Where(x => x.Id == order_item_id).FirstOrDefault().Orders_Id;
                        if (ordersModels.Customers_Id == db.Orders.Where(x => x.Id == order_id).FirstOrDefault().Customers_Id)
                        {
                            list_si.Add(item);
                        }
                    }
                    //ViewBag.listItems = list_si;
                    ViewBag.custID = ordersModels.Customers_Id;
                }
            }
            else
            {
                shipping = await db.Shippings.Where(x => x.Id == id).FirstOrDefaultAsync();
                ViewBag.no_shipping = db.Stations.Where(x => x.Id == shipping.Origin_Stations_Id).FirstOrDefault().Code
                    + db.Stations.Where(x => x.Id == shipping.Destination_Stations_Id).FirstOrDefault().Code
                    + shipping.Timestamp.ToString("MM")
                    + shipping.Timestamp.ToString("dd")
                    + shipping.No;
                var shipping_items = db.ShippingItems.Where(x => x.Shippings_Id == id).OrderBy(x => x.No).ToList();
                ViewBag.listItems = shipping_items;
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

                listDocument = new List<ShippingDocumentsViewModels>();
                foreach(var item in shipping_items)
                {
                    ShippingDocumentsViewModels sdvm = new ShippingDocumentsViewModels();
                    sdvm.Id = item.Id;
                    sdvm.No = "PKG" + item.No;
                    sdvm.ListFileUploads = db.FileUploads.Where(x => x.Ref_Id == item.Id).ToList();
                    listDocument.Add(sdvm);
                }

                listShipment = await (from si in db.ShippingItems
                                      join s in db.Shipments on si.Shipments_Id equals s.Id
                                      join f in db.Forwarders on s.Forwarders_Id equals f.Id
                                      where si.Shippings_Id == id
                                      select new ShipmentsIndexViewModels
                                      {
                                          Id = s.Id,
                                          Timestamp = s.Timestamp,
                                          No = s.No,
                                          Forwarders = f.Name,
                                          Notes = s.Notes,
                                          Status_enumid = s.Status_enumid
                                      }).ToListAsync();

                listPackage = await db.ShippingItems.Where(x => x.Shippings_Id == id && x.Shipments_Id != null).OrderBy(x => x.No).ToListAsync();

                if (shipping.Status_enumid == ShippingStatusEnum.ShipmentComplete)
                {
                    ViewBag.startIndex = 5; //go to step Pickup/Delivery
                }
                else
                {
                    ViewBag.startIndex = (int)shipping.Status_enumid;
                }

                //int count_inv = await db.Invoices.Where(x => x.Ref_Id == id).CountAsync();
                //if (count_inv == 0)
                //{
                //    ViewBag.startIndex = 1;
                //}
                //else
                //{
                //    if (listInvoice.Sum(x => x.TotalAmount) != listInvoice.Sum(x => x.TotalPaid))
                //    {
                //        ViewBag.startIndex = 2;
                //    }
                //    else
                //    {
                //        if (listShipment.Count == 0)
                //        {
                //            ViewBag.startIndex = 3;
                //        }
                //        else
                //        {
                //            if (db.ShippingItems.Where(x => x.Shippings_Id == id && x.Shipments_Id != null && x.Delivery_Status != null).Count() == 0)
                //            {
                //                ViewBag.startIndex = 4;
                //            }
                //            else
                //            {
                //                ViewBag.startIndex = 5;
                //            }
                //        }
                //    }
                //}
            }

            ShippingServices.Shipping = shipping;
            ShippingServices.ListInvoice = listInvoice;
            ShippingServices.ListPayment = listPayment;
            ShippingServices.ListDocument = listDocument;
            ShippingServices.ListShipment = listShipment;
            ShippingServices.ListPackage = listPackage;
            return View(ShippingServices);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create([Bind(Include = "Id,Customers_Id,No,Timestamp,Origin_Stations_Id,Destination_Stations_Id,Address,Notes")] ShippingsModels shippingsModels, string Items_Content)
        //{
        //    if (string.IsNullOrEmpty(Items_Content))
        //    {
        //        ModelState.AddModelError("Items", "No items selected for this package.");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        shippingsModels.Id = Guid.NewGuid();
        //        shippingsModels.Timestamp = DateTime.Now;
        //        db.Shippings.Add(shippingsModels);

        //        List<ShippingItemDetails> listDetails = JsonConvert.DeserializeObject<List<ShippingItemDetails>>(Items_Content);
        //        foreach (var item in listDetails)
        //        {
        //            ShippingItemsModels shippingItemsModels;
        //            if (string.IsNullOrEmpty(item.id))
        //            {
        //                shippingItemsModels = new ShippingItemsModels();
        //                shippingItemsModels.Id = Guid.NewGuid();
        //                shippingItemsModels.Shippings_Id = shippingsModels.Id;
        //                shippingItemsModels.No = item.no;
        //                shippingItemsModels.Length = item.length;
        //                shippingItemsModels.Width = item.width;
        //                shippingItemsModels.Height = item.height;
        //                shippingItemsModels.Weight = item.weight;
        //                shippingItemsModels.Notes = item.notes;
        //                shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
        //                db.ShippingItems.Add(shippingItemsModels);
        //            }
        //            else
        //            {
        //                shippingItemsModels = await db.ShippingItems.Where(x => x.Id.ToString() == item.id).FirstOrDefaultAsync();
        //                shippingItemsModels.Shippings_Id = shippingsModels.Id;
        //                shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
        //                db.Entry(shippingItemsModels).State = EntityState.Modified;
        //            }
        //        }

        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    var customers = db.Customers.OrderBy(x => x.FirstName).ToList();
        //    List<object> newList = new List<object>();
        //    foreach (var customer in customers)
        //    {
        //        var fName = customer.FirstName;
        //        var mName = customer.MiddleName;
        //        var lName = customer.LastName;
        //        string fullName = fName;
        //        if (!string.IsNullOrEmpty(mName)) { fullName += " " + mName; }
        //        if (!string.IsNullOrEmpty(lName)) { fullName += " " + lName; }

        //        newList.Add(new
        //        {
        //            Id = customer.Id,
        //            Name = fullName
        //        });
        //    }

        //    ViewBag.listCustomers = new SelectList(newList, "Id", "Name");
        //    ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
        //    return View(shippingsModels);
        //}

        public async Task<JsonResult> SaveShipping(Guid? shipping_id, Guid customer_id, string company, Guid origin_id, Guid destination_id, string receiver, string address, string address2, string city, string state, string country, string country_code, string postal_code, string mobile, string phone, string fax, string email, string tax_number, string notes, string shipping_items)
        {
            string status;
            ShippingsModels shippingsModels;

            if (shipping_id == null || shipping_id == Guid.Empty)
            {
                status = "new";
                shippingsModels = new ShippingsModels();
                shippingsModels.Id = Guid.NewGuid();
                shippingsModels.Customers_Id = customer_id;
                shippingsModels.Company = company;
                Common.Master m = new Common.Master();
                shippingsModels.No = m.GetLastHexResetDay();
                shippingsModels.Timestamp = DateTime.Now;
                shippingsModels.Origin_Stations_Id = origin_id;
                shippingsModels.Destination_Stations_Id = destination_id;
                shippingsModels.ReceiverName = receiver;
                shippingsModels.Address = address;
                shippingsModels.Address2 = address2;
                shippingsModels.City = city;
                shippingsModels.State = state;
                shippingsModels.Country = country;
                shippingsModels.CountryCode = country_code;
                shippingsModels.PostalCode = postal_code;
                shippingsModels.Phone1 = mobile;
                shippingsModels.Phone2 = phone;
                shippingsModels.Fax = fax;
                shippingsModels.Email = email;
                shippingsModels.TaxNumber = tax_number;
                shippingsModels.Notes = notes;
                shippingsModels.Status_enumid = ShippingStatusEnum.Shipping;
                db.Shippings.Add(shippingsModels);

                List<ShippingItemDetails> listDetails = JsonConvert.DeserializeObject<List<ShippingItemDetails>>(shipping_items);
                foreach (var item in listDetails)
                {
                    ShippingItemsModels shippingItemsModels;
                    if (string.IsNullOrEmpty(item.id))
                    {
                        shippingItemsModels = new ShippingItemsModels();
                        shippingItemsModels.Id = Guid.NewGuid();
                        shippingItemsModels.Shippings_Id = shippingsModels.Id;
                        shippingItemsModels.No = m.GetLastHexAllTime("PKG");
                        shippingItemsModels.Length = item.length;
                        shippingItemsModels.Width = item.width;
                        shippingItemsModels.Height = item.height;
                        shippingItemsModels.Weight = item.weight;
                        shippingItemsModels.DeclaredPrice = item.price;
                        shippingItemsModels.CourierInfo = item.courier;
                        shippingItemsModels.Notes = item.notes;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        shippingItemsModels.Invoiced = false;
                        shippingItemsModels.TrackingNo = Common.Master.GetTrackingNo(Common.Master.GetRandomHexNumber(10)); //generate tracking no for manual package
                        shippingItemsModels.Description = item.desc;
                        db.ShippingItems.Add(shippingItemsModels);

                        TrackingModels tr = new TrackingModels();
                        tr.Id = Guid.NewGuid();
                        tr.Ref_Id = shippingItemsModels.Id; //Shipping Items Id
                        tr.Timestamp = DateTime.Now;
                        tr.Description = "Processed for Shipping";
                        db.Tracking.Add(tr);
                    }
                    else
                    {
                        shippingItemsModels = await db.ShippingItems.Where(x => x.Id.ToString() == item.id).FirstOrDefaultAsync();
                        shippingItemsModels.Shippings_Id = shippingsModels.Id;
                        shippingItemsModels.DeclaredPrice = item.price;
                        shippingItemsModels.CourierInfo = item.courier;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        shippingItemsModels.Invoiced = false;
                        db.Entry(shippingItemsModels).State = EntityState.Modified;

                        List<ShippingItemContentsModels> list_item = db.ShippingItemContents.Where(x => x.ShippingItems_Id == shippingItemsModels.Id).ToList();
                        foreach (var subitem in list_item)
                        {
                            OrderItemsModels orderItemsModels = db.OrderItems.Where(x => x.Id == subitem.OrderItems_Id).FirstOrDefault();
                            OrdersModels ordersModels = db.Orders.Where(x => x.Id == orderItemsModels.Orders_Id).FirstOrDefault();
                            ordersModels.Status_enumid = OrderStatusEnum.Shipping;
                            db.Entry(ordersModels).State = EntityState.Modified;

                            TrackingModels tr = new TrackingModels();
                            tr.Id = Guid.NewGuid();
                            tr.Ref_Id = subitem.OrderItems_Id;
                            tr.Timestamp = DateTime.Now;
                            tr.Description = "Processed for Shipping";
                            db.Tracking.Add(tr);
                        }
                    }
                }
            }
            else
            {
                status = "edit";
                shippingsModels = await db.Shippings.FindAsync(shipping_id);
            }

            await db.SaveChangesAsync();
            return Json(new
            {
                status, id = shippingsModels.Id,
                no_shipping = db.Stations.Where(x=>x.Id==shippingsModels.Origin_Stations_Id).FirstOrDefault().Code
                    + db.Stations.Where(x => x.Id == shippingsModels.Destination_Stations_Id).FirstOrDefault().Code
                    + shippingsModels.Timestamp.ToString("MM")
                    + shippingsModels.Timestamp.ToString("dd") 
                    + shippingsModels.No
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Invoice(Guid? id)
        {
            var shipping = await (from s in db.Shippings
                               join c in db.Customers on s.Customers_Id equals c.Id
                               where s.Id == id
                               select new { s, c }).FirstOrDefaultAsync();
            string fullName = shipping.c.FirstName;
            if (!string.IsNullOrEmpty(shipping.c.MiddleName)) { fullName += " " + shipping.c.MiddleName; }
            if (!string.IsNullOrEmpty(shipping.c.LastName)) { fullName += " " + shipping.c.LastName; }

            ViewBag.ShippingId = id;
            ViewBag.Shipping = fullName + " (" + shipping.s.No + ")";
            ViewBag.ListShippingItem = await db.ShippingItems.Where(x => x.Shippings_Id == id && x.Invoiced == false).OrderBy(x => x.No).ToListAsync();
            ViewBag.ListPrice = new SelectList(db.ShippingPrices.OrderBy(x => x.Description).ToList(), "Id", "Description");

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
                ShippingsModels shippingsModels = await db.Shippings.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
                shippingsModels.Status_enumid = ShippingStatusEnum.WaitingPayment;
                db.Entry(shippingsModels).State = EntityState.Modified;

                int counter = db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id).Count();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.No = db.Stations.Where(x=>x.Id==shippingsModels.Origin_Stations_Id).FirstOrDefault().Code
                    + db.Stations.Where(x => x.Id == shippingsModels.Destination_Stations_Id).FirstOrDefault().Code
                    + shippingsModels.Timestamp.ToString("MM")
                    + shippingsModels.Timestamp.ToString("dd")
                    + shippingsModels.No
                    + "-" + counter;
                db.Invoices.Add(invoicesModels);

                List<ShippingItemsModels> listShippingItems = await db.ShippingItems.Where(x => x.Shippings_Id == invoicesModels.Ref_Id && x.Invoiced == false).ToListAsync();
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

                    foreach (var oi in listShippingItems)
                    {
                        if (item.desc == oi.Description)
                        {
                            oi.Invoiced = true;
                            db.Entry(oi).State = EntityState.Modified;
                        }
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Create", "Shipping", new { id = invoicesModels.Ref_Id });
            }

            var shipping = await (from s in db.Shippings
                                  join c in db.Customers on s.Customers_Id equals c.Id
                                  where s.Id == invoicesModels.Ref_Id
                                  select new { s, c }).FirstOrDefaultAsync();
            string fullName = shipping.c.FirstName;
            if (!string.IsNullOrEmpty(shipping.c.MiddleName)) { fullName += " " + shipping.c.MiddleName; }
            if (!string.IsNullOrEmpty(shipping.c.LastName)) { fullName += " " + shipping.c.LastName; }

            ViewBag.ShippingId = invoicesModels.Ref_Id;
            ViewBag.Shipping = fullName + " (" + shipping.s.No + ")";
            ViewBag.ListShippingItem = await db.ShippingItems.Where(x => x.Shippings_Id == invoicesModels.Ref_Id && x.Invoiced == false).OrderBy(x => x.No).ToListAsync();
            ViewBag.ListPrice = new SelectList(db.ShippingPrices.OrderBy(x => x.Description).ToList(), "Id", "Description");
            return View(invoicesModels);
        }

        public async Task<ActionResult> DeleteInvoice(Guid? id)
        {
            var data = (from i in db.Invoices
                        join s in db.Shippings on i.Ref_Id equals s.Id
                        join c in db.Customers on s.Customers_Id equals c.Id
                        where i.Id == id
                        select new InvoicesIndexViewModels
                        {
                            Id = i.Id,
                            Timestamp = i.Timestamp,
                            No = s.No,
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

            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Shipping", new { id = invoicesModels.Ref_Id });
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
            ViewBag.ShippingId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Payment([Bind(Include = "Id,Invoices_Id,Timestamp,Amount,PaymentInfo,Notes")] PaymentsModels paymentsModels, Guid Shipping_Id)
        {
            decimal remaining = 0;
            var invoice = db.Invoices.AsNoTracking().Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefault();
            if (invoice != null)
            {
                remaining = invoice.TotalAmount - invoice.TotalPaid;
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

                if (paymentsModels.Amount == remaining)
                {
                    ShippingsModels shippingsModels = await db.Shippings.Where(x => x.Id == Shipping_Id).FirstOrDefaultAsync();
                    shippingsModels.Status_enumid = ShippingStatusEnum.Documents;
                    db.Entry(shippingsModels).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Shipping", new { id = invoice.Ref_Id });
            }
            
            var invoices = await db.Invoices.Where(x => x.Ref_Id == Shipping_Id && x.TotalPaid != x.TotalAmount).OrderBy(x => x.No).ToListAsync();
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
            ViewBag.ShippingId = Shipping_Id;
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
        }

        [HttpPost, ActionName("DeletePayment")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePaymentConfirmed(Guid id)
        {
            PaymentsModels paymentsModels = await db.Payments.FindAsync(id);
            db.Payments.Remove(paymentsModels);

            InvoicesModels invoicesModels = await db.Invoices.Where(x => x.Id == paymentsModels.Invoices_Id).FirstOrDefaultAsync();
            invoicesModels.TotalPaid -= paymentsModels.Amount;
            db.Entry(invoicesModels).State = EntityState.Modified;
            
            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Shipping", new { id = invoicesModels.Ref_Id });
        }

        public async Task<ActionResult> UploadDocument(Guid? id)
        {
            ShippingItemsModels shippingItemsModels = await db.ShippingItems.Where(x => x.Id == id).FirstOrDefaultAsync();
            FileUploadsModels fileUploadsModels = new FileUploadsModels();
            fileUploadsModels.Ref_Id = shippingItemsModels.Id;
            ViewBag.Package = "PKG" + shippingItemsModels.No + " (Dimension: " + shippingItemsModels.Length + "cm x " + shippingItemsModels.Width + "cm x " + shippingItemsModels.Height + "cm)";

            return View(fileUploadsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadDocument([Bind(Include = "Id,Ref_Id,OriginalFilename,Description,Notes")] FileUploadsModels fileUploadsModels, List<HttpPostedFileBase> files)
        {
            ShippingItemsModels shippingItemsModels = await db.ShippingItems.Where(x => x.Id == fileUploadsModels.Ref_Id).FirstOrDefaultAsync();

            if (files[0] == null)
            {
                ModelState.AddModelError("Document", "Document(s) file is required.");
            }

            if (ModelState.IsValid)
            {
                string Dir = Server.MapPath("~/assets/document/");
                if (!Directory.Exists(Dir))
                {
                    DirectoryInfo di = Directory.CreateDirectory(Dir);
                }

                //var existedDocuments = db.FileUploads.Where(x => x.Ref_Id == fileUploadsModels.Ref_Id).ToList();
                //foreach (var item in existedDocuments)
                //{
                //    db.FileUploads.Remove(item);
                //    string fileName = item.Id.ToString() + "_" + item.Description + Path.GetExtension(item.OriginalFilename);
                //    if (System.IO.File.Exists(Dir + fileName))
                //        System.IO.File.Delete(Dir + fileName);
                //}

                foreach (HttpPostedFileBase file in files)
                {
                    FileUploadsModels fu = new FileUploadsModels();
                    fu.Id = Guid.NewGuid();
                    fu.Ref_Id = fileUploadsModels.Ref_Id;
                    fu.OriginalFilename = file.FileName;
                    fu.Description = fileUploadsModels.Description;
                    fu.Notes = fileUploadsModels.Notes;
                    db.FileUploads.Add(fu);

                    file.SaveAs(Path.Combine(Dir, fu.Id.ToString() + "_" + fu.Description + Path.GetExtension(file.FileName)));
                }

                ShippingsModels shippingsModels = await db.Shippings.Where(x => x.Id == shippingItemsModels.Shippings_Id).FirstOrDefaultAsync();
                if ((int)shippingsModels.Status_enumid <= 3)
                {
                    shippingsModels.Status_enumid = ShippingStatusEnum.OnShipments;
                    db.Entry(shippingsModels).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();

                return RedirectToAction("Create", "Shipping", new { id = shippingItemsModels.Shippings_Id });
            }
            
            ViewBag.Package = shippingItemsModels.No + " (Dimension: " + shippingItemsModels.Length + "cm x " + shippingItemsModels.Width + "cm x " + shippingItemsModels.Height + "cm)";
            return View(fileUploadsModels);
        }

        public ActionResult Download(Guid id)
        {
            FileUploadsModels fileUploadsModels = db.FileUploads.Where(x => x.Id == id).FirstOrDefault();
            string path = Server.MapPath("~/assets/document/" + id.ToString() + "_" + fileUploadsModels.Description + Path.GetExtension(fileUploadsModels.OriginalFilename));
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(path, contentType, Path.GetFileName(path));
        }

        public ActionResult DeleteDocumentUpload(Guid id)
        {
            FileUploadsModels fileUploadsModels = db.FileUploads.Where(x => x.Id == id).FirstOrDefault();
            db.FileUploads.Remove(fileUploadsModels);

            string Dir = Server.MapPath("~/assets/document/");
            string fileName = fileUploadsModels.Id.ToString() + "_" + fileUploadsModels.Description + Path.GetExtension(fileUploadsModels.OriginalFilename);
            if (System.IO.File.Exists(Dir + fileName))
                System.IO.File.Delete(Dir + fileName);

            db.SaveChanges();

            ShippingItemsModels shippingItemsModels = db.ShippingItems.Where(x => x.Id == fileUploadsModels.Ref_Id).FirstOrDefault();
            return RedirectToAction("Create", "Shipping", new { id = shippingItemsModels.Shippings_Id });
        }

        public async Task<ActionResult> DeliveryLog(Guid? id)
        {
            ShippingItemsModels shippingItemsModels = await db.ShippingItems.Where(x => x.Id == id).FirstOrDefaultAsync();
            return View(shippingItemsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeliveryLog([Bind(Include = "Id,Shippings_Id,No,Length,Width,Height,Weight,Notes,Status_enumid,Invoiced,Shipments_Id,Delivery_Status,TrackingNo")] ShippingItemsModels shippingItemsModels, string Description)
        {
            db.Entry(shippingItemsModels).State = EntityState.Modified;

            DeliveryLogModels deliveryLogModels = new DeliveryLogModels();
            deliveryLogModels.Id = Guid.NewGuid();
            deliveryLogModels.ShippingItem_Id = shippingItemsModels.Id;
            deliveryLogModels.Timestamp = DateTime.Now;
            if (string.IsNullOrEmpty(Description))
            {
                deliveryLogModels.Description = "[" + Enum.GetName(typeof(DeliveryItemStatusEnum), shippingItemsModels.Delivery_Status) + "]";
            }
            else
            {
                deliveryLogModels.Description = "[" + Enum.GetName(typeof(DeliveryItemStatusEnum), shippingItemsModels.Delivery_Status) + "] " + Description;
            }
            deliveryLogModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            db.DeliveryLog.Add(deliveryLogModels);

            if (shippingItemsModels.Delivery_Status == DeliveryItemStatusEnum.LocalDelivery || shippingItemsModels.Delivery_Status == DeliveryItemStatusEnum.Completed)
            {
                if (string.IsNullOrEmpty(shippingItemsModels.TrackingNo))
                {
                    List<ShippingItemContentsModels> list_item = db.ShippingItemContents.Where(x => x.ShippingItems_Id == shippingItemsModels.Id).ToList();
                    foreach (var item in list_item)
                    {
                        TrackingModels tr = new TrackingModels();
                        tr.Id = Guid.NewGuid();
                        tr.Ref_Id = item.OrderItems_Id;
                        tr.Timestamp = DateTime.Now;
                        tr.Description =
                            (shippingItemsModels.Delivery_Status == DeliveryItemStatusEnum.LocalDelivery)
                            ? "Sent Out to Local Courier"
                            : "Pick up by Customer";
                        db.Tracking.Add(tr);
                    }
                }
                else //manual package
                {
                    TrackingModels tr = new TrackingModels();
                    tr.Id = Guid.NewGuid();
                    tr.Ref_Id = shippingItemsModels.Id; //shipping item id
                    tr.Timestamp = DateTime.Now;
                    tr.Description =
                        (shippingItemsModels.Delivery_Status == DeliveryItemStatusEnum.LocalDelivery)
                        ? "Sent Out to Local Courier"
                        : "Pick up by Customer";
                    db.Tracking.Add(tr);
                }
            }

            ShippingsModels shippingsModels = await db.Shippings.Where(x => x.Id == shippingItemsModels.Shippings_Id).FirstOrDefaultAsync();
            if (shippingItemsModels.Delivery_Status == DeliveryItemStatusEnum.Completed)
            {
                shippingsModels.Status_enumid = ShippingStatusEnum.Completed;
            }
            else
            {
                shippingsModels.Status_enumid = ShippingStatusEnum.OnDelivery;
            }
            db.Entry(shippingsModels).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return RedirectToAction("Create", "Shipping", new { id = shippingItemsModels.Shippings_Id });
        }

        public async Task<ActionResult> Label(Guid id)
        {
            ShippingItemsModels shippingItemsModels = await db.ShippingItems.Where(x => x.Id == id).FirstOrDefaultAsync();
            var shipping = (from s in db.Shippings
                            join os in db.Stations on s.Origin_Stations_Id equals os.Id
                            join ds in db.Stations on s.Destination_Stations_Id equals ds.Id
                            join c in db.Customers on s.Customers_Id equals c.Id
                            where s.Id == shippingItemsModels.Shippings_Id
                            select new { s, os, ds, c }).FirstOrDefault();
            ViewBag.NoShipping = shipping.os.Code + shipping.ds.Code + shipping.s.Timestamp.ToString("MM") + shipping.s.Timestamp.ToString("dd") + shipping.s.No;
            ViewBag.Origin = shipping.os.Name;
            ViewBag.Destination = shipping.ds.Name;
            ViewBag.Customer = shipping.c.FirstName + " " + shipping.c.MiddleName + " " + shipping.c.LastName;
            ViewBag.Address = shipping.s.Address;

            return View(shippingItemsModels);
        }
    }
}