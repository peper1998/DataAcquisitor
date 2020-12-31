using System.Collections.Generic;
using DataAcquisitor.iOS.Services;
using DataAcquisitor.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FilesStorageService))]
namespace DataAcquisitor.iOS.Services
{
    public class FilesStorageService : IFilesStorageService
    {
        public void DeleteFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetMeasurementFiles()
        {
            throw new System.NotImplementedException();
        }

        public void SaveFile(string filename, string content)
        {
            throw new System.NotImplementedException();
        }
    }
}