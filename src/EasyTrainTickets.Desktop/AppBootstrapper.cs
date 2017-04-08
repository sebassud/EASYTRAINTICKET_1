namespace EasyTrainTickets.Desktop
{
    using System;
    using System.Collections.Generic;
    using Caliburn.Micro;
    using EasyTrainTickets.Desktop.ViewModels;
    using EasyTrainTickets.Domain.Data;
    using System.Windows.Controls;
    using Work;

    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
            ConventionManager.AddElementConvention<PasswordBox>(
            PasswordBoxHelper.BoundPasswordProperty,
            "Password",
            "PasswordChanged");
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, MainWindowViewModel>();
            container.Singleton<IEasyTrainTicketsDbEntities, EasyTrainTicketsDbEntities>();
            container.Singleton<IUnitOfWork, UnitOfWork>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}