using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using EasyTrainTickets.Desktop.GraphHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Desktop.Work
{
    public class UnitOfWork : IUnitOfWork
    {
        private IEasyTrainTicketsDbEntities dbContext;
        private Graph graph;

        public List<string> Stations
        {
            get
            {
                return graph.Stations;
            }
        }

        public List<Route> GetRoutes
        {
            get
            {
                return dbContext.Routes.ToList();
            }
        }

        public List<Train> GetTrains
        {
            get
            {
                return dbContext.Trains.ToList();
            }
        }

        public UnitOfWork(IEasyTrainTicketsDbEntities _dbContext)
        {
            dbContext = _dbContext;
            graph = new Graph(dbContext);
        }

        public List<ConnectionPath> Search(string from, string to, DateTime userTime)
        {
            List<Path> paths = graph.SearchPaths(from, to);
            List<ConnectionPath> conPaths = new List<ConnectionPath>();
            Parallel.For(0, paths.Count, i =>
            {
                using (IEasyTrainTicketsDbEntities tmpContext = new EasyTrainTicketsDbEntities())
                {
                    List<ConnectionPath> candidatePaths = paths[i].SecondSearch(userTime, tmpContext);
                    foreach (var conPath in candidatePaths)
                    {
                        conPath.WriteConnection();
                        conPaths.Add(conPath);
                    }
                }
            });
            conPaths = conPaths.OrderBy(c => c.StartTime).ThenBy(c => c.EndTime).Take(10).ToList();
            return conPaths;
        }

        public User SignIn(string login, string password)
        {
            var record = dbContext.Users.Where(u => u.Login == login);
            if (record.Count() == 1)
            {
                string pass = HashMd5(password);
                string sqlpass = record.First().Password;
                if (sqlpass == pass)
                {
                    return record.ToList()[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private string HashMd5(string password)
        {
            var salt = System.Text.Encoding.UTF8.GetBytes("susz");
            var pass = System.Text.Encoding.UTF8.GetBytes(password);
            var hmacMD5 = new HMACMD5(salt);
            var saltedHash = hmacMD5.ComputeHash(pass);

            return Convert.ToBase64String(saltedHash);
        }

        public User Registration(string login, string password)
        {
            var record = dbContext.Users.Where((u) => u.Login == login);
            if (record.Count() == 1)
            {
                return null;
            }
            User user = new User()
            {
                IsAdmin = false,
                Login = login,
                Password = HashMd5(password),
                Tickets = ""
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            return user;
        }

        public List<Ticket> CreateTickets(User user)
        {
            dbContext.Dispose();
            dbContext = new EasyTrainTicketsDbEntities();
            user = dbContext.Users.Where(u => u.Id == user.Id).First();
            return CreateTicketsDo(user);
        }

        public List<Ticket> CreateTicketsDo(User user)
        {        
            string[] tickets = user.Tickets.Split(';');
            if (user.Tickets.Length == 0) return new List<Ticket>();
            Ticket[] Tickets = new Ticket[tickets.Length];
            for (int i = 0; i < tickets.Length; i++)
            {
                Tickets[i] = new Ticket();
                string[] conParts = tickets[i].Split('|');
                foreach (var conPart in conParts)
                {
                    string[] part = conPart.Split('-');
                    Tickets[i].conPartsId.Add(int.Parse(part[0]));
                    Tickets[i].seats.Add(int.Parse(part[1]));
                }
            }
            return Tickets.ToList();
        }

        public List<Ticket> InitializeTickets(List<Ticket> tickets)
        {
            for (int i = 0; i < tickets.Count; i++)
            {
                HashSet<int> ids = new HashSet<int>();
                foreach (var id in tickets[i].conPartsId)
                {
                    ids.Add(id);
                }
                List<ConnectionPart> parts = dbContext.ConnectionParts.Where(cp => ids.Contains(cp.Id)).ToList();
                for (int j = 0; j < tickets[i].Count; j++)
                {
                    tickets[i].connectionsParts.Add(parts.Find(p => p.Id == tickets[i][j]));
                }
                tickets[i].Initialize();
            }
            return tickets;
        }

        public List<int> GetSeats(ConnectionPath conpath, int from, int to)
        {
            List<int> freeseats = new List<int>();

            List<ConnectionPart> conparts = new List<ConnectionPart>();
            conparts.AddRange(conpath.connectionsParts.GetRange(from, to - from + 1));
            List<int[]> allseats = new List<int[]>();
            foreach (var part in conparts)
            {
                string seats = dbContext.ConnectionParts.Where(cp => cp.Id == part.Id).Select(cp => cp.Seats).First();
                if (seats == "") return freeseats;
                allseats.Add(seats.Split(';').Select(Int32.Parse).ToArray());
            }

            int[] table = new int[120];
            foreach (var list in allseats)
                foreach (var s in list)
                    table[s]++;

            int count = to - from + 1;
            for (int i = 0; i < table.Count(); i++)
            {
                if (table[i] == count) freeseats.Add(i);
            }
            return freeseats;
        }

        public bool BuyTicket(User currentUser, Ticket ticket)
        {
            dbContext.Dispose();
            dbContext = new EasyTrainTicketsDbEntities();
            currentUser = dbContext.Users.Where(u => u.Id == currentUser.Id).First();
            return BuyTicketDo(currentUser, ticket);
        }

        public bool BuyTicketDo(User currentUser, Ticket ticket)
        {
            for (int i = 0; i < ticket.Count; i++)
            {
                var tmp = ticket.connectionsParts[i];
                ticket.connectionsParts[i] = dbContext.ConnectionParts.Where(cp => cp.Id == tmp.Id).First();
                string seat = ticket.seats[i].ToString();
                string seats = ticket.connectionsParts[i].Seats;
                int index = seats.IndexOf(seat);
                string newSeats;
                if (index < 0) return false;
                if (seats.Length == seat.Length)
                {
                    newSeats = "";
                }
                else if (index == seats.Length - seat.Length) newSeats = seats.Remove(index - 1, seat.Length + 1);
                else
                {
                    newSeats = seats.Remove(index, seat.Length + 1);
                }

                ticket.connectionsParts[i].Seats = newSeats;
                ticket.connectionsParts[i].FreeSeats--;
            }
            if (currentUser.Tickets.Length == 0)
                currentUser.Tickets = CreateStringTicket(ticket);
            else
                currentUser.Tickets = string.Format(currentUser.Tickets + ";" + CreateStringTicket(ticket));
            dbContext.SaveChanges();
            return true;
        }

        private string CreateStringTicket(Ticket ticket)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ticket.Count; i++)
            {
                sb.Append(string.Format("{0}-{1}", ticket.connectionsParts[i].Id, ticket.seats[i]));
                if (i < ticket.Count - 1)
                    sb.Append("|");
            }
            return sb.ToString();
        }

        public bool DeleteTicket(User user, Ticket ticket)
        {
            dbContext.Dispose();
            dbContext = new EasyTrainTicketsDbEntities();
            user = dbContext.Users.Where(u => u.Id == user.Id).First();
            return DeleteTicketDo(user, ticket);
        }

        public bool DeleteTicketDo(User user, Ticket ticket)
        {
            
            string ticketString = CreateStringTicket(ticket);
            string userTickets = user.Tickets;
            int index = userTickets.IndexOf(ticketString);
            string newTickets;

            if (index < 0) return false;
            else if (ticketString.Length == userTickets.Length) newTickets = "";
            else if (index == userTickets.Length - ticketString.Length) newTickets = userTickets.Remove(index - 1, ticketString.Length + 1);
            else
            {
                newTickets = userTickets.Remove(index, ticketString.Length + 1);
            }
            user.Tickets = newTickets;


            for (int i = 0; i < ticket.Count; i++)
            {
                var tmp = ticket.connectionsParts[i];
                ticket.connectionsParts[i] = dbContext.ConnectionParts.Where(cp => cp.Id == tmp.Id).First();
                if (ticket.connectionsParts[i].Seats == "") ticket.connectionsParts[i].Seats = ticket.seats[i].ToString();
                else
                {
                    ticket.connectionsParts[i].Seats = String.Format("{0};{1}", ticket.connectionsParts[i].Seats, ticket.seats[i].ToString());
                }
                ticket.connectionsParts[i].FreeSeats++;
            }

            dbContext.SaveChanges();
            return true;
        }

        public bool AddConnection(Connection connection)
        {
            dbContext.Connections.Add(connection);
            return dbContext.SaveChanges() > 0 ? true : false;
        }


    }
}
