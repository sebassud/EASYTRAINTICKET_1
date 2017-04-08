using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Domain.Model
{
    public class Route : Entity
    {
        public virtual string From { get; set; }
        public virtual string To { get; set; }
        public virtual int Distance { get; set; }
        public virtual int BestTime { get; set; }
    }
}
