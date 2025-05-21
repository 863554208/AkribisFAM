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
        void AddAlarm(AlarmRecord alarm);

        // Add more methods as needed, e.g.
        // AlarmRecord GetAlarmById(int id);
        // List<AlarmRecord> GetUnresolvedAlarms();
    }
}
