using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EasyTrainTickets.Domain.Model
{
    public abstract class Entity
    {
        [Key]
        public virtual int Id { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
