using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using EncodingConverter.UI.ViewModels;
using EncodingConverter.UI.Views;
using Microsoft.Extensions.Configuration;

namespace EncodingConverter.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        #region DI

        private SimpleContainer _container = new SimpleContainer();

        protected override object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        #endregion DI

        IConfigurationRoot Configuration { get; set; }
        IEncodingConverterConfig _config;

        public Bootstrapper() => Initialize();

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            BuildConfiguration();

            BuildDIContainer();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            try
            {
                DisplayRootViewFor<EncodingViewModel>().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    Debug.WriteLine(inner);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
        }

        private void BuildConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();

            _config = new EncodingConverterConfig();
            Configuration.GetSection("Settings").Bind(_config);
        }

        private void BuildDIContainer()
        {
            _container.Singleton<IWindowManager, WindowManager>();
            _container.PerRequest<App>();
            _container.PerRequest<EncodingViewModel>();
            _container.PerRequest<EncodingView>();
            _container.Instance<IEncodingConverterConfig>(_config);
        }
    }
}
