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
    public class ShipmentsController : Controller
    {
        private LintasContext db = new LintasContext();
        
        public async Task<ActionResult> Index()
        {
            var data = (from s in db.Shipments
                        join f in db.Forwarders on s.Forwarders_Id equals f.Id
                        select new ShipmentsIndexViewModels
                        {
                            Id = s.Id,
                            Timestamp = s.Timestamp,
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
            ViewBag.listItems = db.ShippingItems.Where(x => x.Shipments_Id == null).OrderBy(x => x.No).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Timestamp,Forwarders_Id,Notes")] ShipmentsModels shipmentsModels, string items_selected)
        {
            if (string.IsNullOrEmpty(items_selected))
            {
                ModelState.AddModelError("Items", "The Items checked is required.");
            }

            if (ModelState.IsValid)
            {
                shipmentsModels.Id = Guid.NewGuid();
                shipmentsModels.Timestamp = DateTime.Now;
                db.Shipments.Add(shipmentsModels);

                string[] array_ids = items_selected.Split(',');
                foreach (string s in array_ids)
                {
                    ShippingItemsModels model = await db.ShippingItems.Where(x => x.Id.ToString() == s).FirstOrDefaultAsync();
                    model.Shipments_Id = shipmentsModels.Id;
                    db.Entry(model).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.listForwarders = new SelectList(db.Forwarders.Where(x => x.Active == true).OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listItems = db.ShippingItems.Where(x => x.Shipments_Id == null).OrderBy(x => x.No).ToList();
            return View(shipmentsModels);
        }

        public async Task<ActionResult> Log(Guid? id)
        {
            ShipmentsModels shipmentsModels = await db.Shipments.Where(x => x.Id == id).FirstOrDefaultAsync();
            return View(shipmentsModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Log([Bind(Include = "Id,Timestamp,Forwarders_Id,Notes,Status_enumid")] ShipmentsModels shipmentsModels, string Description)
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
    }
}