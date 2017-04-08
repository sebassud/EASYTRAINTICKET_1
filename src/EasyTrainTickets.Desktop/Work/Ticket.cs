using EasyTrainTickets.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Desktop.Work
{
    public class Ticket
    {
        public List<ConnectionPart> connectionsParts { get; set; }
        public List<int> seats { get; set; }
        public List<int> conPartsId { get; set; }
        public int Count
        {
            get
            {
                return Math.Max(conPartsId.Count, connectionsParts.Count);
            }
        }
        private int change;
        public int Change
        {
            get
            {
                return change;
            }
        }
        private decimal price;
        public decimal Price
        {
            get
            {
                return price;
            }
        }
        private DateTime startTime;
        public DateTime StartTime
        {
            get
            {
                return startTime;
            }
        }
        private DateTime endTime;
        public DateTime EndTime
        {
            get
            {
                return endTime;
            }
        }

        private List<string> sourcePictures;
        public List<string> SourcePictures
        {
            get
            {
                return sourcePictures;
            }
        }
        private string startStation;
        public string StartStation
        {
            get
            {
                return startStation;
            }
        }
        private string endStation;
        public string EndStation
        {
            get
            {
                return endStation;
            }
        }

        public Ticket()
        {
            connectionsParts = new List<ConnectionPart>();
            seats = new List<int>();
            conPartsId = new List<int>();
            sourcePictures = new List<string>();
        }
        public Ticket(List<ConnectionPart> conParts) : this()
        {
            connectionsParts = conParts;
            this.Initialize();
        }
        public void AddSeat(int seat)
        {
            seats.Add(seat);
        }

        public int this[int i]
        {
            get
            {
                return conPartsId[i];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < seats.Count; i++)
            {
                sb.AppendFormat("{0,10} => {1,10}      {2,10} {3,-15} {4,10} {5,5}\n", connectionsParts[i].Route.From, connectionsParts[i].Route.To, "Pociąg: ", connectionsParts[i].Connection.Name, "Zarezerwowane miejsce:", seats[i]);
            }
            return sb.ToString();
        }

        public void Initialize()
        {
            startTime = connectionsParts[0].StartTime;
            endTime = connectionsParts.Last().EndTime;
            startStation = connectionsParts[0].Route.From;
            endStation = connectionsParts.Last().Route.To;
            price = 0;
            foreach (var conPart in connectionsParts)
            {
                price += conPart.Connection.Train.PricePerKilometer * conPart.Route.Distance;
            }
            change = connectionsParts.Select(c => c.Connection).Distinct().Count() - 1;
            for (int i = 0; i < Count - 1; i++)
            {
                if (connectionsParts[i].Connection != connectionsParts[i + 1].Connection)
                {
                    if (connectionsParts[i].Connection.Train.Type == "Pośpieszny") sourcePictures.Add("/Resources/pospImg.png");
                    else if (connectionsParts[i].Connection.Train.Type == "Ekspres") sourcePictures.Add("/Resources/exImg.png");
                }
            }
            if (connectionsParts[Count - 1].Connection.Train.Type == "Pośpieszny") sourcePictures.Add("/Resources/pospImg.png");
            else if (connectionsParts[Count - 1].Connection.Train.Type == "Ekspres") sourcePictures.Add("/Resources/exImg.png");
        }
    }
}
