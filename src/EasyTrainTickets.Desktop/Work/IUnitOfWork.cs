using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Domain.Model;

namespace EasyTrainTickets.Desktop.Work
{
    public interface IUnitOfWork
    {
        List<Route> GetRoutes { get; }
        List<Train> GetTrains { get; }
        List<ConnectionPath> Search(string from, string to, DateTime userTime);
        List<string> Stations { get; }
        User SignIn(string login, string password);
        User Registration(string login, string password);
        List<Ticket> CreateTickets(User user);
        List<Ticket> InitializeTickets(List<Ticket> tickets);
        List<int> GetSeats(ConnectionPath conpath, int from, int to);
        bool BuyTicket(User currentUser, Ticket ticket);
        bool DeleteTicket(User user, Ticket ticket);
        bool AddConnection(Connection connection);
    }
}
