using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Domain.Model
{
    public class User : Entity
    {
        public virtual string Login { get; set; }
        public virtual string Password { get; set; }
        public virtual string Tickets { get; set; }
        public virtual bool IsAdmin { get; set; }
    }
}
