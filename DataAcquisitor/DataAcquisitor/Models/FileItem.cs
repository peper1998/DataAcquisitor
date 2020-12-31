namespace DataAcquisitor.Models
{
    public class FileItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public FileItem(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
