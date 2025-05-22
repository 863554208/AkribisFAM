using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Models
{
    /// <summary>
    /// Represents a single alarm record.
    /// </summary>
    public class AlarmRecord
    {
        public string AlarmLevel { get; set; }
        public string AlarmCode { get; set; }
        public string AlarmMessage { get; set; }
        public string LotID { get; set; }
        public DateTime TimeOccurred { get; set; }
        public DateTime? TimeResolved { get; set; }  // Nullable if unresolved
        public string UserID { get; set; }
    }
}
