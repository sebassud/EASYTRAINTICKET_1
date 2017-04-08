using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Domain.Model
{
    public class Train : Entity
    {
        public virtual string Type { get; set; }
        public virtual decimal PricePerKilometer { get; set; }
    }
}
