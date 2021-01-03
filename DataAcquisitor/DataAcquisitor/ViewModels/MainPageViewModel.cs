using DataAcquisitor.Views;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class MainPageViewModel
    {
        public MainPageViewModel()
        {
            GoToPacketReciever = new Command(async () =>
            {
                var viewModel = new UdpReceiverViewModel("d");
                var page = new UdpReceiverPage();
                page.BindingContext = viewModel;
                await Application.Current.MainPage.Navigation.PushAsync(page);
            });

            GoToFilesList = new Command(async () =>
             {
                 var viewModel = new MeasurementFilesViewModel();
                 var page = new MeasurementFilesListPage();
                 page.BindingContext = viewModel;
                 await Application.Current.MainPage.Navigation.PushAsync(page);
             });

        }

        public Command GoToPacketReciever { get; }
        public Command GoToFilesList { get; }
    }
}
