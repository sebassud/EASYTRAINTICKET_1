using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Domain.Model
{
    public class Connection : Entity
    {   
        public Connection()
        {
            this.Parts = new List<ConnectionPart>();
        }
        public virtual string StartPlace { get; set; }
        public virtual string EndPlace { get; set; }
        public virtual string Name { get; set; }
        public virtual Train Train { get; set; }
        public virtual ICollection<ConnectionPart> Parts { get; set; }
    }
}
