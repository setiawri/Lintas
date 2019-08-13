using LintasMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Common
{
    public class Master
    {
        public int GetLastNo(DateTime tanggal)
        {
            int result;
            using (var ctx = new LintasContext())
            {
                result = ctx.Orders.Where(x => x.Timestamp >= tanggal && x.Timestamp <= tanggal).Select(x => x.No).Cast<int>().DefaultIfEmpty(0).Max();
            }

            return result + 1;
        }

        public string GetLastHexAllTime(string prefix) // prefix => CS / CS+ / PKG / SHP
        {
            string last_hex_string = "";
            using (var ctx = new LintasContext())
            {
                if (prefix == "CS") { last_hex_string = ctx.Orders.Where(x => x.Service == prefix).Max(x => x.No ?? string.Empty); }
                else if (prefix == "CS+") { last_hex_string = ctx.Orders.Where(x => x.Service == prefix).Max(x => x.No ?? string.Empty); }
                else if (prefix == "PKG") { last_hex_string = ctx.ShippingItems.Max(x => x.No ?? string.Empty); }
                else if (prefix == "SHP") { last_hex_string = ctx.Shipments.Max(x => x.No ?? string.Empty); }
            }
            int last_hex_int = int.Parse(string.IsNullOrEmpty(last_hex_string) ? 0.ToString("X5") : last_hex_string, System.Globalization.NumberStyles.HexNumber);

            return (last_hex_int + 1).ToString("X5");
        }

        public string GetLastHexResetDay()
        {
            string last_hex_string = "";
            using (var ctx = new LintasContext())
            {
                DateTime from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0);
                DateTime to = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 59);
                last_hex_string = ctx.Shippings.Where(x => x.Timestamp >= from && x.Timestamp <= to).Max(x => x.No ?? string.Empty);
            }
            int last_hex_int = int.Parse(string.IsNullOrEmpty(last_hex_string) ? 0.ToString("X3") : last_hex_string, System.Globalization.NumberStyles.HexNumber);

            return (last_hex_int + 1).ToString("X3");
        }

        public string GetTotalOrderItem(Guid order_id)
        {
            string result;
            using (var ctx = new LintasContext())
            {
                decimal sum = ctx.OrderItems.Where(x => x.Orders_Id == order_id).Sum(x => x.Amount);
                int count = ctx.OrderItems.Where(x => x.Orders_Id == order_id).Count();
                result = string.Format("{0:N2}", sum) + " (" + string.Format("{0:N0}", count) + " items)";
            }

            return result;
        }

        static Random random = new Random();
        public static string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        public static string GetTrackingNo(string no)
        {
            int count_oi = 0;
            int count_si = 0;
            using (var ctx = new LintasContext())
            {
                count_oi = ctx.OrderItems.Where(x => x.TrackingNo == no).ToList().Count;
                count_si = ctx.ShippingItems.Where(x => x.TrackingNo == no).ToList().Count;
            }

            if (count_oi == 0 && count_si == 0)
                return no;

            return GetTrackingNo(GetRandomHexNumber(10));
        }
    }
}