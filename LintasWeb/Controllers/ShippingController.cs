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

        public ActionResult Create()
        {
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
            return View();
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
    }
}