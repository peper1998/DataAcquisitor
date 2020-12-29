namespace DataAcquisitor.Services
{
    public interface IFilesStorageService
    {
        void SaveFile(string filename, string content);
    }
}
