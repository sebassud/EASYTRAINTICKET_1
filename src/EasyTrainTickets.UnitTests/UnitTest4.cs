using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Desktop.ViewModels;
using EasyTrainTickets.Desktop.Work;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace EasyTrainTickets.UnitTests
{
    [TestClass]
    public class UnitTest4
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
        public void Test_CreateTicket()
        {
            // Arrange
            User user0 = new User() { Id=11, Tickets = "1-2|5-2|2-10;3-5" };
            User user1 = new User() { Id=21, Tickets = "" };
            User user2 = new User() { Id=1, Tickets = "1-2|5-2|2-10|2-14" };
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(new List<User>() { user0, user1, user2 }));
            UnitOfWork unitOfWork = new UnitOfWork(mock.Object);

            // Act
            List<Ticket> ticket0 = unitOfWork.CreateTicketsDo(user0);
            List<Ticket> ticket1 = unitOfWork.CreateTicketsDo(user1);
            List<Ticket> ticket2 = unitOfWork.CreateTicketsDo(user2);

            // Assert
            Assert.AreEqual(2, ticket0.Count);
            Assert.AreEqual(5, ticket0[0][1]);
            Assert.AreEqual(3, ticket0[1][0]);
            Assert.AreEqual(10, ticket0[0].seats[2]);
            Assert.AreEqual(0, ticket1.Count);
            Assert.AreEqual(1, ticket2.Count);
        }

        [TestMethod]
        public void Test_InitializeTickets()
        {
            // Arrange
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);

            Route route = new Route() { Distance = 100, From="stacja1", To="stacja2" };
            Connection connetion = new Connection() { Train = new Train() { Type = "Ekspres" } };
            ConnectionPart conpart0 = new ConnectionPart() { Route = route, Connection = connetion, StartTime = DateTime.Now, EndTime = DateTime.Now, Id = 1 };
            ConnectionPart conpart1 = new ConnectionPart() { Route = route, Connection = connetion, StartTime = DateTime.Now, EndTime = DateTime.Now, Id = 5 };
            ConnectionPart conpart2 = new ConnectionPart() { Route = route, Connection = connetion, StartTime = DateTime.Now, EndTime = DateTime.Now, Id = 2 };
            List<ConnectionPart> conparts = new List<ConnectionPart>();
            conparts.Add(conpart0);
            conparts.Add(conpart1);
            conparts.Add(conpart2);
            mock.Setup(m => m.ConnectionParts).Returns(GetQueryableMockDbSet<ConnectionPart>(conparts));          
            User user = new User() { Id=1, Tickets = "1-10|5-2|2-1" };
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(new List<User>() { user}));
            UnitOfWork unitOfWork = new UnitOfWork(mock.Object);
            List<Ticket> tickets = unitOfWork.CreateTicketsDo(user);

            // Act
            unitOfWork.InitializeTickets(tickets);
            
            // Assert
            Assert.AreEqual(1, tickets.Count);
            Assert.AreEqual(3, tickets[0].connectionsParts.Count);
            Assert.AreEqual(5, tickets[0].connectionsParts[1].Id);
        }

        [TestMethod]
        public void Test_BuyTicket()
        {
            // Arrange
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);

            List<ConnectionPart> listConnectionPart = new List<ConnectionPart>();
            listConnectionPart.Add(new ConnectionPart() { Id = 1, Seats = "1;2;3;4;5" ,FreeSeats=5});
            listConnectionPart.Add(new ConnectionPart() { Id = 2, Seats = "3;4;5", FreeSeats=3 });
            listConnectionPart.Add(new ConnectionPart() { Id = 4, Seats = "1;2;3", FreeSeats=3});
            listConnectionPart.Add(new ConnectionPart() { Id = 5, Seats = "1", FreeSeats=1 });
            mock.Setup(m => m.ConnectionParts).Returns(GetQueryableMockDbSet<ConnectionPart>(listConnectionPart));
            List<int> seats = new List<int>() { 1, 5, 2, 1 };
            Ticket ticket = new Ticket() { connectionsParts = listConnectionPart, seats = seats };
            User user0 = new User() { Tickets = "" };
            UnitOfWork unitOfWork = new UnitOfWork(mock.Object);
            // Act
            unitOfWork.BuyTicketDo(user0, ticket);

            // Assert
            Assert.AreEqual("2;3;4;5",ticket.connectionsParts[0].Seats);
            Assert.AreEqual("3;4", ticket.connectionsParts[1].Seats);
            Assert.AreEqual("1;3", ticket.connectionsParts[2].Seats);
            Assert.AreEqual("", ticket.connectionsParts[3].Seats);
            Assert.AreEqual(0, ticket.connectionsParts[3].FreeSeats);
            Assert.AreEqual("1-1|2-5|4-2|5-1", user0.Tickets);
        }

        [TestMethod]
        public void Test_DeleteTicket()
        {
            // Arrange
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            List<ConnectionPart> listConnectionPart = new List<ConnectionPart>();
            listConnectionPart.Add(new ConnectionPart() { Id = 1, Seats = "1;2;3;4;5", FreeSeats = 5 });
            listConnectionPart.Add(new ConnectionPart() { Id = 2, Seats = "1;2;3", FreeSeats = 3 });
            listConnectionPart.Add(new ConnectionPart() { Id = 3, Seats = "", FreeSeats = 0 });
            mock.Setup(m => m.ConnectionParts).Returns(GetQueryableMockDbSet<ConnectionPart>(listConnectionPart));
            UnitOfWork unitOfWork = new UnitOfWork(mock.Object);

            User user0 = new User() { Tickets = "1-10|2-23|3-100" };
            User user1 = new User() { Tickets = "1-34;1-20|2-13|3-20" };
            User user2 = new User() { Tickets = "1-50|2-43|3-11;45-70" };
            List<int> seats0 = new List<int>() { 10, 23, 100 };
            List<int> seats1 = new List<int>() { 20, 13, 20 };
            List<int> seats2 = new List<int>() { 50, 43, 11 };
            Ticket ticket0 = new Ticket() { connectionsParts = listConnectionPart, seats = seats0 };
            Ticket ticket1 = new Ticket() { connectionsParts = listConnectionPart, seats = seats1 };
            Ticket ticket2 = new Ticket() { connectionsParts = listConnectionPart, seats = seats2 };

            // Act
            unitOfWork.DeleteTicketDo(user0, ticket0);
            unitOfWork.DeleteTicketDo(user1, ticket1);
            unitOfWork.DeleteTicketDo(user2, ticket2);

            // Assert
            Assert.AreEqual("", user0.Tickets);
            Assert.AreEqual("1-34", user1.Tickets);
            Assert.AreEqual("45-70", user2.Tickets);
            Assert.AreEqual("1;2;3;23;13;43", listConnectionPart[1].Seats);
            Assert.AreEqual("100;20;11", listConnectionPart[2].Seats);
            Assert.AreEqual(3, listConnectionPart[2].FreeSeats);
        }
    }
}
