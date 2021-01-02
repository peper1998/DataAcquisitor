using System.Collections.Generic;

namespace DataAcquisitor.Services
{
    public interface IFilesStorageService
    {
        void SaveFile(string filename, string content);
        void SaveFile(string filename, byte[] content);
        List<string> GetMeasurementFiles();
        void DeleteFile(string path);
    }
}
