using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using System.Windows;
using System.ComponentModel;
using System.Security.Cryptography;
using EasyTrainTickets.Desktop.Work;

namespace EasyTrainTickets.Desktop.ViewModels
{
    public class RegistrationViewModel : Conductor<object>, IDataErrorInfo
    {
        private IUnitOfWork unitOfWork;
        private string login;
        public string Login
        {
            get
            {
                return login;
            }
            set
            {
                login = value;
                NotifyOfPropertyChange("User");
                NotifyOfPropertyChange(() => CanRegistration);
            }
        }

        private string password;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                NotifyOfPropertyChange("Password");
                NotifyOfPropertyChange(() => CanRegistration);
            }
        }

        private string repeatPassword;
        public string RepeatPassword
        {
            get
            {
                return repeatPassword;
            }
            set
            {
                repeatPassword = value;
                NotifyOfPropertyChange("RepeatPassword");
                NotifyOfPropertyChange(() => CanRegistration);
            }
        }

        private bool isuser = false;
        public bool IsUser
        {
            get
            {
                return isuser;
            }
        }

        private User currentuser;
        public User CurrentUser
        {
            get
            {
                return currentuser;
            }
        }

        public RegistrationViewModel(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }

        private string error;
        public string Error
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
                NotifyOfPropertyChange("Error");
            }
        }

        public string this[string fieldName]
        {
            get
            {
                string error = null;
                if (fieldName == "Login")
                {
                    if (string.IsNullOrEmpty(login))
                        error = "Imię nie może być puste!";
                }
                else if (fieldName == "Password")
                {
                    if (string.IsNullOrEmpty(password))
                        error = "Hasło nie może być puste!";
                }
                else if (fieldName == "RepeatPassword")
                {
                    if (RepeatPassword != Password)
                        error = "Hasło muszą być jednakowe";
                }
                return error;
            }
        }

        public void Cancel()
        {
            this.TryClose();
        }

        public bool CanRegistration
        {
            get
            {
                return Validate();
            }
        }

        private bool Validate()
        {
            if (login != null && password != null && repeatPassword != null && login.Length != 0 && password.Length != 0 && repeatPassword == password) return true;
            else
                return false;
        }

        private void RegistrationTask()
        {
            if (CanRegistration == false) return;

            User user =unitOfWork.Registration(login, password);

            if (user == null)
            {
                Error = "Istnieje już user o podanym loginie proszę wybrać inny";
                return;
            }

            currentuser = user;
            isuser = true;

            this.TryClose();
        }

        public async void Registration()
        {
            await Task.Run(() => RegistrationTask());
        }

    }
}
