using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTrainTickets.Domain.Model;
using System.Data.Entity;

namespace EasyTrainTickets.Domain.Data
{
    public interface IEasyTrainTicketsDbEntities : IDisposable
    {
        IDbSet<Connection> Connections { get; set; }
        IDbSet<ConnectionPart> ConnectionParts { get; set; }
        IDbSet<Route> Routes { get; set; }
        IDbSet<Train> Trains { get; set; }
        IDbSet<User> Users { get; set; }

        DbSet Set(Type entityType);
        int SaveChanges();

        Task<int> SaveChangesAsync();

    }
}
