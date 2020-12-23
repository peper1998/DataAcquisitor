using DataAcquisitor.Views;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class MainPageViewModel
    {
        public MainPageViewModel()
        {
            GoToPackedReciever = new Command(async () =>
            {
                var udpVM = new UdpReceiverViewModel("Dupa");
                var udpPage = new UdpReceiverPage();
                udpPage.BindingContext = udpVM;
                await Application.Current.MainPage.Navigation.PushAsync(udpPage);
            });
        }

        public Command GoToPackedReciever { get; }
    }
}
