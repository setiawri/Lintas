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
    public class OrdersController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: Orders
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
                            Notes = o.Notes
                        }).ToListAsync();
            return View(await data);
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
        public async Task<ActionResult> Create([Bind(Include = "Id,No,Timestamp,Customers_Id,Origin_Stations_Id,Destination_Stations_Id,Notes,Status_enumid")] OrdersModels ordersModels, string Items)
        {
            if (ModelState.IsValid)
            {
                Common.Master m = new Common.Master();
                ordersModels.Id = Guid.NewGuid();
                ordersModels.No = m.GetLastNo(ordersModels.Timestamp).ToString("000");
                ordersModels.Status_enumid = OrderStatusEnum.Ordered;
                db.Orders.Add(ordersModels);

                List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(Items);
                foreach (var item in lOrderItem)
                {
                    OrderItemsModels oi = new OrderItemsModels();
                    oi.Id = Guid.NewGuid();
                    oi.Orders_Id = ordersModels.Id;
                    oi.Description = item.desc;
                    oi.Cost = item.cost;
                    oi.Notes = item.note;
                    db.OrderItems.Add(oi);
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
            return View(ordersModels);
        }

        public async Task<ActionResult> Edit(Guid? id)
        {
            OrdersModels ordersModels = await db.Orders.FindAsync(id);
            ViewBag.listItems = db.OrderItems.Where(x => x.Orders_Id == id).ToList();

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
            return View(ordersModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,No,Timestamp,Customers_Id,Origin_Stations_Id,Destination_Stations_Id,Notes,Status_enumid")] OrdersModels ordersModels, string Items)
        {
            if (ModelState.IsValid)
            {
                List<OrderItemsModels> lOrderItem_before = db.OrderItems.Where(x => x.Orders_Id == ordersModels.Id).ToList();
                foreach (var item in lOrderItem_before)
                {
                    db.OrderItems.Remove(item);
                }

                db.Entry(ordersModels).State = EntityState.Modified;

                List<OrderItemDetails> lOrderItem = JsonConvert.DeserializeObject<List<OrderItemDetails>>(Items);
                foreach (var item in lOrderItem)
                {
                    OrderItemsModels oi = new OrderItemsModels();
                    oi.Id = Guid.NewGuid();
                    oi.Orders_Id = ordersModels.Id;
                    oi.Description = item.desc;
                    oi.Cost = item.cost;
                    oi.Notes = item.note;
                    db.OrderItems.Add(oi);
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
            return View(ordersModels);
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
            List<OrderItemsModels> lOrderItem = await db.OrderItems.Where(x => x.Orders_Id == id).ToListAsync();
            foreach (var item in lOrderItem)
            {
                db.OrderItems.Remove(item);
            }
            OrdersModels ordersModels = await db.Orders.FindAsync(id);
            db.Orders.Remove(ordersModels);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}