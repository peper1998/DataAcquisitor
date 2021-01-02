using Android.Widget;
using DataAcquisitor.Droid.Services;
using DataAcquisitor.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(MessageService))]

namespace DataAcquisitor.Droid.Services
{
    public class MessageService : IMessageService
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}