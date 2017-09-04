using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Data
{
    public partial class TimeZones
    {
        public static List<TimeZones> Get()
        {
            using (var db = new RelationalDatabaseEntities())
                return db.TimeZones.ToList();
        }
    }
}
