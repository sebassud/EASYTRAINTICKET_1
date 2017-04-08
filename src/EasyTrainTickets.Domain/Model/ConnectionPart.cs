using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Domain.Model
{
    public class ConnectionPart : Entity
    {
        public virtual Route Route { get; set; }
        public virtual Connection Connection { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual string Seats { get; set; }
        public virtual int FreeSeats { get; set; }
    }
}
