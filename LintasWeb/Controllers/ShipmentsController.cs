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
    public class ShipmentsController : Controller
    {
        private LintasContext db = new LintasContext();
        
        public async Task<ActionResult> Index(string no)
        {
            ViewBag.No = no;
            var data = (from s in db.Shipments
                        join f in db.Forwarders on s.Forwarders_Id equals f.Id
                        select new ShipmentsIndexViewModels
                        {
                            Id = s.Id,
                            Timestamp = s.Timestamp,
                            No = s.No,
                            Forwarders = f.Name,
                            Notes = s.Notes,
                            Status_enumid = s.Status_enumid
                        }).ToListAsync();
            return View(await data);
        }

        public JsonResult GetShippingItem(Guid id)
        {
            var list = db.ShippingItems.Where(x => x.Shipments_Id == id).ToList();
            string message = @"<div class='table-responsive'>
                                    <table class='table table-striped table-bordered'>
                                        <thead>
                                            <tr>
                                                <th>No</th>
                                                <th>Declared Price</th>
                                                <th>Courier Info</th>
                                                <th>Dimension (cm)</th>
                                                <th>Weight (gr)</th>
                                                <th>Notes</th>
                                                <th>Document</th>
                                            </tr>
                                        </thead>
                                        <tbody>";
            foreach (var item in list)
            {
                string price = (item.DeclaredPrice.HasValue) ? item.DeclaredPrice.Value.ToString("#,##0.00") : "0";
                message += @"<tr>
                                <td>PKG" + item.No + @"</td>
                                <td>" + price + @"</td>
                                <td>" + item.CourierInfo + @"</td>
                                <td>" + item.Length + " x " + item.Width + " x " + item.Height + @"</td>
                                <td>" + item.Weight.ToString("#,##0") + @"</td>
                                <td>" + item.Notes + @"</td>
                                <td><a href='" + Url.Content("~") + "Shipments/Report/" + item.Id + @"'>Edit</a></td>
                            </tr>";
            }
            message += "</tbody></table></div>";

            return Json(new { content = message }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLogs(Guid id)
        {
            List<ShipmentLogModels> itemLogs = db.ShipmentLog.Where(x => x.Shipments_Id == id).OrderByDescending(x => x.Timestamp).ToList();
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
            foreach (ShipmentLogModels item in itemLogs)
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

        public ActionResult Create()
        {
            ViewBag.listForwarders = new SelectList(db.Forwarders.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listItems = db.ShippingItems.Where(x => x.Shipments_Id == null && x.Invoiced == true).OrderBy(x => x.No).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Timestamp,No,Forwarders_Id,AWB,Notes")] ShipmentsModels shipmentsModels, string items_selected, string Items_List)
        {
            if (string.IsNullOrEmpty(items_selected))
            {
                ModelState.AddModelError("Items", "The Items checked is required.");
            }

            if (ModelState.IsValid)
            {
                shipmentsModels.Id = Guid.NewGuid();
                Common.Master m = new Common.Master();
                shipmentsModels.No = m.GetLastHexAllTime("SHP");
                shipmentsModels.Timestamp = DateTime.Now;
                db.Shipments.Add(shipmentsModels);

                string[] array_ids = items_selected.Split(',');
                foreach (string s in array_ids)
                {
                    //decimal declared_price = 0; string courier_info = "";

                    //List<ShipmentsDetails> item_list = JsonConvert.DeserializeObject<List<ShipmentsDetails>>(Items_List);
                    //foreach (var item in item_list)
                    //{
                    //    if (item.id == s)
                    //    {
                    //        declared_price = string.IsNullOrEmpty(item.price) ? 0 : decimal.Parse(item.price);
                    //        courier_info = item.courier;
                    //        break;
                    //    }
                    //}
                    
                    ShippingItemsModels model = await db.ShippingItems.Where(x => x.Id.ToString() == s).FirstOrDefaultAsync();
                    model.Shipments_Id = shipmentsModels.Id;
                    //model.DeclaredPrice = declared_price;
                    //model.CourierInfo = courier_info;
                    db.Entry(model).State = EntityState.Modified;

                    ShippingsModels shippingsModels = await db.Shippings.Where(x => x.Id == model.Shippings_Id).FirstOrDefaultAsync();
                    var customer = db.Customers.Where(x => x.Id == shippingsModels.Customers_Id).FirstOrDefault();
                    var country = db.Countries.Where(x => x.Id == customer.Countries_Id).FirstOrDefault();

                    ShipmentsReportModels shipmentsReportModels = new ShipmentsReportModels();
                    shipmentsReportModels.Id = Guid.NewGuid();
                    shipmentsReportModels.ShippingItems_Id = model.Id;
                    shipmentsReportModels.WaybillNumber = shipmentsModels.AWB;
                    shipmentsReportModels.ServiceNumber = db.Stations.Where(x => x.Id == shippingsModels.Origin_Stations_Id).FirstOrDefault().Code
                        + db.Stations.Where(x => x.Id == shippingsModels.Destination_Stations_Id).FirstOrDefault().Code
                        + shippingsModels.Timestamp.ToString("MM") + shippingsModels.Timestamp.ToString("dd")
                        + shippingsModels.No;
                    shipmentsReportModels.OriginCountry = db.Countries.Where(x => x.Id == db.Stations.Where(y => y.Id == shippingsModels.Origin_Stations_Id).FirstOrDefault().Countries_Id).FirstOrDefault().Name;
                    shipmentsReportModels.ParcelWeight = model.Weight;
                    shipmentsReportModels.ParcelLong = model.Length;
                    shipmentsReportModels.ParcelWide = model.Width;
                    shipmentsReportModels.ParcelHigh = model.Height;
                    shipmentsReportModels.ConsignmentDate = DateTime.Today;
                    shipmentsReportModels.TaxConsigneeNumber = shippingsModels.TaxNumber;

                    shipmentsReportModels.ConsigneeName = shippingsModels.ReceiverName;
                    shipmentsReportModels.ConsigneeCompany = shippingsModels.Company;
                    shipmentsReportModels.ConsigneePhone = shippingsModels.Phone2;
                    shipmentsReportModels.ConsigneeMobile = shippingsModels.Phone1;
                    shipmentsReportModels.ConsigneeFax = shippingsModels.Fax;
                    shipmentsReportModels.ConsigneeEmail = shippingsModels.Email;
                    shipmentsReportModels.ConsigneePostalCode = shippingsModels.PostalCode;
                    shipmentsReportModels.ConsigneeCountry = shippingsModels.Country;
                    shipmentsReportModels.ConsigneeCountryCode = shippingsModels.CountryCode;
                    shipmentsReportModels.ConsigneeState = shippingsModels.State;
                    shipmentsReportModels.ConsigneeCity = shippingsModels.City;
                    shipmentsReportModels.ConsigneeAddress1 = shippingsModels.Address;
                    shipmentsReportModels.ConsigneeAddress2 = shippingsModels.Address2;

                    shipmentsReportModels.ShipperName = customer.FirstName + " " + customer.MiddleName + " " + customer.LastName;
                    shipmentsReportModels.ShipperCompany = customer.Company;
                    shipmentsReportModels.ShipperPhone = customer.Phone2;
                    shipmentsReportModels.ShipperMobile = customer.Phone1;
                    shipmentsReportModels.ShipperFax = customer.Fax;
                    shipmentsReportModels.ShipperEmail = customer.Email;
                    shipmentsReportModels.ShipperPostalCode = customer.Zipcode;
                    shipmentsReportModels.ShipperCountry = country.Name;
                    shipmentsReportModels.ShipperCountryCode = country.Code;
                    shipmentsReportModels.ShipperState = customer.State;
                    shipmentsReportModels.ShipperCity = customer.City;
                    shipmentsReportModels.ShipperAddress1 = customer.Address;
                    shipmentsReportModels.ShipperAddress2 = customer.Address2;

                    shipmentsReportModels.ParcelQty = 1;
                    shipmentsReportModels.ProductQty = 1;
                    shipmentsReportModels.ProductDescription = model.Description;
                    shipmentsReportModels.DeclarationPrice = model.DeclaredPrice.Value;
                    shipmentsReportModels.Currency = "AUD$";
                    var forwarder = db.Forwarders.Where(x => x.Id == shipmentsModels.Forwarders_Id).FirstOrDefault();
                    shipmentsReportModels.BillingCode = forwarder.BillingCode;
                    shipmentsReportModels.BillingAccount = forwarder.BillingAccount;
                    shipmentsReportModels.BrokerName = forwarder.Name;
                    shipmentsReportModels.BrokerPhone = forwarder.Phone1;
                    db.ShipmentsReport.Add(shipmentsReportModels);

                    if (string.IsNullOrEmpty(model.TrackingNo))
                    {
                        List<ShippingItemContentsModels> list_item = db.ShippingItemContents.Where(x => x.ShippingItems_Id == model.Id).ToList();
                        foreach (var item in list_item)
                        {
                            TrackingModels tr = new TrackingModels();
                            tr.Id = Guid.NewGuid();
                            tr.Ref_Id = item.OrderItems_Id;
                            tr.Timestamp = DateTime.Now;
                            tr.Description = "Item sent to Forwarders";
                            db.Tracking.Add(tr);
                        }
                    }
                    else //manual package
                    {
                        TrackingModels tr = new TrackingModels();
                        tr.Id = Guid.NewGuid();
                        tr.Ref_Id = model.Id; //Shipping Items Id
                        tr.Timestamp = DateTime.Now;
                        tr.Description = "Item sent to Forwarders";
                        db.Tracking.Add(tr);
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listForwarders = new SelectList(db.Forwarders.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listItems = db.ShippingItems.Where(x => x.Shipments_Id == null && x.Invoiced == true).OrderBy(x => x.No).ToList();
            return View(shipmentsModels);
        }

        public async Task<ActionResult> Log(Guid? id)
        {
            ShipmentsModels shipmentsModels = await db.Shipments.Where(x => x.Id == id).FirstOrDefaultAsync();
            return View(shipmentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Log([Bind(Include = "Id,Timestamp,No,Forwarders_Id,Notes,Status_enumid")] ShipmentsModels shipmentsModels, string Description)
        {
            db.Entry(shipmentsModels).State = EntityState.Modified;

            ShipmentLogModels shipmentLogModels = new ShipmentLogModels();
            shipmentLogModels.Id = Guid.NewGuid();
            shipmentLogModels.Shipments_Id = shipmentsModels.Id;
            shipmentLogModels.Timestamp = DateTime.Now;
            if (string.IsNullOrEmpty(Description))
            {
                shipmentLogModels.Description = "[" + Enum.GetName(typeof(ShipmentItemStatusEnum), shipmentsModels.Status_enumid) + "]";
            }
            else
            {
                shipmentLogModels.Description = "[" + Enum.GetName(typeof(ShipmentItemStatusEnum), shipmentsModels.Status_enumid) + "] " + Description;
            }
            shipmentLogModels.UserAccounts_Id = db.User.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            db.ShipmentLog.Add(shipmentLogModels);

            if (shipmentsModels.Status_enumid == ShipmentItemStatusEnum.Completed)
            {
                bool isShipmentComplete = false;
                var list_shipping_item = db.ShippingItems.Where(x => x.Shipments_Id == shipmentsModels.Id).ToList();
                foreach (var sItem in list_shipping_item)
                {
                    #region Add Tracking
                    if (string.IsNullOrEmpty(sItem.TrackingNo))
                    {
                        List<ShippingItemContentsModels> list_item = db.ShippingItemContents.Where(x => x.ShippingItems_Id == sItem.Id).ToList();
                        foreach (var item in list_item)
                        {
                            TrackingModels tr = new TrackingModels();
                            tr.Id = Guid.NewGuid();
                            tr.Ref_Id = item.OrderItems_Id;
                            tr.Timestamp = DateTime.Now;
                            tr.Description = "Received at Destination Station";
                            db.Tracking.Add(tr);
                        }
                    }
                    else //manual package
                    {
                        TrackingModels tr = new TrackingModels();
                        tr.Id = Guid.NewGuid();
                        tr.Ref_Id = sItem.Id; //shipping items id
                        tr.Timestamp = DateTime.Now;
                        tr.Description = "Received at Destination Station";
                        db.Tracking.Add(tr);
                    }
                    #endregion
                    #region Check Shipping Item in same Shipping
                    var list_shipment_same_shipping = (from si in db.ShippingItems
                                                       join s in db.Shipments on si.Shipments_Id equals s.Id
                                                       join f in db.Forwarders on s.Forwarders_Id equals f.Id
                                                       where si.Shippings_Id == sItem.Shippings_Id && si.Id != sItem.Id
                                                       select new ShipmentsIndexViewModels
                                                       {
                                                           Id = s.Id,
                                                           Timestamp = s.Timestamp,
                                                           No = s.No,
                                                           Forwarders = f.Name,
                                                           Notes = s.Notes,
                                                           Status_enumid = s.Status_enumid
                                                       }).ToList();
                    if (list_shipment_same_shipping.Count == 0) { isShipmentComplete = true; }
                    else
                    {
                        foreach (var subitem in list_shipment_same_shipping)
                        {
                            if (subitem.Status_enumid == ShipmentItemStatusEnum.Completed)
                            {
                                isShipmentComplete = true;
                            }
                            else
                            {
                                isShipmentComplete = false; break;
                            }
                        }
                    }

                    if (isShipmentComplete)
                    {
                        ShippingsModels shippingsModels = db.Shippings.Where(x => x.Id == sItem.Shippings_Id).FirstOrDefault();
                        shippingsModels.Status_enumid = ShippingStatusEnum.ShipmentComplete;
                        db.Entry(shippingsModels).State = EntityState.Modified;
                    }
                    #endregion
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(Guid? id)
        {
            var data = (from s in db.Shipments
                        join f in db.Forwarders on s.Forwarders_Id equals f.Id
                        where s.Id == id
                        select new ShipmentsIndexViewModels
                        {
                            Id = s.Id,
                            Timestamp = s.Timestamp,
                            Forwarders = f.Name,
                            Notes = s.Notes,
                            Status_enumid = s.Status_enumid
                        }).FirstOrDefaultAsync();
            return View(await data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ShipmentsModels shipmentsModels = await db.Shipments.FindAsync(id);
            db.Shipments.Remove(shipmentsModels);

            var logs = await db.ShipmentLog.Where(x => x.Shipments_Id == id).ToListAsync();
            foreach (var item in logs)
            {
                ShipmentLogModels shipmentLogModels = db.ShipmentLog.Where(x => x.Id == item.Id).FirstOrDefault();
                db.ShipmentLog.Remove(shipmentLogModels);
            }

            var items = await db.ShippingItems.Where(x => x.Shipments_Id == id).ToListAsync();
            foreach (var item in items)
            {
                ShippingItemsModels shippingItemsModels = db.ShippingItems.Where(x => x.Id == item.Id).FirstOrDefault();
                shippingItemsModels.Shipments_Id = null;
                db.Entry(shippingItemsModels).State = EntityState.Modified;
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //public async Task<ActionResult> Create(Guid? id)
        //{
        //    ShipmentsModels shipmentsModels;
        //    ForwardersModels forwardersModels;
        //    List<ShippingItemsModels> list_items;
        //    ShipmentsViewModels shipmentsViewModels = new ShipmentsViewModels();

        //    if (id == null || id == Guid.Empty)
        //    {
        //        shipmentsModels = new ShipmentsModels();
        //        forwardersModels = new ForwardersModels();
        //        list_items = db.ShippingItems.Where(x => x.Shipments_Id == null).OrderBy(x => x.No).ToList();
        //        ViewBag.startIndex = 0;
        //    }
        //    else
        //    {
        //        shipmentsModels = await db.Shipments.Where(x => x.Id == id).FirstOrDefaultAsync();
        //        forwardersModels = db.Forwarders.Where(x => x.Id == shipmentsModels.Forwarders_Id).FirstOrDefault();
        //        list_items = db.ShippingItems.Where(x => x.Shipments_Id == id).OrderBy(x => x.No).ToList();
        //        ViewBag.startIndex = 1;
        //    }

        //    shipmentsViewModels.Shipments = shipmentsModels;
        //    shipmentsViewModels.Forwarders = forwardersModels;
        //    shipmentsViewModels.ListItems = list_items;
        //    return View(shipmentsViewModels);
        //}

        public async Task<JsonResult> SaveShipments(Guid? shipments_id, DateTime shipments_timestamp, string shipments_notes, string forwarders_name, string forwarders_address, string forwarders_phone1, string forwarders_phone2, string forwarders_notes, string id_items)
        {
            string status;
            ShipmentsModels shipmentsModels;

            if (shipments_id == null || shipments_id == Guid.Empty)
            {
                status = "new";

                ForwardersModels forwardersModels = new ForwardersModels();
                forwardersModels.Id = Guid.NewGuid();
                forwardersModels.Name = forwarders_name;
                forwardersModels.Address = forwarders_address;
                forwardersModels.Phone1 = forwarders_phone1;
                forwardersModels.Phone2 = forwarders_phone2;
                forwardersModels.Notes = forwarders_notes;
                //forwardersModels.Active = true;
                db.Forwarders.Add(forwardersModels);

                shipmentsModels = new ShipmentsModels();
                shipmentsModels.Id = Guid.NewGuid();
                shipmentsModels.Timestamp = shipments_timestamp;
                shipmentsModels.Forwarders_Id = forwardersModels.Id;
                shipmentsModels.Notes = shipments_notes;
                db.Shipments.Add(shipmentsModels);

                string[] array_ids = id_items.Split(',');
                foreach (string s in array_ids)
                {
                    ShippingItemsModels model = await db.ShippingItems.Where(x => x.Id.ToString() == s).FirstOrDefaultAsync();
                    model.Shipments_Id = shipmentsModels.Id;
                    db.Entry(model).State = EntityState.Modified;
                }
            }
            else
            {
                status = "edit";
                shipmentsModels = await db.Shipments.FindAsync(shipments_id);
            }

            await db.SaveChangesAsync();
            return Json(new { status = status, id = shipmentsModels.Id, name = forwarders_name }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Report(Guid id)
        {
            ShipmentsReportModels shipmentsReportModels = await db.ShipmentsReport.Where(x => x.ShippingItems_Id == id).FirstOrDefaultAsync();
            return View(shipmentsReportModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Report([Bind(Include = "Id,ShippingItems_Id,WaybillNumber,ServiceNumber,ConversionNumber,OriginCountry,ParcelWeight,ParcelLong,ParcelWide,ParcelHigh,ParcelVolume,ConsignmentDate,TaxConsigneeNumber,ConsigneeName,ConsigneeCompany,ConsigneePhone,ConsigneeMobile,ConsigneeFax,ConsigneeEmail,ConsigneePostalCode,ConsigneeCountry,ConsigneeCountryCode,ConsigneeState,ConsigneeCity,ConsigneeAddress1,ConsigneeAddress2,ShipperName,ShipperCompany,ShipperPhone,ShipperMobile,ShipperFax,ShipperEmail,ShipperPostalCode,ShipperCountry,ShipperCountryCode,ShipperState,ShipperCity,ShipperAddress1,ShipperAddress2,ParcelQty,ProductQty,ProductDescription,DeclarationPrice,Currency,BillingCode,BillingAccount,BrokerName,BrokerPhone,HsCode,FreightCost,Insurance,BagNo,PaymentType")] ShipmentsReportModels shipmentsReportModels)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shipmentsReportModels).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(shipmentsReportModels);
        }

        public async Task<ActionResult> Excel(Guid id)
        {
            var data = await (from sr in db.ShipmentsReport
                              join si in db.ShippingItems on sr.ShippingItems_Id equals si.Id
                              join s in db.Shipments on si.Shipments_Id equals s.Id
                              where s.Id == id
                              select new { sr }).ToListAsync();
            List<ShipmentsReportModels> listReport = new List<ShipmentsReportModels>();
            foreach (var item in data)
            {
                ShipmentsReportModels sr = item.sr;
                listReport.Add(sr);
            }

            Common.ExportExcel ee = new Common.ExportExcel();

            var fileDownloadName = db.Shipments.Where(x => x.Id == id).FirstOrDefault().No + ".xls";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            var package = ee.ForwarderReports(listReport);

            var fileStream = new MemoryStream();
            package.SaveAs(fileStream);
            fileStream.Position = 0;

            var fsr = new FileStreamResult(fileStream, contentType);
            fsr.FileDownloadName = fileDownloadName;

            return fsr;
        }
    }
}