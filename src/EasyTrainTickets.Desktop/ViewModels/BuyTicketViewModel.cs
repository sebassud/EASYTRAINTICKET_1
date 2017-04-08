using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using EasyTrainTickets.Desktop.Work;
using EasyTrainTickets.Desktop.GraphHelpers;
using Caliburn.Micro;
using System.Windows;
using System.Data.Entity.Infrastructure;
using System.Windows.Media;

namespace EasyTrainTickets.Desktop.ViewModels
{
    public class BuyTicketViewModel : Screen
    {
        private IUnitOfWork unitOfWork;
        private ConnectionPath connectionpath;
        private SearchViewModel searchViewModel;
        private IEventAggregator eventAggregator;
        private User currentUser;
        private int to;
        private string sourcePicture;
        public string SourcePicture
        {
            get
            {
                return sourcePicture;
            }
            set
            {
                sourcePicture = value;
                NotifyOfPropertyChange("SourcePicture");
            }
        }
        private List<int> seats = new List<int>();
        public List<int> Seats
        {
            get
            {
                return seats;
            }
            set
            {
                seats = value;
                NotifyOfPropertyChange("Seats");
            }

        }

        private int? selectedSeat;
        public int? SelectedSeat
        {
            get
            {
                return selectedSeat;
            }
            set
            {
                selectedSeat = value;
                NotifyOfPropertyChange("SelectedSeat");
                NotifyOfPropertyChange("CanNext");
            }
        }
        private string currentReservation;
        public string CurrentReservation
        {
            get
            {
                return currentReservation;
            }
            set
            {
                currentReservation = value;
                NotifyOfPropertyChange("CurrentReservation");
            }
        }
        public int Count
        {
            get
            {
                return Ticket.Count;
            }
        }
        public bool CanNext
        {
            get
            {
                return SelectedSeat != null && Part < Count;
            }
        }

        public bool CanConfirm
        {
            get
            {
                return Part == Count;
            }
        }
        private int part;
        public int Part
        {
            get
            {
                return part;
            }
            set
            {
                part = value;
                NotifyOfPropertyChange("Part");
                NotifyOfPropertyChange("CanNext");
                NotifyOfPropertyChange("CanConfirm");
            }
        }
        private string way;
        public string Way
        {
            get
            {
                return way;
            }
            set
            {
                way = value;
                NotifyOfPropertyChange("Way");
            }
        }
        private Ticket ticket;
        public Ticket Ticket
        {
            get
            {
                return ticket;
            }
            set
            {
                ticket = value;
                NotifyOfPropertyChange("Ticket");
            }
        }

        public BuyTicketViewModel(IUnitOfWork _unitOfWork, ConnectionPath _connectionpath, SearchViewModel _searchViewModel, IEventAggregator _eventAggregator, User _currentUser)
        {
            unitOfWork = _unitOfWork;
            connectionpath = _connectionpath;
            searchViewModel = _searchViewModel;
            eventAggregator = _eventAggregator;
            Way = connectionpath.Way;
            currentUser = _currentUser;
            Ticket = new Ticket(connectionpath.connectionsParts);
            CalculateReservation();
        }

        public void Cancel()
        {
            eventAggregator.PublishOnUIThreadAsync(searchViewModel);
        }

        private void CalculateReservationTask()
        {
            string fromStation = connectionpath.connectionsParts[Part].Route.From;
            to = part;

            while (to + 1 < Count && connectionpath.connectionsParts[to].Connection.Id == connectionpath.connectionsParts[to + 1].Connection.Id) to++;

            string endStation = connectionpath.connectionsParts[to].Route.To;
            if (connectionpath.connectionsParts[to].Connection.Train.Type == "Pośpieszny") SourcePicture = "/Resources/pospMsc.png";
            else if (connectionpath.connectionsParts[to].Connection.Train.Type == "Ekspres") SourcePicture = "/Resources/exMsc.png";

            Seats = unitOfWork.GetSeats(connectionpath, part, to);
            if (Seats.Count == 0)
            {
                endStation = connectionpath.connectionsParts[part].Route.To;
                to = part;
                Seats = unitOfWork.GetSeats(connectionpath, part, to);
                if (Seats.Count == 0)
                {
                    eventAggregator.PublishOnUIThread(new InformationToUser
                    {
                        Message = "Brak miejsc.",
                        Color = Brushes.Red
                    });
                    Cancel();
                }
            }
            seats.Sort((a, b) => a - b);
            CurrentReservation = String.Format("{0,10} => {1,10}", fromStation, endStation);
        }

        private async void CalculateReservation()
        {
            await Task.Run(() => CalculateReservationTask());
        }

        public void Next()
        {
            for (int i = Part; i <= to; i++)
            {
                ticket.AddSeat((int)selectedSeat);
                NotifyOfPropertyChange("Ticket");
            }
            Part = to + 1;
            SelectedSeat = null;
            if (Part < Count)
                CalculateReservation();
            else
            {
                CurrentReservation = "";
                Seats = null;
                SourcePicture = null;
            }
        }

        private void ConfirmTask()
        {
            bool result = false;
            try
            {
                result = unitOfWork.BuyTicket(currentUser, Ticket);
            }
            catch (DbUpdateConcurrencyException)
            {
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = "Nie udało się zakupić biletu.", Color = Brushes.Red });
            }
            catch (Exception ex)
            {
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = ex.Message, Color = Brushes.Red });
            }
            if (result)
                eventAggregator.PublishOnUIThread(new InformationToUser
                {
                    Message = string.Format("Zakupiono bilet na relację:\n {0} => {1}",
                    Ticket.StartStation, Ticket.EndStation),
                    Color = Brushes.Lime
                });
            else
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = "Nie udało się zakupić biletu.", Color = Brushes.Red });
            Cancel();
        }
        public async void Confirm()
        {
            await Task.Run(() => ConfirmTask());
        }

    }
}
