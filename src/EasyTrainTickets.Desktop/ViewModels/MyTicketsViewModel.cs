using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using Caliburn.Micro;
using EasyTrainTickets.Desktop.GraphHelpers;
using EasyTrainTickets.Desktop.Work;
using System.Windows.Media;
using System.Data.Entity.Infrastructure;
using System.Windows;

namespace EasyTrainTickets.Desktop.ViewModels
{
    public class MyTicketsViewModel : Screen
    {
        private User currentuser;
        private IUnitOfWork unitOfWork;

        private BindableCollection<Ticket> tickets;
        public BindableCollection<Ticket> Tickets
        {
            get
            {
                return tickets;
            }
            set
            {
                tickets = value;
                NotifyOfPropertyChange("Tickets");
            }
        }

        private Ticket selectedTicket;
        private IEventAggregator eventAggregator;

        public Ticket SelectedTicket
        {
            get
            {
                return selectedTicket;
            }
            set
            {
                selectedTicket = value;
                NotifyOfPropertyChange("SelectedTicket");
                NotifyOfPropertyChange("CanDelete");
            }
        }

        public bool CanDelete
        {
            get
            {
                if (SelectedTicket == null) return false;
                if (TimeSpan.Compare(SelectedTicket.StartTime - DateTime.Now, new TimeSpan(0, 0, 0)) > 0) return true;
                return false;
            }
        }

        public MyTicketsViewModel(User _currentuser, IUnitOfWork _unitOfWork, IEventAggregator _eventAggregator)
        {
            currentuser = _currentuser;
            unitOfWork = _unitOfWork;
            eventAggregator = _eventAggregator;
            Initialize();
        }

        private async void Initialize()
        {
            List<Ticket> mytickets = unitOfWork.CreateTickets(currentuser);
            tickets = new BindableCollection<Ticket>();           
            Tickets.AddRange(await Task.Run(() => unitOfWork.InitializeTickets(mytickets)));
            var list = Tickets.OrderByDescending(t => t.StartTime).ThenByDescending(t => t.EndTime).ToList();
            Tickets.Clear();
            Tickets.AddRange(list);
        }

        private void DeleteTicketTask()
        {
            bool result = false;
            try
            {
                result = unitOfWork.DeleteTicket(currentuser, selectedTicket);
            }
            catch (DbUpdateConcurrencyException)
            {
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = "Bilet został już usunięty.", Color = Brushes.Red });
            }
            catch (Exception ex)
            {
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = ex.Message, Color = Brushes.Red });
            }
            if (result)
                eventAggregator.PublishOnUIThread(new InformationToUser
                {
                    Message = string.Format("Usunięto bilet:\n{0} => {1}", selectedTicket.StartStation, selectedTicket.EndStation),
                    Color = Brushes.Lime
                });
            else
                eventAggregator.PublishOnUIThread(new InformationToUser { Message = "Bilet został już usunięty.", Color = Brushes.Red });
            Tickets.Remove(selectedTicket);
        }
        public async void Delete()
        {
            await Task.Run(() => DeleteTicketTask());
        }
    }
}
