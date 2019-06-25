using LintasMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    public class TrackingController : Controller
    {
        private LintasContext db = new LintasContext();

        public ActionResult Index(string no)
        {
            ViewBag.NoTracking = no;
            return View();
        }

        public JsonResult GetData(string no)
        {
            var data = (from oi in db.OrderItems
                        join tr in db.Tracking on oi.Id equals tr.Ref_Id
                        where oi.TrackingNo == no
                        orderby tr.Timestamp descending
                        select new { tr }).ToList();

            string status = (data.Count > 0) ? "200" : "404";

            string result = "";
            if (data.Count > 0)
            {
                int row = 0;
                foreach (var item in data)
                {
                    string color = (row > 0) ? "text-danger" : "text-success";
                    string icon = "";
                    if (item.tr.Description.Contains("Orders")) { icon = "icon-copy"; }
                    else if (item.tr.Description.Contains("Payment")) { icon = "icon-cash"; }
                    else if (item.tr.Description.Contains("Supplier")) { icon = "icon-profile"; }
                    else if (item.tr.Description.Contains("Processed")) { icon = "icon-airplane2"; }
                    else if (item.tr.Description.Contains("Forwarders")) { icon = "icon-box"; }
                    else if (item.tr.Description.Contains("Station")) { icon = "icon-office"; }
                    else if (item.tr.Description.Contains("Courier")) { icon = "icon-truck"; }
                    else { icon = "icon-check"; }

                    result += @"<tr>
                                    <td class='text-center'>
                                        <i class='" + icon + @" " + color + @"'></i>
                                    </td>
                                    <td>[ " + item.tr.Timestamp.ToString("yyyy/MM/dd HH:mm") + " ] " + item.tr.Description + @"</td>
                                </tr>";
                    row++;
                }
            }

            return Json(new { status = status, result = result }, JsonRequestBehavior.AllowGet);
        }
    }
}