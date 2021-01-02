using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DataAcquisitor.Models;
using DataAcquisitor.Services;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class MeasurementFilesViewModel : INotifyPropertyChanged
    {
        private IFilesStorageService _filesStorageService = DependencyService.Get<IFilesStorageService>();
        private IFilesSharingService _filesSharingService = DependencyService.Get<IFilesSharingService>();
        private IMessageService _messageService = DependencyService.Get<IMessageService>();

        private List<FileItem> _filesList;
        public List<FileItem> FilesList
        {
            get => _filesList;

            set
            {
                if (_filesList != value)
                {
                    _filesList = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilesList)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MeasurementFilesViewModel()
        {
            FilesList = _filesStorageService.GetMeasurementFiles().Select(f => new FileItem(f.Split('/').Last(), f)).ToList();

            ShareFile = new Command(async (file) =>
            {
                var fileToShare = (file as FileItem);
                await _filesSharingService.ShareFileAsync(fileToShare.Path);
            });

            DeleteFile = new Command((file) =>
            {
                var fileToDelete = (file as FileItem);
                try
                {
                    _filesStorageService.DeleteFile(fileToDelete.Path);
                    _messageService.ShortAlert("File deleted");
                }
                catch (Exception)
                {
                    _messageService.ShortAlert("Error occured when deleting file!");
                }

                FilesList = _filesStorageService.GetMeasurementFiles().Select(f => new FileItem(f.Split('/').Last(), f)).ToList(); ;
            });
        }

        public Command ShareFile { get; }
        public Command DeleteFile { get; }
    }
}
