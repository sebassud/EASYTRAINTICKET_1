using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using System.Linq;

namespace EasyTrainTickets.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestOpenDatabase()
        {
            // Arrange
            using (EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities())
            {
                // Act
                db.Database.Initialize(false);
                // Assert
                Assert.IsNotNull(db);
                Assert.IsTrue(0 < db.ConnectionParts.Count());
                Assert.IsTrue(0 < db.Connections.Count());
                Assert.IsTrue(0 < db.Routes.Count());
                Assert.IsTrue(0 < db.Users.Count());
                Assert.IsTrue(0 < db.Trains.Count());
            }
        }

    }
}
