﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EncodingConverter.UI.WPF.Models;
using Microsoft.Win32;

namespace EncodingConverter.UI.WPF.ViewModels
{
    public class EncodingViewModel : IEncodingViewModel, INotifyPropertyChanged
    {
        private readonly List<EncodingModel> _encodings;
        private readonly IEncodingConverterConfig _config;

        public ObservableCollection<EncodingModel> SourceEncodings { get; set; }
        public ObservableCollection<EncodingModel> TargetEncodings { get; set; }

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

        public string _operationResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public string OperationResult
        {
            get => _operationResult;
            set
            {
                _operationResult = value;
                OnPropertyChanged("OperationResult");
            }
        }

        public EncodingViewModel(IEncodingConverterConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _encodings = Encoding.GetEncodings()

                    .Select(x => new EncodingModel { CodePage = x.CodePage, DisplayName = x.DisplayName, Priority = _config.PriorityEncodings.Contains(x.CodePage) })
                    .OrderByDescending(x => x.Priority)
                    .ThenBy(x => x.DisplayName)
                    .ToList();

            SourceEncodings = new ObservableCollection<EncodingModel>(_encodings);
            TargetEncodings = new ObservableCollection<EncodingModel>(_encodings);
        }

        private ICommand _selectSourceFile;

        public ICommand SelectSourceFile
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

        public bool CanConvert => SelectedSourceEncoding != null
                && SelectedDestinationEncoding != null
                && SelectedSourceEncoding != SelectedDestinationEncoding
                && !string.IsNullOrWhiteSpace(SourceFilePath);

        private ICommand _convertCommand;

        public ICommand Convert => _convertCommand ?? new RelayCommand((obj) =>
        {
            var data = File.ReadAllBytes(_path.FullName);

            var source = Encoding.GetEncoding(SelectedSourceEncoding.CodePage);
            var destination = Encoding.GetEncoding(SelectedDestinationEncoding.CodePage);

            var outputData = new EncodingConverter.Core.Converter().Convert(source, destination, data);

            File.WriteAllBytes(Path.Combine(_path.DirectoryName, $"{_path.Name}_{SelectedDestinationEncoding.DisplayName}_{Guid.NewGuid()}{_path.Extension}"), outputData);

            OperationResult = "Complete";
        });
    }
}