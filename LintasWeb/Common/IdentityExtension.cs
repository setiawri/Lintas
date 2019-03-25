using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace LintasMVC.Common
{
    public static class IdentityExtension
    {
        public static string GetFullName(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                return ci.FindFirstValue("Fullname");
            }
            return null;
        }

        public static string GetStation(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var ci = identity as ClaimsIdentity;
            if (ci != null)
            {
                string station;
                using (var ctx = new LintasMVC.Models.LintasContext())
                {
                    var data = (from s in ctx.Stations
                                join u in ctx.User on s.Id equals u.Stations_Id
                                join c in ctx.Countries on s.Countries_Id equals c.Id
                                where u.UserName == ci.Name
                                select new { s, u, c }).FirstOrDefault();
                    station = (data == null) ? "N/A" : data.s.Name + ", " + data.c.Name;
                }
                return station;
            }
            return null;
        }
    }
}