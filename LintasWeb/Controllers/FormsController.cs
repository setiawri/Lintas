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
    public class FormsController : Controller
    {
        private LintasContext db = new LintasContext();

        #region Save Forms
        public async Task<JsonResult> SaveForms(string Customer_FirstName, string Customer_MiddleName, string Customer_LastName, string Customer_Company, string Customer_Address, string Customer_Address2, string Customer_City, string Customer_State, Guid Customer_Countries_Id, string Customer_PostalCode, string Customer_Phone1, string Customer_Phone2, string Customer_Fax, string Customer_Email, string Customer_Notes
            , Guid Shipping_Origin_Stations_Id, Guid Shipping_Destination_Stations_Id, string Shipping_ReceiverName, string Shipping_Company, string Shipping_Address, string Shipping_Address2, string Shipping_City, string Shipping_State, string Shipping_Country, string Shipping_PostalCode, string Shipping_Phone1, string Shipping_Phone2, string Shipping_Fax, string Shipping_Email, string Shipping_TaxNumber, string Shipping_Notes)
        {
            CustomerFormsModels customerFormsModels = new CustomerFormsModels
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Customer_FirstName = Customer_FirstName,
                Customer_MiddleName = Customer_MiddleName,
                Customer_LastName = Customer_LastName,
                Customer_Company = Customer_Company,
                Customer_Address = Customer_Address,
                Customer_Address2 = Customer_Address2,
                Customer_City = Customer_City,
                Customer_State = Customer_State,
                Customer_Countries_Id = Customer_Countries_Id,
                Customer_PostalCode = Customer_PostalCode,
                Customer_Phone1 = Customer_Phone1,
                Customer_Phone2 = Customer_Phone2,
                Customer_Fax = Customer_Fax,
                Customer_Email = Customer_Email,
                Customer_Notes = Customer_Notes,
                Shipping_Origin_Stations_Id = Shipping_Origin_Stations_Id,
                Shipping_Destination_Stations_Id = Shipping_Destination_Stations_Id,
                Shipping_ReceiverName = Shipping_ReceiverName,
                Shipping_Company = Shipping_Company,
                Shipping_Address = Shipping_Address,
                Shipping_Address2 = Shipping_Address2,
                Shipping_City = Shipping_City,
                Shipping_State = Shipping_State,
                Shipping_Country = Shipping_Country,
                Shipping_PostalCode = Shipping_PostalCode,
                Shipping_Phone1 = Shipping_Phone1,
                Shipping_Phone2 = Shipping_Phone2,
                Shipping_Fax = Shipping_Fax,
                Shipping_Email = Shipping_Email,
                Shipping_TaxNumber = Shipping_TaxNumber,
                Shipping_Notes = Shipping_Notes,
                Status = CustomerFormsStatusEnum.None
            };
            db.CustomerForms.Add(customerFormsModels);
            await db.SaveChangesAsync();

            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Cancel Forms
        public async Task<JsonResult> Cancelled(Guid id)
        {
            var customer_forms = await db.CustomerForms.FindAsync(id);
            customer_forms.Status = CustomerFormsStatusEnum.Cancelled;
            db.Entry(customer_forms).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Undo Cancel Forms
        public async Task<JsonResult> UndoCancelled(Guid id)
        {
            var customer_forms = await db.CustomerForms.FindAsync(id);
            customer_forms.Status = CustomerFormsStatusEnum.None;
            db.Entry(customer_forms).State = EntityState.Modified;

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Generate Shipping
        public async Task<JsonResult> GenerateShipping(Guid CustomerForms_Id, string Items_Content
            , string Customer_FirstName, string Customer_MiddleName, string Customer_LastName, string Customer_Company, string Customer_Address, string Customer_Address2, string Customer_City, string Customer_State, Guid Customer_Countries_Id, string Customer_PostalCode, string Customer_Phone1, string Customer_Phone2, string Customer_Fax, string Customer_Email, string Customer_Notes
            , Guid Shipping_Origin_Stations_Id, Guid Shipping_Destination_Stations_Id, string Shipping_ReceiverName, string Shipping_Company, string Shipping_Address, string Shipping_Address2, string Shipping_City, string Shipping_State, string Shipping_Country, string Shipping_PostalCode, string Shipping_Phone1, string Shipping_Phone2, string Shipping_Fax, string Shipping_Email, string Shipping_TaxNumber, string Shipping_Notes)
        {
            #region Shippings Add
            Common.Master m = new Common.Master();
            ShippingsModels shippingsModels = new ShippingsModels
            {
                Id = Guid.NewGuid(),
                Customers_Id = Guid.Empty,
                Company = Shipping_Company,
                No = m.GetLastHexResetDay(),
                Timestamp = DateTime.UtcNow,
                Origin_Stations_Id = Shipping_Origin_Stations_Id,
                Destination_Stations_Id = Shipping_Destination_Stations_Id,
                ReceiverName = Shipping_ReceiverName,
                Address = Shipping_Address,
                Address2 = Shipping_Address2,
                City = Shipping_City,
                State = Shipping_State,
                Country = Shipping_Country,
                //CountryCode = country_code,
                PostalCode = Shipping_PostalCode,
                Phone1 = Shipping_Phone1,
                Phone2 = Shipping_Phone2,
                Fax = Shipping_Fax,
                Email = Shipping_Email,
                TaxNumber = Shipping_TaxNumber,
                Notes = Shipping_Notes,
                Status_enumid = ShippingStatusEnum.Shipping
            };
            db.Shippings.Add(shippingsModels);
            #endregion
            #region Shipping Items Add
            List<ShippingItemDetails> listDetails = JsonConvert.DeserializeObject<List<ShippingItemDetails>>(Items_Content);
            foreach (var item in listDetails)
            {
                if (string.IsNullOrEmpty(item.id))
                {
                    ShippingItemsModels shippingItemsModels = new ShippingItemsModels
                    {
                        Id = Guid.NewGuid(),
                        Shippings_Id = shippingsModels.Id,
                        No = m.GetLastHexAllTime("PKG"),
                        Length = item.length,
                        Width = item.width,
                        Height = item.height,
                        Weight = item.weight,
                        DeclaredPrice = item.price,
                        CourierInfo = item.courier,
                        Notes = item.notes,
                        Status_enumid = ShippingItemStatusEnum.Closed,
                        Invoiced = false,
                        TrackingNo = Common.Master.GetTrackingNo(Common.Master.GetRandomHexNumber(10)), //generate tracking no for manual package
                        Description = item.desc
                    };
                    db.ShippingItems.Add(shippingItemsModels);

                    TrackingModels tr = new TrackingModels
                    {
                        Id = Guid.NewGuid(),
                        Ref_Id = shippingItemsModels.Id, //Shipping Items Id
                        Timestamp = DateTime.UtcNow,
                        Description = "Processed for Shipping"
                    };
                    db.Tracking.Add(tr);
                }
                await db.SaveChangesAsync();
            }
            #endregion
            #region Customer Forms Update
            CustomerFormsModels customerFormsModels = await db.CustomerForms.FindAsync(CustomerForms_Id);
            customerFormsModels.Customer_FirstName = Customer_FirstName;
            customerFormsModels.Customer_MiddleName = Customer_MiddleName;
            customerFormsModels.Customer_LastName = Customer_LastName;
            customerFormsModels.Customer_Company = Customer_Company;
            customerFormsModels.Customer_Address = Customer_Address;
            customerFormsModels.Customer_Address2 = Customer_Address2;
            customerFormsModels.Customer_City = Customer_City;
            customerFormsModels.Customer_State = Customer_State;
            customerFormsModels.Customer_Countries_Id = Customer_Countries_Id;
            customerFormsModels.Customer_PostalCode = Customer_PostalCode;
            customerFormsModels.Customer_Phone1 = Customer_Phone1;
            customerFormsModels.Customer_Phone2 = Customer_Phone2;
            customerFormsModels.Customer_Fax = Customer_Fax;
            customerFormsModels.Customer_Email = Customer_Email;
            customerFormsModels.Customer_Notes = Customer_Notes;
            customerFormsModels.Shipping_Origin_Stations_Id = Shipping_Origin_Stations_Id;
            customerFormsModels.Shipping_Destination_Stations_Id = Shipping_Destination_Stations_Id;
            customerFormsModels.Shipping_ReceiverName = Shipping_ReceiverName;
            customerFormsModels.Shipping_Company = Shipping_Company;
            customerFormsModels.Shipping_Address = Shipping_Address;
            customerFormsModels.Shipping_Address2 = Shipping_Address2;
            customerFormsModels.Shipping_City = Shipping_City;
            customerFormsModels.Shipping_State = Shipping_State;
            customerFormsModels.Shipping_Country = Shipping_Country;
            customerFormsModels.Shipping_PostalCode = Shipping_PostalCode;
            customerFormsModels.Shipping_Phone1 = Shipping_Phone1;
            customerFormsModels.Shipping_Phone2 = Shipping_Phone2;
            customerFormsModels.Shipping_Fax = Shipping_Fax;
            customerFormsModels.Shipping_Email = Shipping_Email;
            customerFormsModels.Shipping_TaxNumber = Shipping_TaxNumber;
            customerFormsModels.Shipping_Notes = Shipping_Notes;
            customerFormsModels.Shippings_Id = shippingsModels.Id;
            customerFormsModels.Status = CustomerFormsStatusEnum.Approved;
            db.Entry(customerFormsModels).State = EntityState.Modified;
            #endregion

            await db.SaveChangesAsync();
            return Json(new { status = "200" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Index()
        {
            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            
            ViewBag.Title = "Customer Forms";
            ViewBag.startIndex = 0;
            return View();
        }

        public ActionResult Completed()
        {
            return View();
        }

        public async Task<ActionResult> Customer()
        {
            var items = await (from cf in db.CustomerForms
                               join o in db.Stations on cf.Shipping_Origin_Stations_Id equals o.Id
                               join d in db.Stations on cf.Shipping_Destination_Stations_Id equals d.Id
                               join u in db.User on o.Id equals u.Stations_Id
                               select new { cf, o, d }).ToListAsync();
            List<CustomerFormsViewModels> list = new List<CustomerFormsViewModels>();
            foreach (var item in items)
            {
                list.Add(new CustomerFormsViewModels
                {
                    Id = item.cf.Id,
                    Timestamp = TimeZoneInfo.ConvertTimeFromUtc(item.cf.Timestamp, TimeZoneInfo.FindSystemTimeZoneById(item.o.TimeZone)),
                    CustomerName = item.cf.Customer_FirstName + " " + item.cf.Customer_MiddleName + " " + item.cf.Customer_LastName,
                    ReceiverName = item.cf.Shipping_ReceiverName,
                    Origin = item.o.Name,
                    Destination = item.d.Name,
                    ShippingAddress = item.cf.Shipping_Address,
                    ShippingMobile = item.cf.Shipping_Phone1,
                    Status = item.cf.Status
                });
            }

            return View(list);
        }

        public async Task<ActionResult> Process(Guid id)
        {
            CustomerFormsModels customerFormsModels = await db.CustomerForms.FindAsync(id);
            CustomersModels customersModels = new CustomersModels
            {
                FirstName = customerFormsModels.Customer_FirstName,
                MiddleName = customerFormsModels.Customer_MiddleName,
                LastName = customerFormsModels.Customer_LastName,
                Company = customerFormsModels.Customer_Company,
                Address = customerFormsModels.Customer_Address,
                Address2 = customerFormsModels.Customer_Address2,
                City = customerFormsModels.Customer_City,
                State = customerFormsModels.Customer_State,
                Countries_Id = customerFormsModels.Customer_Countries_Id,
                Zipcode = customerFormsModels.Customer_PostalCode,
                Phone1 = customerFormsModels.Customer_Phone1,
                Phone2 = customerFormsModels.Customer_Phone2,
                Fax = customerFormsModels.Customer_Fax,
                Email = customerFormsModels.Customer_Email,
                Notes = customerFormsModels.Customer_Notes
            };
            ShippingsModels shippingsModels = new ShippingsModels
            {
                Origin_Stations_Id = customerFormsModels.Shipping_Origin_Stations_Id,
                Destination_Stations_Id = customerFormsModels.Shipping_Destination_Stations_Id,
                ReceiverName = customerFormsModels.Shipping_ReceiverName,
                Company = customerFormsModels.Shipping_Company,
                Address = customerFormsModels.Shipping_Address,
                Address2 = customerFormsModels.Shipping_Address2,
                City = customerFormsModels.Shipping_City,
                State = customerFormsModels.Shipping_State,
                Country = customerFormsModels.Shipping_Country,
                PostalCode = customerFormsModels.Shipping_PostalCode,
                Phone1 = customerFormsModels.Shipping_Phone1,
                Phone2 = customerFormsModels.Shipping_Phone2,
                Fax = customerFormsModels.Shipping_Fax,
                Email = customerFormsModels.Shipping_Email,
                TaxNumber = customerFormsModels.Shipping_TaxNumber,
                Notes = customerFormsModels.Shipping_Notes
            };

            CustomerFormsIndexViewModels models = new CustomerFormsIndexViewModels
            {
                Customer = customersModels,
                Shipping = shippingsModels
            };

            ViewBag.listCountries = new SelectList(db.Countries.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listStations = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.CustomerForms_Id = id;
            ViewBag.startIndex = 0;
            return View(models);
        }
    }
}