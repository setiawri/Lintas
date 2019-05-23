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
                            Notes = s.Notes
                        }).ToListAsync();
            return View(await data);
        }

        public async Task<ActionResult> Create(Guid? id)
        {
            ShipmentsModels shipmentsModels;
            ForwardersModels forwardersModels;
            List<ShippingItemsModels> list_items;
            ShipmentsViewModels shipmentsViewModels = new ShipmentsViewModels();

            if (id == null || id == Guid.Empty)
            {
                shipmentsModels = new ShipmentsModels();
                forwardersModels = new ForwardersModels();
                list_items = db.ShippingItems.Where(x => x.Shipments_Id == null).OrderBy(x => x.No).ToList();
                ViewBag.startIndex = 0;
            }
            else
            {
                shipmentsModels = await db.Shipments.Where(x => x.Id == id).FirstOrDefaultAsync();
                forwardersModels = db.Forwarders.Where(x => x.Id == shipmentsModels.Forwarders_Id).FirstOrDefault();
                list_items = db.ShippingItems.Where(x => x.Shipments_Id == id).OrderBy(x => x.No).ToList();
                ViewBag.startIndex = 1;
            }
            
            shipmentsViewModels.Shipments = shipmentsModels;
            shipmentsViewModels.Forwarders = forwardersModels;
            shipmentsViewModels.ListItems = list_items;
            return View(shipmentsViewModels);
        }

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