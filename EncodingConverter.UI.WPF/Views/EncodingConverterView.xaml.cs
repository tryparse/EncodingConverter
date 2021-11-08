using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EncodingConverter.UI.WPF.ViewModels;

namespace EncodingConverter.UI.WPF.Views
{
    /// <summary>
    /// Interaction logic for EncodingConverterView.xaml
    /// </summary>
    public partial class EncodingConverterView : Window
    {
        private readonly IEncodingViewModel ViewModel;

        public EncodingConverterView(IEncodingViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;

            DataContext = ViewModel;
        }
    }
}
