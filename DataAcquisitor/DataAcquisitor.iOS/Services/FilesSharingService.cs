using System.Threading.Tasks;
using DataAcquisitor.iOS.Services;
using DataAcquisitor.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(FilesSharingService))]

namespace DataAcquisitor.iOS.Services
{
    public class FilesSharingService : IFilesSharingService
    {
        public Task ShareFileAsync(string path)
        {
            return Share.RequestAsync(new ShareFileRequest
            {
                File = new ShareFile(path)
            });
        }
    }
}