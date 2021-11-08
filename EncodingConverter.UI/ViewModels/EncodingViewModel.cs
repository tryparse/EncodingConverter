using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using EncodingConverter.UI.Models;
using Microsoft.Win32;

namespace EncodingConverter.UI.ViewModels
{
    public class EncodingViewModel : Screen
    {
        private readonly List<EncodingModel> _encodings;
        private readonly IEncodingConverterConfig _config;

        public BindableCollection<EncodingModel> SourceEncodings { get; set; }
        public BindableCollection<EncodingModel> TargetEncodings { get; set; }

        private EncodingModel _selectedSourceEncoding;

        public EncodingModel SelectedSourceEncoding 
        {
            get => _selectedSourceEncoding; 
            set
            {
                _selectedSourceEncoding = value;
                NotifyOfPropertyChange(() => SelectedSourceEncoding);
                NotifyOfPropertyChange(() => CanConvert);
            }
        }

        private EncodingModel _selectedDestinationEncoding;
        public EncodingModel SelectedDestinationEncoding
        {
            get => _selectedDestinationEncoding;
            set
            {
                _selectedDestinationEncoding = value;
                NotifyOfPropertyChange(() => SelectedDestinationEncoding);
                NotifyOfPropertyChange(() => CanConvert);
            }
        }

        private FileInfo _path;

        private string _sourceFilePath;
        public string SourceFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_sourceFilePath))
                {
                    return _sourceFilePath;
                }

                var pathLengthLimit = 50;

                return _sourceFilePath.Length <= pathLengthLimit
                    ? _sourceFilePath
                    : "..." + _sourceFilePath.Substring(_sourceFilePath.Length - pathLengthLimit);
            }
            set
            {
                _sourceFilePath = value;
                NotifyOfPropertyChange(() => SourceFilePath);
                NotifyOfPropertyChange(() => CanConvert);
            }
        }

        public string _operationResult;

        public string OperationResult
        {
            get => _operationResult;
            set
            {
                _operationResult = value;
                NotifyOfPropertyChange(() => OperationResult);
            }
        }

        public EncodingViewModel(IEncodingConverterConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _encodings = Encoding.GetEncodings()
                    
                    .Select(x => new EncodingModel { CodePage = x.CodePage, DisplayName = x.DisplayName, Priority = _config.PriorityEncodings.Contains(x.CodePage)})
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.DisplayName)
                    .ToList();

            SourceEncodings = new BindableCollection<EncodingModel>(_encodings);
            TargetEncodings = new BindableCollection<EncodingModel>(_encodings);
            
        }

        public void SelectSourceFile()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue
                && dialogResult.Value)
            {
                SourceFilePath = dialog.FileName;
                _path = new FileInfo(dialog.FileName);
            }
        }

        public bool CanConvert => SelectedSourceEncoding != null
                && SelectedDestinationEncoding != null
                && SelectedSourceEncoding != SelectedDestinationEncoding
                && !string.IsNullOrWhiteSpace(SourceFilePath);

        public async Task Convert()
        {
            await Task.Run(async () =>
            {
                var data = await File.ReadAllBytesAsync(_path.FullName);

                var source = Encoding.GetEncoding(SelectedSourceEncoding.CodePage);
                var destination = Encoding.GetEncoding(SelectedDestinationEncoding.CodePage);

                var outputData = new EncodingConverter.Core.Converter().Convert(source, destination, data);

                await Task.Delay(1500);

                await File.WriteAllBytesAsync(Path.Combine(_path.DirectoryName, $"{_path.Name}_{SelectedDestinationEncoding.DisplayName}_{Guid.NewGuid()}{_path.Extension}"), outputData);
            }).ContinueWith((previousTask) =>
            {
                OperationResult = "Complete";
            });
        }
    }
}
