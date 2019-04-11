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
    }
}