using System.Collections.ObjectModel;
using EncodingConverter.UI.WPF.Commands;
using EncodingConverter.UI.WPF.Models;

namespace EncodingConverter.UI.WPF.ViewModels
{
    public interface IEncodingViewModel
    {
        ObservableCollection<EncodingModel> SourceEncodings { get; }
        ObservableCollection<EncodingModel> DestinationEncodings { get; }
        EncodingModel SelectedSourceEncoding { get; }
        EncodingModel SelectedDestinationEncoding { get; }
        string SourceFilePath { get; }
        string OperationResult { get; }
        bool CanConvert { get; }

        RelayCommand SelectSourceFile { get; }
        RelayCommand Convert { get; }
    }
}
