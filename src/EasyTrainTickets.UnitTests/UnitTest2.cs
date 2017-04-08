using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Desktop.ViewModels;
using Moq;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using EasyTrainTickets.Desktop.Work;
using System.Threading;

namespace EasyTrainTickets.UnitTests
{
    [TestClass]
    public class UnitTest2
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
        public void Add_User_Nonexist()
        {
            // Arrange
            var users = new List<User>();
            users.Add(new User { Login = "aaa", Password = "costam", Tickets = "", IsAdmin = false });
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(users));
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Trains).Returns(db.Trains);
            UnitOfWork unit = new UnitOfWork(mock.Object);

            // Act
           User user = unit.Registration("ccc", "bbb");

            //Assert
            Assert.AreEqual(2, users.Count());
            Assert.AreEqual(false, user.IsAdmin);
            Assert.AreEqual("ccc", user.Login);
        }

        [TestMethod]
        public void Add_User_Exist()
        {
            // Arrange
            var users = new List<User>();
            users.Add(new User { Login = "aaa", Password = "costam", Tickets = "", IsAdmin = false });
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(users));
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Trains).Returns(db.Trains);
            UnitOfWork unit = new UnitOfWork(mock.Object);

            // Act
            User user = unit.Registration("aaa", "bbb");

            //Assert
            Assert.AreEqual(1, users.Count());
            Assert.AreEqual(null, user);
        }

        [TestMethod]
        public void Add_User_Validate()
        {
            // Arrange
            var users = new List<User>();
            users.Add(new User { Login = "aaa", Password = "costam", Tickets = "", IsAdmin = false });
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(users));
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Trains).Returns(db.Trains);
            RegistrationViewModel rg = new RegistrationViewModel(new UnitOfWork(mock.Object));
            rg.Login = "ccc";
            rg.Password = "bbb";
            rg.RepeatPassword = "bcb";

            // Act
            rg.Registration();

            //Assert
            Assert.AreEqual(1, users.Count());
            Assert.AreEqual(false, rg.IsUser);
            Assert.AreEqual(null, rg.CurrentUser);
        }

        [TestMethod]
        public void Sign_Admin()
        {
            // Arrange
            var users = new List<User>();
            users.Add(new User { Login = "Admin", Password = "6NgzabR+BeSAT8ps06HWgA==", Tickets = "", IsAdmin = true });
            Mock<IEasyTrainTicketsDbEntities> mock = new Mock<IEasyTrainTicketsDbEntities>();
            mock.Setup(m => m.Users).Returns(GetQueryableMockDbSet<User>(users));
            EasyTrainTicketsDbEntities db = new EasyTrainTicketsDbEntities();
            mock.Setup(m => m.Routes).Returns(db.Routes);
            mock.Setup(m => m.Trains).Returns(db.Trains);
            UnitOfWork unit = new UnitOfWork(mock.Object);

            // Act
            User user = unit.SignIn("Admin","admin");

            //Assert
            Assert.AreEqual(1, users.Count());
            Thread.Sleep(1000);
            Assert.AreEqual(true, user.IsAdmin);
            Assert.AreEqual("Admin", user.Login);
        }

    }
}
