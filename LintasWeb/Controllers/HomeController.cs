using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LintasMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public static void GetDataTZ()
        {
            const string OUTPUTFILENAME = @"D:\TimeZoneInfo.txt";

            DateTimeFormatInfo dateFormats = CultureInfo.CurrentCulture.DateTimeFormat;
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            StreamWriter sw = new StreamWriter(OUTPUTFILENAME, false);

            foreach (TimeZoneInfo timeZone in timeZones)
            {
                bool hasDST = timeZone.SupportsDaylightSavingTime;
                TimeSpan offsetFromUtc = timeZone.BaseUtcOffset;
                TimeZoneInfo.AdjustmentRule[] adjustRules;
                string offsetString;

                sw.WriteLine("ID: {0}", timeZone.Id);
                sw.WriteLine("   Display Name: {0, 40}", timeZone.DisplayName);
                sw.WriteLine("   Standard Name: {0, 39}", timeZone.StandardName);
                sw.Write("   Daylight Name: {0, 39}", timeZone.DaylightName);
                sw.Write(hasDST ? "   ***Has " : "   ***Does Not Have ");
                sw.WriteLine("Daylight Saving Time***");
                offsetString = String.Format("{0} hours, {1} minutes", offsetFromUtc.Hours, offsetFromUtc.Minutes);
                sw.WriteLine("   Offset from UTC: {0, 40}", offsetString);
                adjustRules = timeZone.GetAdjustmentRules();
                sw.WriteLine("   Number of adjustment rules: {0, 26}", adjustRules.Length);
                if (adjustRules.Length > 0)
                {
                    sw.WriteLine("   Adjustment Rules:");
                    foreach (TimeZoneInfo.AdjustmentRule rule in adjustRules)
                    {
                        TimeZoneInfo.TransitionTime transTimeStart = rule.DaylightTransitionStart;
                        TimeZoneInfo.TransitionTime transTimeEnd = rule.DaylightTransitionEnd;

                        sw.WriteLine("      From {0} to {1}", rule.DateStart, rule.DateEnd);
                        sw.WriteLine("      Delta: {0}", rule.DaylightDelta);
                        if (!transTimeStart.IsFixedDateRule)
                        {
                            sw.WriteLine("      Begins at {0:t} on {1} of week {2} of {3}", transTimeStart.TimeOfDay,
                                                                                          transTimeStart.DayOfWeek,
                                                                                          transTimeStart.Week,
                                                                                          dateFormats.MonthNames[transTimeStart.Month - 1]);
                            sw.WriteLine("      Ends at {0:t} on {1} of week {2} of {3}", transTimeEnd.TimeOfDay,
                                                                                          transTimeEnd.DayOfWeek,
                                                                                          transTimeEnd.Week,
                                                                                          dateFormats.MonthNames[transTimeEnd.Month - 1]);
                        }
                        else
                        {
                            sw.WriteLine("      Begins at {0:t} on {1} {2}", transTimeStart.TimeOfDay,
                                                                           transTimeStart.Day,
                                                                           dateFormats.MonthNames[transTimeStart.Month - 1]);
                            sw.WriteLine("      Ends at {0:t} on {1} {2}", transTimeEnd.TimeOfDay,
                                                                         transTimeEnd.Day,
                                                                         dateFormats.MonthNames[transTimeEnd.Month - 1]);
                        }
                    }
                }
            }
            sw.Close();
        }

        public ActionResult Index()
        {
            //GetDataTZ();
            ViewBag.UTC = DateTime.UtcNow;
            ViewBag.Jakarta = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            ViewBag.Melbourne = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}