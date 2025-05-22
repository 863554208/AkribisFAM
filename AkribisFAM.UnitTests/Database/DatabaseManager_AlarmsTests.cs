using AkribisFAM.Manager;
using AkribisFAM.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace AkribisFAM.UnitTests.Database
{
    [TestClass]
    public class DatabaseManager_AlarmsTests : DatabaseManagerTestsBase
    {
        [TestMethod]
        public void AddAlarm_WithValidInput_ShouldReturnTrue()
        {
            // Arrange
            var alarm = new AlarmRecord
            {
                AlarmLevel = "High",
                AlarmCode = "A001",
                AlarmMessage = "Test alarm message",
                LotID = "Lot123",
                TimeOccurred = DateTime.Now,
                TimeResolved = null,
                UserID = "User01"
            };

            // Act
            var result = _dbManager.AddAlarm(alarm);

            // Assert
            Assert.IsTrue(result, "Alarm should be added successfully.");
        }
    }

}
