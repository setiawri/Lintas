﻿using LintasMVC.Models;
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
    public class ShippingController : Controller
    {
        private LintasContext db = new LintasContext();

        public async Task<ActionResult> Index()
        {
            var data = (from s in db.Shippings
                        join o in db.Stations on s.Origin_Stations_Id equals o.Id
                        join d in db.Stations on s.Destination_Stations_Id equals d.Id
                        select new ShippingsViewModels
                        {
                            Id = s.Id,
                            No = s.No,
                            Timestamp = s.Timestamp,
                            Origin = o.Name,
                            Destination = d.Name,
                            Notes = s.Notes
                        }).ToListAsync();
            return View(await data);
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

        public async Task<ActionResult> Create(Guid? id)
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
            ShippingServicesModels ShippingServices = new ShippingServicesModels();
            if (id == null || id == Guid.Empty)
            {
                shipping = new ShippingsModels();
                listInvoice = new List<InvoicesModels>();
                listPayment = new List<PaymentsIndexViewModels>();
                ViewBag.startIndex = 0;
            }
            else
            {
                shipping = await db.Shippings.Where(x => x.Id == id).FirstOrDefaultAsync();
                ViewBag.listItems = db.ShippingItems.Where(x => x.Shippings_Id == id).ToList();
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

                int count_inv = await db.Invoices.Where(x => x.Ref_Id == id).CountAsync();
                if (count_inv > 0)
                {
                    ViewBag.startIndex = 2;
                }
                else
                {
                    ViewBag.startIndex = 1;
                }
            }

            ShippingServices.Shipping = shipping;
            ShippingServices.ListInvoice = listInvoice;
            ShippingServices.ListPayment = listPayment;
            return View(ShippingServices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Customers_Id,No,Timestamp,Origin_Stations_Id,Destination_Stations_Id,Address,Notes")] ShippingsModels shippingsModels, string Items_Content)
        {
            if (string.IsNullOrEmpty(Items_Content))
            {
                ModelState.AddModelError("Items", "No items selected for this package.");
            }

            if (ModelState.IsValid)
            {
                shippingsModels.Id = Guid.NewGuid();
                shippingsModels.Timestamp = DateTime.Now;
                db.Shippings.Add(shippingsModels);

                List<ShippingItemDetails> listDetails = JsonConvert.DeserializeObject<List<ShippingItemDetails>>(Items_Content);
                foreach (var item in listDetails)
                {
                    ShippingItemsModels shippingItemsModels;
                    if (string.IsNullOrEmpty(item.id))
                    {
                        shippingItemsModels = new ShippingItemsModels();
                        shippingItemsModels.Id = Guid.NewGuid();
                        shippingItemsModels.Shippings_Id = shippingsModels.Id;
                        shippingItemsModels.No = item.no;
                        shippingItemsModels.Length = item.length;
                        shippingItemsModels.Width = item.width;
                        shippingItemsModels.Height = item.height;
                        shippingItemsModels.Weight = item.weight;
                        shippingItemsModels.Notes = item.notes;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        db.ShippingItems.Add(shippingItemsModels);
                    }
                    else
                    {
                        shippingItemsModels = await db.ShippingItems.Where(x => x.Id.ToString() == item.id).FirstOrDefaultAsync();
                        shippingItemsModels.Shippings_Id = shippingsModels.Id;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        db.Entry(shippingItemsModels).State = EntityState.Modified;
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var customers = db.Customers.OrderBy(x => x.FirstName).ToList();
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
            return View(shippingsModels);
        }

        public async Task<JsonResult> SaveShipping(Guid? shipping_id, Guid customer_id, string no, Guid origin_id, Guid destination_id, string address, string notes, string shipping_items)
        {
            string status;
            ShippingsModels shippingsModels;

            if (shipping_id == null || shipping_id == Guid.Empty)
            {
                status = "new";
                shippingsModels = new ShippingsModels();
                shippingsModels.Id = Guid.NewGuid();
                shippingsModels.Customers_Id = customer_id;
                shippingsModels.No = no;
                shippingsModels.Timestamp = DateTime.Now;
                shippingsModels.Origin_Stations_Id = origin_id;
                shippingsModels.Destination_Stations_Id = destination_id;
                shippingsModels.Address = address;
                shippingsModels.Notes = notes;
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
                        shippingItemsModels.No = item.no;
                        shippingItemsModels.Length = item.length;
                        shippingItemsModels.Width = item.width;
                        shippingItemsModels.Height = item.height;
                        shippingItemsModels.Weight = item.weight;
                        shippingItemsModels.Notes = item.notes;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        shippingItemsModels.Invoiced = false;
                        db.ShippingItems.Add(shippingItemsModels);
                    }
                    else
                    {
                        shippingItemsModels = await db.ShippingItems.Where(x => x.Id.ToString() == item.id).FirstOrDefaultAsync();
                        shippingItemsModels.Shippings_Id = shippingsModels.Id;
                        shippingItemsModels.Status_enumid = ShippingItemStatusEnum.Closed;
                        shippingItemsModels.Invoiced = false;
                        db.Entry(shippingItemsModels).State = EntityState.Modified;
                    }
                }
            }
            else
            {
                status = "edit";
                shippingsModels = await db.Shippings.FindAsync(shipping_id);
            }

            await db.SaveChangesAsync();
            return Json(new { status = status, id = shippingsModels.Id, no_shipping = no }, JsonRequestBehavior.AllowGet);
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
                ShippingsModels shippingsModels = await db.Shippings.AsNoTracking().Where(x => x.Id == invoicesModels.Ref_Id).FirstOrDefaultAsync();
                int counter = db.Invoices.AsNoTracking().Where(x => x.Ref_Id == invoicesModels.Ref_Id).Count();
                invoicesModels.Id = Guid.NewGuid();
                invoicesModels.No = shippingsModels.Timestamp.ToString("yyyyMMdd") + shippingsModels.No + counter;
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
                        if (item.desc == oi.No)
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
            ViewBag.ListPrice = new SelectList(db.OrderPrices.OrderBy(x => x.Description).ToList(), "Id", "Description");
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
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ") Due " + string.Format("{0:N2}", inv.TotalAmount - inv.TotalPaid)
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
                    Name = inv.No + " - " + inv.Notes + " (" + string.Format("{0:N2}", inv.TotalAmount) + ") Due " + string.Format("{0:N2}", inv.TotalAmount - inv.TotalPaid)
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
    }
}