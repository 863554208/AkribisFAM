using AkribisFAM.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Models
{
    public class LotRecord
    {
        public string LotID { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Creator { get; set; }
        public int LotState { get; set; }
        public string RecipeName { get; set; }
       
    }
}
