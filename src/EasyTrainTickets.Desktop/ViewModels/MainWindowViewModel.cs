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
using System.Windows.Threading;

namespace EasyTrainTickets.Desktop.ViewModels
{
    class MainWindowViewModel : Conductor<object>, IShell, IHandle<BuyTicketViewModel>, IHandle<SearchViewModel>, IHandle<InformationToUser>
    {
        private IWindowManager windowManager;
        private IEventAggregator eventAggregator;
        private IUnitOfWork unitOfWork;
        private User currentUser;
        private DispatcherTimer timer = new DispatcherTimer();

        public string Title { get; set; } = "Easy Train Tickets";

        private bool isadmin = false;
        public bool IsAdmin
        {
            get
            {
                return isadmin;
            }
            set
            {
                isadmin = value;
                NotifyOfPropertyChange("IsAdmin");
            }
        }
        private bool isuser = false;
        public bool IsUser
        {
            get
            {
                return isuser;
            }
            set
            {
                isuser = value;
                NotifyOfPropertyChange("IsUser");
            }
        }
        private string welcome;
        public string Welcome
        {
            get
            {
                if (currentUser == null) return "";
                return welcome;
            }
            set
            {
                welcome = value;
                NotifyOfPropertyChange("Welcome");
            }
        }

        private InformationToUser information;
        public InformationToUser Information
        {
            get
            {
                return information;
            }
            set
            {
                information = value;
                NotifyOfPropertyChange("Information");
            }
        }


        public MainWindowViewModel(IWindowManager _windowManager, IEventAggregator _eventAggretator, IUnitOfWork _unitOfWork)
        {
            windowManager = _windowManager;
            eventAggregator = _eventAggretator;
            unitOfWork = _unitOfWork;
            eventAggregator.Subscribe(this);
            timer.Interval = new TimeSpan(0, 0, 15);
            timer.Tick += TimerTick;
            ActivateItem(new WelcomeViewModel());
        }

        public void SignIn()
        {
            LoginViewModel login = new LoginViewModel(unitOfWork);
            windowManager.ShowDialog(login);
            IsAdmin = login.IsAdmin;
            IsUser = login.IsUser;
            currentUser = login.currentUser;
            if (currentUser != null)
            {
                Welcome = "Zalogowany jako:\n" + currentUser.Login;
                eventAggregator.PublishOnUIThread(currentUser);
            }
            if (IsAdmin) ActivateItem(null);
        }

        public void LoginOut()
        {
            IsAdmin = false;
            IsUser = false;
            currentUser = null;
            NotifyOfPropertyChange("Welcome");
            ActivateItem(new WelcomeViewModel());
        }

        public void Registration()
        {
            RegistrationViewModel registration = new RegistrationViewModel(unitOfWork);
            windowManager.ShowDialog(registration);
            IsUser = registration.IsUser;
            currentUser = registration.CurrentUser;
            if (currentUser != null)
            {
                Welcome = "Zalogowany jako:\n" + currentUser.Login;
                eventAggregator.PublishOnUIThread(currentUser);
            }
        }

        public void AddConnection()
        {
            ActivateItem(new AddConnectionsViewModel(unitOfWork, eventAggregator));
        }

        public void Search()
        {
            ActivateItem(new SearchViewModel(unitOfWork, eventAggregator, currentUser));
        }

        public void Tickets()
        {
            ActivateItem(new MyTicketsViewModel(currentUser, unitOfWork, eventAggregator));
        }

        public void Handle(BuyTicketViewModel message)
        {
            if (currentUser == null)
            {
                this.SignIn();
                return;
            }
            ActivateItem(message);
        }

        public void Handle(SearchViewModel message)
        {
            ActivateItem(message);
        }

        public void Handle(InformationToUser message)
        {
            timer.Stop();
            Information = message;
            timer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timer.Stop();
            Information = new InformationToUser();
        }
    }
}
