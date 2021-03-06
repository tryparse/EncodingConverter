using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EncodingConverter.UI.WPF.Commands;
using EncodingConverter.UI.WPF.Config;
using EncodingConverter.UI.WPF.Models;
using Microsoft.Win32;

namespace EncodingConverter.UI.WPF.ViewModels
{
    public class EncodingViewModel : IEncodingViewModel, INotifyPropertyChanged
    {
        private readonly List<EncodingModel> _encodings;
        private readonly IEncodingConverterConfig _config;

        public ObservableCollection<EncodingModel> SourceEncodings { get; set; }
        public ObservableCollection<EncodingModel> DestinationEncodings { get; set; }

        public EncodingViewModel(IEncodingConverterConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _encodings = Encoding.GetEncodings()

                    .Select(x => new EncodingModel { CodePage = x.CodePage, DisplayName = x.DisplayName, Priority = _config.PriorityEncodings.Contains(x.CodePage) })
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.CodePage)
                    .ToList();

            SourceEncodings = new ObservableCollection<EncodingModel>(_encodings);
            DestinationEncodings = new ObservableCollection<EncodingModel>(_encodings);
        }

        #region UI bindable properties

        private EncodingModel _selectedSourceEncoding;
        public EncodingModel SelectedSourceEncoding
        {
            get => _selectedSourceEncoding;
            set
            {
                _selectedSourceEncoding = value;
                OnPropertyChanged(nameof(SelectedSourceEncoding));
                OnPropertyChanged(nameof(CanConvert));
            }
        }

        private EncodingModel _selectedDestinationEncoding;
        public EncodingModel SelectedDestinationEncoding
        {
            get => _selectedDestinationEncoding;
            set
            {
                _selectedDestinationEncoding = value;
                OnPropertyChanged(nameof(SelectedDestinationEncoding));
                OnPropertyChanged(nameof(CanConvert));
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
                OnPropertyChanged("SourceFilePath");
                OnPropertyChanged("CanConvert");
            }
        }

        private string _operationResult = string.Empty;
        public string OperationResult
        {
            get => _operationResult;
            set
            {
                _operationResult = value;
                OnPropertyChanged(nameof(OperationResult));
            }
        }


        private string _resultPath = string.Empty;
        public string ResultFilePath
        {
            get => _resultPath;
            set
            {
                _resultPath = value;
                OnPropertyChanged(nameof(ResultFilePath));
            }
        }

        private string _operationError;
        public string OperationError
        {
            get => _operationError;
            set
            {
                _operationError = value;
                OnPropertyChanged(nameof(OperationError));
            }
        }

        public bool CanConvert => SelectedSourceEncoding != null
                && SelectedDestinationEncoding != null
                && SelectedSourceEncoding != SelectedDestinationEncoding
                && !string.IsNullOrWhiteSpace(SourceFilePath);

        #endregion UI bindable properties

        #region Commands

        private RelayCommand _selectSourceFile;

        public RelayCommand SelectSourceFile
        {
            get
            {
                if (_selectSourceFile != null)
                {
                    return _selectSourceFile;
                }
                else
                {
                    _selectSourceFile = new RelayCommand((obj) =>
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
                    });
                }

                return _selectSourceFile;
            }
        }

        private RelayCommand _convertCommand;

        public RelayCommand Convert
        {
            get
            {
                if (_convertCommand != null)
                {
                    return _convertCommand;
                }

                _convertCommand = new RelayCommand((obj) =>
                {
                    try
                    {
                        var data = File.ReadAllBytes(_path.FullName);

                        var source = Encoding.GetEncoding(SelectedSourceEncoding.CodePage);
                        var destination = Encoding.GetEncoding(SelectedDestinationEncoding.CodePage);

                        var outputData = new Core.Converter().Convert(source, destination, data);

                        var fileName = $"{_path.Name}_{SelectedDestinationEncoding.DisplayName}_{Guid.NewGuid()}{_path.Extension}";
                        var filePath = Path.Combine(_path.DirectoryName, fileName);

                        File.WriteAllBytes(filePath, outputData);

                        OperationResult = fileName;
                        ResultFilePath = filePath;
                    }
                    catch (Exception ex)
                    {
                        OperationError = ex.Message;
                    }
                });

                return _convertCommand;
            }
        }

        private RelayCommand _openNewFile;

        public RelayCommand OpenNewFile
        {
            get
            {
                if (_openNewFile != null)
                {
                    return _openNewFile;
                }

                _openNewFile = new RelayCommand((obj) =>
                {
                    if (!string.IsNullOrWhiteSpace(ResultFilePath)
                        && File.Exists(ResultFilePath))
                    {
                        using var p = new Process();

                        p.StartInfo = new ProcessStartInfo(ResultFilePath)
                        {
                            CreateNoWindow = false,
                            UseShellExecute = true
                        };

                        p.Start();
                    }
                });

                return _openNewFile;
            }
        }

        #endregion Commands

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion INotifyPropertyChanged
    }
}
