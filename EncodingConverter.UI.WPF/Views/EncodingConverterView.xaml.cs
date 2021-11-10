using System.Windows;
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
