using System.Threading.Tasks;
using DataAcquisitor.Droid.Services;
using DataAcquisitor.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(FilesSharingService))]
namespace DataAcquisitor.Droid.Services
{
    class FilesSharingService : IFilesSharingService
    {
        public Task ShareFileAsync(string path)
        {
            return Share.RequestAsync(new ShareFileRequest
            {
                Title = "Share File",
                File = new ShareFile(path)
            });
        }
    }
}