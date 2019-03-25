using LintasMVC.Common;
using LintasMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private LintasContext db = new LintasContext();

        // GET: User
        public ActionResult Index()
        {
            Permissions p = new Permissions();
            bool auth = p.isGranted(User.Identity.Name, this.ControllerContext.RouteData.Values["controller"].ToString() + "_" + this.ControllerContext.RouteData.Values["action"].ToString());
            if (!auth) { return new ViewResult() { ViewName = "Unauthorized" }; }
            else
            {
                var user = (from u in db.User
                            join ur in db.UserRole on u.Id equals ur.UserId
                            join r in db.Role on ur.RoleId equals r.Id
                            join s in db.Stations
                                .Join(db.User, x => x.Id, y => y.Stations_Id, (x,y) => new { Station = x.Name , StationId = y.Stations_Id})
                            on u.Stations_Id equals s.StationId into joined
                            from station in joined.DefaultIfEmpty()
                            orderby u.Fullname ascending
                            select new UserViewModels
                            {
                                Id = u.Id,
                                Fullname = u.Fullname,
                                UserName = u.UserName,
                                Email = u.Email,
                                Role = r.Name,
                                RoleId = r.Id,
                                Station = (station.StationId == null) ? string.Empty : station.Station,
                                Notes = u.Notes
                            });

                return View(user.ToList());
            }
        }

        public ActionResult Edit(string id)
        {
            var user = (from u in db.User
                        join ur in db.UserRole on u.Id equals ur.UserId
                        join r in db.Role on ur.RoleId equals r.Id
                        where u.Id == id
                        select new UserViewModels
                        {
                            Id = u.Id,
                            Fullname = u.Fullname,
                            UserName = u.UserName,
                            Email = u.Email,
                            Role = r.Name,
                            RoleId = r.Id,
                            Stations_Id = u.Stations_Id,
                            Notes = u.Notes
                        }).FirstOrDefault();
            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listStation = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Fullname,UserName,Email,Role,RoleId,Notes,Station,Stations_Id")] UserViewModels userViewModels)
        {
            if (ModelState.IsValid)
            {
                var user = new UserModels()
                {
                    Id = userViewModels.Id,
                    Fullname = userViewModels.Fullname,
                    UserName = userViewModels.UserName,
                    Email = userViewModels.Email,
                    Stations_Id = userViewModels.Stations_Id,
                    Notes = userViewModels.Notes
                };
                var userRole = new UserRoleModels() { UserId = userViewModels.Id, RoleId = userViewModels.RoleId };

                using (var database = new LintasContext())
                {
                    database.User.Attach(user);
                    database.Entry(user).Property(x => x.Fullname).IsModified = true;
                    database.Entry(user).Property(x => x.UserName).IsModified = true;
                    database.Entry(user).Property(x => x.Email).IsModified = true;
                    database.Entry(user).Property(x => x.Stations_Id).IsModified = true;
                    database.Entry(user).Property(x => x.Notes).IsModified = true;

                    database.UserRole.Attach(userRole);
                    database.Entry(userRole).Property(x => x.RoleId).IsModified = true;

                    database.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewBag.listRole = new SelectList(db.Role.OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.listStation = new SelectList(db.Stations.OrderBy(x => x.Name).ToList(), "Id", "Name");
            return View(userViewModels);
        }
    }
}