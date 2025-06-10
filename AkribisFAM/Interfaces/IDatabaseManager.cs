using AkribisFAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkribisFAM.Interfaces
{
    public interface IDatabaseManager : IDisposable
    {
        bool AddAlarm(AlarmRecord alarm);
        bool AddOeeRecord(OeeRecord oee);
        bool AddLotRecord(LotRecord lot);

        List<AlarmRecord> GetAlarms(/*DateTime from, DateTime to*/); // TODO: search with filter
        List<OeeRecord> GetOeeRecords(/*DateTime from, DateTime to*/); // TODO: search with filter
        List<LotRecord> GetLotRecords(); 

    }
}
