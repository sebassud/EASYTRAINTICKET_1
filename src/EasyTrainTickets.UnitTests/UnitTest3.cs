using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Desktop.ViewModels;
using EasyTrainTickets.Desktop.Work;
using System.Collections.Generic;
using Moq;
using System.Linq;
using System.Data.Entity;

namespace EasyTrainTickets.UnitTests
{
    [TestClass]
    public class UnitTest3
    {
        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }

        [TestMethod]
        public void Test_AddConnections()
        {
            // Arrange
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            List<ConnectionPart> listConnectionPart = new List<ConnectionPart>();
            List<Connection> listConnection = new List<Connection>();
            mock.Setup(m => m.ConnectionParts).Returns(GetQueryableMockDbSet<ConnectionPart>(listConnectionPart));
            mock.Setup(m => m.Connections).Returns(GetQueryableMockDbSet<Connection>(listConnection));
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Trains).Returns(db.Trains);
            UnitOfWork unit = new UnitOfWork(mock.Object);
            Connection con = new Connection();
            con.StartPlace = "Warszawa";
            con.EndPlace = "Kraków";
            con.Id = 2;
            con.Train = new Train() { Type="Ekspres"};

            // Act
            unit.AddConnection(con);

            // Assert
            Assert.AreEqual(1, listConnection.Count);
            Assert.AreEqual(2, listConnection[0].Id);
        }


        [TestMethod]
        public void Test_Search()
        {
            // Arrange
            IUnitOfWork unityOfWork = new UnitOfWork(new EasyTrainTicketsDbEntities());

            // Act           
            List<ConnectionPath> conp0 = unityOfWork.Search("Warszawa", "Kraków", DateTime.Now.Date.AddDays(1));
            List<ConnectionPath> conp1 = unityOfWork.Search("Białystok", "Wrocław", DateTime.Now.Date.AddDays(1));

            // Assert
            Assert.AreEqual(conp0.Count, 10);
            Assert.AreEqual(conp1.Count, 10);

        }

        [TestMethod]
        public void Test_GetSeets()
        {
            // Arrange
            ConnectionPath conpath = new ConnectionPath();
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            conpath.Add(new ConnectionPart() { Id = 1 });
            conpath.Add(new ConnectionPart() { Id = 2 });
            conpath.Add(new ConnectionPart() { Id = 3 });
            conpath.Add(new ConnectionPart() { Id = 4 });
            conpath.Add(new ConnectionPart() { Id = 5 });
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            List<ConnectionPart> listConnectionPart = new List<ConnectionPart>();
            listConnectionPart.Add(new ConnectionPart() { Id = 1, Seats = "1;2;3;4;5" });
            listConnectionPart.Add(new ConnectionPart() { Id = 3, Seats = "3;4;5" });
            listConnectionPart.Add(new ConnectionPart() { Id = 5, Seats = "" });
            listConnectionPart.Add(new ConnectionPart() { Id = 2, Seats = "1;2;3" });
            listConnectionPart.Add(new ConnectionPart() { Id = 4, Seats = "1" });
            mock.Setup(m => m.ConnectionParts).Returns(GetQueryableMockDbSet<ConnectionPart>(listConnectionPart));
            mock.Setup(m => m.Routes).Returns(db.Routes);
            UnitOfWork unitOfWork = new UnitOfWork(mock.Object);

            // Act
            List<int> seats0 = unitOfWork.GetSeats(conpath, 0, 0);
            List<int> seats1 = unitOfWork.GetSeats(conpath, 0, 2);
            List<int> seats2 = unitOfWork.GetSeats(conpath, 0, 4);
            List<int> seats3 = unitOfWork.GetSeats(conpath, 2, 3);

            // Assert
            Assert.AreEqual(5, seats0.Count);
            Assert.AreEqual(1, seats1.Count);
            Assert.AreEqual(3, seats1[0]);
            Assert.AreEqual(0, seats2.Count);
            Assert.AreEqual(0, seats3.Count);
        }
    }
}
