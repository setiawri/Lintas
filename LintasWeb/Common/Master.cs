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
    }
}