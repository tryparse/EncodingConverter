using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EncodingConverter.UI.WPF.Models;

namespace EncodingConverter.UI.WPF.ViewModels
{
    public interface IEncodingViewModel
    {
        ObservableCollection<EncodingModel> SourceEncodings { get; }
        ObservableCollection<EncodingModel> TargetEncodings { get; }
        EncodingModel SelectedSourceEncoding { get; }
        EncodingModel SelectedDestinationEncoding { get; }
        string SourceFilePath { get; }
        string OperationResult { get; }
        bool CanConvert { get; }

        ICommand SelectSourceFile { get; }
        ICommand Convert { get; }
    }
}
