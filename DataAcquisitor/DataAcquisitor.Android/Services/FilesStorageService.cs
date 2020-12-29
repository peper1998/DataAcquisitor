using System.IO;
using DataAcquisitor.Droid.Services;
using DataAcquisitor.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(FilesStorageService))]
namespace DataAcquisitor.Droid.Services
{
    public class FilesStorageService : IFilesStorageService
    {
        private static string MeasurementsDirectoryPath = "/storage/emulated/0/Android/data/pl.polsl.dataacquisitor/files/measurements";
        private static string MeasurementsDirectoryPatsssh = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
        public void SaveFile(string filename, string content)
        {
            CreateDirectoryIfNotExists();
            File.WriteAllText(Path.Combine(MeasurementsDirectoryPath, filename), content);
        }

        private void CreateDirectoryIfNotExists()
        {
            var ss = MeasurementsDirectoryPatsssh;
            Directory.CreateDirectory(MeasurementsDirectoryPath);
        }
    }
}