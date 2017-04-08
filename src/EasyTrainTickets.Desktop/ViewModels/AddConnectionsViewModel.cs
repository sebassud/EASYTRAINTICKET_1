using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using System.Windows;
using EasyTrainTickets.Desktop.Work;
using System.Windows.Media;

namespace EasyTrainTickets.Desktop.ViewModels
{
    public class AddConnectionsViewModel : Screen
    {
        private List<string> stations = new List<string>();
        private List<int> times = new List<int>();
        private List<string> cities;
        private List<Route> routes;
        string seats = @"11;12;13;14;15;16;17;18;21;22;23;24;25;26;27;28;31;32;33;34;35;36;37;38;41;42;43;44;45;46;47;48;51;52;53;54;55;56;57;58;61;62;62;64;65;66;67;68;71;72;73;74;75;76;77;78;81;82;83;84;85;86;87;88;91;92;93;94;95;96;97;98;101;102;103;104;105;106;107;108";
        string expressSeats = @"11;12;13;14;15;16;21;22;23;24;25;26;31;32;33;34;35;36;41;42;43;44;45;46;51;52;53;54;55;56;61;62;62;64;65;66;71;72;73;74;75;76;81;82;83;84;85;86;91;92;93;94;95;96;101;102;103;104;105;106";
        public List<string> Cities
        {
            get
            {
                return cities;
            }
            set
            {
                cities = value;
                NotifyOfPropertyChange("Cities");
            }
        }
        private string lastStation = "";
        public string LastStation
        {
            get
            {
                return lastStation;
            }
            set
            {
                lastStation = value;
                NotifyOfPropertyChange("LastStation");
            }
        }
        private string selectedStation = "";
        public string SelectedStation
        {
            get
            {
                return selectedStation;
            }
            set
            {
                selectedStation = value;
                NotifyOfPropertyChange("SelectedStation");
                //aktualizacja SegmentTime
                if (!string.IsNullOrEmpty(lastStation) && !string.IsNullOrEmpty(selectedStation))
                    SegmentTime = routes.Where(r => r.From == lastStation && r.To == selectedStation).Select(r => r.BestTime).First();
                else
                    SegmentTime = 0;
            }
        }
        private int stop;
        public int Stop
        {
            get
            {
                return stop;
            }
            set
            {
                stop = value;
                NotifyOfPropertyChange("Stop");
            }
        }

        private int segmentTime;
        public int SegmentTime
        {
            get
            {
                return segmentTime;
            }
            set
            {
                segmentTime = value;
                NotifyOfPropertyChange("SegmentTime");
            }
        }
        private DateTime startDate = DateTime.Now;
        public DateTime StartDate
        {
            get
            {
                return startDate;
            }
            set
            {
                startDate = value;
                NotifyOfPropertyChange("StartDate");
                RewriteItinerary();
            }
        }

        private BindableCollection<string> itinerary = new BindableCollection<string>();
        public BindableCollection<string> Itinerary
        {
            get { return itinerary; }
            set
            {
                itinerary = value;
                NotifyOfPropertyChange("Itinerary");
                NotifyOfPropertyChange("CanAddConnection");
                NotifyOfPropertyChange("CanDelete");
            }
        }

        private List<string> trainTypes;
        public List<string> TrainTypes
        {
            get
            {
                return trainTypes;
            }
            set
            {
                trainTypes = value;
                NotifyOfPropertyChange("TrainTypes");
            }
        }
        private string selectedTrainType;
        public string SelectedTrainType
        {
            get
            {
                return selectedTrainType;
            }
            set
            {
                selectedTrainType = value;
                NotifyOfPropertyChange("SelectedTrainType");
                NotifyOfPropertyChange("CanAddConnection");
            }
        }

        private string connectionName;
        private List<Train> trains;
        private IUnitOfWork unitOfWork;
        private IEventAggregator eventAggregator;

        public string ConnectionName
        {
            get
            {
                return connectionName;
            }
            set
            {
                connectionName = value;
                NotifyOfPropertyChange("ConnectionName");
                NotifyOfPropertyChange("CanAddConnection");
            }
        }

        public bool CanAddConnection
        {
            get
            {
                return SelectedTrainType != null && !string.IsNullOrEmpty(ConnectionName) && Itinerary.Count > 2;
            }
        }
        public bool CanDelete
        {
            get
            {
                return Itinerary.Count > 1;
            }
        }

        public AddConnectionsViewModel(IUnitOfWork _unitOfWork, IEventAggregator _eventAggregator)
        {
            unitOfWork = _unitOfWork;
            eventAggregator = _eventAggregator;
            Initialize();
        }
        private async void Initialize()
        {
            Itinerary.Add(string.Format("{0,-20} {1,20} {2,20}", "Stacja", "Przyjazd", "Odjazd"));
            routes = await Task.Run(() => unitOfWork.GetRoutes);
            trains = await Task.Run(() => unitOfWork.GetTrains);
            Cities = routes.Select(r => r.From).Distinct().ToList();
            TrainTypes = trains.Select(t => t.Type).Distinct().ToList();
        }

        private void AddConnectionTask()
        {
            string seat = seats;
            int numberOfSeats = 80;
            if (SelectedTrainType == "Ekspres")
            {
                seat = expressSeats;
                numberOfSeats = 60;
            }
            Connection connection = new Connection()
            {
                StartPlace = stations[0],
                EndPlace = stations[stations.Count - 1],
                Train = trains.Where(t => t.Type == SelectedTrainType).First(),
                Name = ConnectionName,
            };
            ConnectionPart[] parts = new ConnectionPart[stations.Count - 1];
            int k = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = new ConnectionPart();
                string from = stations[i];
                string to = stations[i + 1];
                parts[i].Route = routes.Where(r => r.From == from && r.To == to).First();
                if (i == 0)
                {
                    parts[i].StartTime = startDate;
                    parts[i].EndTime = startDate.AddMinutes(times[k++]);
                }
                else
                {
                    parts[i].StartTime = startDate.AddMinutes(times[k++]);
                    parts[i].EndTime = startDate.AddMinutes(times[k++]);
                }
                parts[i].Seats = seat;
                parts[i].FreeSeats = numberOfSeats;
            }
            foreach (var part in parts)
            {
                connection.Parts.Add(part);
            }
            bool result = unitOfWork.AddConnection(connection);
            if (result)
                eventAggregator.PublishOnUIThread(new InformationToUser
                {
                    Message = "Zapisano połączenie.",
                    Color = Brushes.Lime
                });
            else
                eventAggregator.PublishOnUIThread(new InformationToUser
                {
                    Message = "Nie udało się zapisać połączenia.",
                    Color = Brushes.Red
                });
        }

        public async void AddConnection()
        { 
            await Task.Run(() => AddConnectionTask());
        }

        public void AddStation()
        {
            if (string.IsNullOrEmpty(selectedStation)) return;
            if (Itinerary.Count == 1) { }
            else if (Itinerary.Count == 2)
            {
                times.Add(segmentTime);
            }
            else
            {
                times.Add(times.Last() + Stop);
                times.Add(times.Last() + segmentTime);
            }
            stations.Add(selectedStation);
            LastStation = selectedStation;
            SelectedStation = "";
            Cities = routes.Where(r => r.From == LastStation).Select(r => r.To).ToList();
            RewriteItinerary();
        }

        private void RewriteItinerary()
        {
            Itinerary.Clear();
            Itinerary.Add(String.Format("{0,-20} {1,20} {2,20}", "Stacja", "Przyjazd", "Odjazd"));
            int k = 0;
            for (int i = 0; i < stations.Count; i++)
            {
                if (i == 0)
                    Itinerary.Add(string.Format("{0,-20} {1,20} {2,20}", stations[i], "", startDate.ToShortTimeString()));
                else if (i == stations.Count - 1)
                    Itinerary.Add(string.Format("{0,-20} {1,20} {2,20}", stations[i], startDate.AddMinutes(times[k++]).ToShortTimeString(), ""));
                else
                    Itinerary.Add(string.Format("{0,-20} {1,20} {2,20}", stations[i], startDate.AddMinutes(times[k++]).ToShortTimeString(),                                                                                      startDate.AddMinutes(times[k++]).ToShortTimeString()));
            }
            NotifyOfPropertyChange("CanDelete");
            NotifyOfPropertyChange("CanAddConnection");
        }

        public void Clear()
        {
            stations.Clear();
            times.Clear();
            Itinerary = new BindableCollection<string>();
            Itinerary.Add(String.Format("{0,-20} {1,20} {2,20}", "Stacja", "Przyjazd", "Odjazd"));
            Cities = routes.Select(r => r.From).Distinct().ToList();
            LastStation = "";
            SelectedStation = "";
        }

        public void Delete()
        {
            if (stations.Count == 1)
            {
                Clear();
                return;
            }
            else if (stations.Count == 2)
                times.Clear();
            else
                times.RemoveRange(times.Count - 2, 2);
            stations.Remove(stations.Last());
            LastStation = stations.Last();
            SelectedStation = "";
            Cities = routes.Where(r => r.From == LastStation).Select(r => r.To).ToList();
            RewriteItinerary();
        }
    }
}
