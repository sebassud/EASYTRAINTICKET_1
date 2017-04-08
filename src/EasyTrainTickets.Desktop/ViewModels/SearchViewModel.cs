using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using Caliburn.Micro;
using System.Windows;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Desktop.Work;

namespace EasyTrainTickets.Desktop.ViewModels
{
    public class SearchViewModel : Screen, IHandle<User>
    {
        private IUnitOfWork unitOfWork;
        private IEventAggregator eventAggregator;
        private User currentUser;

        private List<string> fromStation;
        public List<string> FromStation
        {
            get
            {
                return fromStation;
            }
            set
            {
                fromStation = value;
                NotifyOfPropertyChange("FromStation");
            }
        }

        private string selectedFromStation;
        public string SelectedFromStation
        {
            get
            {
                return selectedFromStation;
            }
            set
            {
                selectedFromStation = value;
                NotifyOfPropertyChange("SelectedFromStation");
                NotifyOfPropertyChange("CanSearch");
            }
        }

        private List<string> endStation;
        public List<string> EndStation
        {
            get
            {
                return endStation;
            }
            set
            {
                endStation = value;
                NotifyOfPropertyChange("EndStation");
            }
        }

        private string selectedEndStation;
        public string SelectedEndStation
        {
            get
            {
                return selectedEndStation;
            }
            set
            {
                selectedEndStation = value;
                NotifyOfPropertyChange("SelectedEndStation");
                NotifyOfPropertyChange("CanSearch");
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
            }
        }
        public DateTime MinDate
        {
            get
            {
                return DateTime.Now;
            }
        }

        private BindableCollection<ConnectionPath> paths = new BindableCollection<ConnectionPath>();
        public BindableCollection<ConnectionPath> Paths
        {
            get { return paths; }
            set
            {
                paths = value;
                NotifyOfPropertyChange("Paths");
            }
        }

        private ConnectionPath selectedPath;
        public ConnectionPath SelectedPath
        {
            get
            {
                return selectedPath;
            }

            set
            {
                selectedPath = value;
                NotifyOfPropertyChange("SelectedPath");
                NotifyOfPropertyChange("CanBuyTicket");
            }
        }
        private bool canSearch = true;
        public bool CanSearch
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedFromStation) && !string.IsNullOrEmpty(SelectedEndStation) && SelectedFromStation != SelectedEndStation && canSearch;
            }
            set
            {
                canSearch = value;
                NotifyOfPropertyChange("CanSearch");
            }
        }

        public SearchViewModel(IUnitOfWork _unitOfWork, IEventAggregator _eventAggregator, User _currentUser)
        {
            unitOfWork = _unitOfWork;
            eventAggregator = _eventAggregator;
            FromStation = unitOfWork.Stations;
            EndStation = unitOfWork.Stations;
            currentUser = _currentUser;
            eventAggregator.Subscribe(this);
        }

        private void SearchTask()
        {
            Paths.Clear();
            CanSearch = false;
            Paths.AddRange(unitOfWork.Search(SelectedFromStation, SelectedEndStation, StartDate));
            CanSearch = true;
        }

        public async void Search()
        {
            await Task.Run(() => SearchTask());
        }

        public bool CanBuyTicket
        {
            get
            {
                return SelectedPath != null;
            }
        }

        public void BuyTicket()
        {
            eventAggregator.PublishOnUIThreadAsync(new BuyTicketViewModel(unitOfWork, selectedPath, this, eventAggregator, currentUser));
        }

        public void Handle(User message)
        {
            currentUser = message;
        }
    }
}
