using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Desktop.GraphHelpers
{
    public class ConnectionPath
    {
        public List<ConnectionPart> connectionsParts = new List<ConnectionPart>();

        public int Count
        {
            get
            {
                return connectionsParts.Count;
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
        public string Way { get; set; }
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
        private TimeSpan journeyTime;
        public TimeSpan JourneyTime
        {
            get
            {
                return journeyTime;
            }
        }

        public DateTime? JourneyEnd
        {
            get
            {
                if (startTime.Date == endTime.Date)
                    return null;
                else
                    return EndTime.Date;
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

        public void Add(ConnectionPart conPart)
        {
            connectionsParts.Add(conPart);
        }

        public void WriteConnection()
        {
            StringBuilder sb = new StringBuilder();
            int k = 0;
            for (int i = 0; i < Count - 1; i++)
            {
                if (connectionsParts[i].Connection != connectionsParts[i + 1].Connection)
                {
                    sb.AppendFormat("{0,10} => {1,10} {2,10} {3,7} {4,10} {5,7} {6,10} {7,10}\n", connectionsParts[k].Route.From, connectionsParts[i].Route.To, "Odjazd:", connectionsParts[k].StartTime.ToShortTimeString(), "Przyjazd:", connectionsParts[i].EndTime.ToShortTimeString(), "Pociąg:", connectionsParts[i].Connection.Name);
                    k = i + 1;
                }
            }
            sb.AppendFormat("{0,10} => {1,10} {2,10} {3,7} {4,10} {5,7} {6,10} {7,10}\n", connectionsParts[k].Route.From, connectionsParts[connectionsParts.Count - 1].Route.To, "Odjazd:", connectionsParts[k].StartTime.ToShortTimeString(), "Przyjazd:", connectionsParts[connectionsParts.Count - 1].EndTime.ToShortTimeString(), "Pociąg:", connectionsParts[connectionsParts.Count - 1].Connection.Name);

            Way = sb.ToString();
        }

        public void Initialize()
        {
            sourcePictures = new List<string>();
            startTime = connectionsParts[0].StartTime;
            endTime = connectionsParts.Last().EndTime;
            startStation = connectionsParts[0].Route.From;
            endStation = connectionsParts.Last().Route.To;
            journeyTime = endTime - startTime;
            price = 0;
            foreach (var conPart in connectionsParts)
            {
                price += conPart.Connection.Train.PricePerKilometer * conPart.Route.Distance;
            }
            change = 0;
            for (int i = 0; i < Count - 1; i++)
            {
                if (connectionsParts[i].Connection != connectionsParts[i + 1].Connection)
                {
                    change++;
                    if (connectionsParts[i].Connection.Train.Type == "Pośpieszny") sourcePictures.Add("/Resources/pospImg.png");
                    else if (connectionsParts[i].Connection.Train.Type == "Ekspres") sourcePictures.Add("/Resources/exImg.png");
                }
            }
            if (connectionsParts[Count - 1].Connection.Train.Type == "Pośpieszny") sourcePictures.Add("/Resources/pospImg.png");
            else if (connectionsParts[Count - 1].Connection.Train.Type == "Ekspres") sourcePictures.Add("/Resources/exImg.png");
        }
    }
}
