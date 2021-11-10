using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EncodingConverter.UI.WPF.Config;
using EncodingConverter.UI.WPF.ViewModels;
using EncodingConverter.UI.WPF.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EncodingConverter.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost ApplicationHost { get; private set; }

        IEncodingConverterConfig _config;

        protected override void OnStartup(StartupEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ApplicationHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var config = builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();

                    _config = new EncodingConverterConfig();
                    
                    config.GetSection("Settings").Bind(_config);
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddSingleton<IEncodingConverterConfig>(_config)
                        .AddScoped<IEncodingViewModel, EncodingViewModel>()
                        .AddSingleton<EncodingConverterView>();
                })
                .Build();

            using (var services = ApplicationHost.Services.CreateScope())
            {
                var view = services.ServiceProvider.GetRequiredService<EncodingConverterView>();
                view.Show();
            }
        }
    }
}
