using System.Threading.Tasks;

namespace DataAcquisitor.Services
{
    public interface IFilesSharingService
    {
        Task ShareFileAsync(string path);
    }
}
