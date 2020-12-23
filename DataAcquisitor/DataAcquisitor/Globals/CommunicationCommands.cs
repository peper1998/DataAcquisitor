using System.Text;

namespace DataAcquisitor.Globals
{
    public static class CommunicationCommands
    {
        public static byte[] DATA_ON = Encoding.ASCII.GetBytes("data on\r\n");
        public static byte[] DATA_OFF = Encoding.ASCII.GetBytes("data off\r\n");
        public static byte[] STREAM_START = Encoding.ASCII.GetBytes("****");
        public static byte[] STREAM_END = Encoding.ASCII.GetBytes("####");
    }
}
