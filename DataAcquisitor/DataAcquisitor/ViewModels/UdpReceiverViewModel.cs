using System.ComponentModel;
using System.Diagnostics;
using DataAcquisitor.DataAcquisitionServices;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class UdpReceiverViewModel : INotifyPropertyChanged
    {
        private DeviceClient _deviceClient = DeviceClient.GetInstance();

        public UdpReceiverViewModel(string v)
        {
            //_deviceClient = DeviceClient.GetInstance();

            StartRecieving = new Command(async () =>
            {
                _deviceClient.StartConnection();
                Debug.Write("start");

            });

            StopRecieving = new Command(() =>
            {
                _deviceClient.StopDataStream();
            });
        }

        public UdpReceiverViewModel()
        {
        }

        public Command StartRecieving { get; }
        public Command StopRecieving { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        public int FramesCount
        {
            get => _deviceClient.FramesCount;
        }
    }
}