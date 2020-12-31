using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DataAcquisitor.DataAcquisitionServices;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class UdpReceiverViewModel : INotifyPropertyChanged
    {
        private DeviceClient _deviceClient = DeviceClient.GetInstance();

        public UdpReceiverViewModel(string v)
        {
            StartRecieving = new Command(async () =>
            {
                _deviceClient.StartConnection();

                StartTimer();

                if (isTimerModeSelected)
                {
                    Task.Delay(new TimeSpan(0, measurementTime, 0)).ContinueWith(o => { _deviceClient.StopDataStream(); });
                }

                Debug.Write("start");

            });

            StopRecieving = new Command(() =>
            {
                _deviceClient.StopDataStream();
            });

            _fileSavedEventAsObservable = Observable.FromEventPattern(ev => _deviceClient.FileSavedEvent += ev,
                                                                                    ev => _deviceClient.FileSavedEvent -= ev);
            _fileSavedEventAsObservable.Subscribe(async x => await Application.Current.MainPage.DisplayAlert(
                "Process completed", ((FileSavedEventArgs)x.EventArgs).FramesCount + "frames has been processed and saved in file!",
                "Ok")
            );
        }

        public UdpReceiverViewModel()
        {
        }

        public Command StartRecieving { get; }
        public Command StopRecieving { get; }

        private IObservable<EventPattern<object>> _framesCountChangedEventAsObservable;
        private IObservable<EventPattern<object>> _lostFramesCountChangedEventAsObservable;
        private IObservable<EventPattern<object>> _fileSavedEventAsObservable;

        public event PropertyChangedEventHandler PropertyChanged;

        private int framesCount;
        public int FramesCount
        {
            set
            {
                if (framesCount != value)
                {
                    framesCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FramesCount)));
                }
            }
            get => framesCount;
        }

        private int lostFramesCount;
        public int LostFramesCount
        {
            set
            {
                if (lostFramesCount != value)
                {
                    lostFramesCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LostFramesCount)));
                }
            }
            get => lostFramesCount;
        }

        private bool isTimerModeSelected;
        public bool IsTimerModeSelected
        {
            set
            {
                if (isTimerModeSelected != value)
                {
                    isTimerModeSelected = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTimerModeSelected)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableSlider)));
                }
            }
            get => isTimerModeSelected;
        }
        private int measurementTime;
        public int MeasurementTime
        {
            set
            {
                if (measurementTime != value)
                {
                    measurementTime = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasurementTime)));
                }
            }
            get => measurementTime;
        }

        public long measurementTimeInSecs = 0;
        public void StartTimer()
        {
            measurementTimeInSecs = 0;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                measurementTimeInSecs++;
                MeasurementTimerValue = TimeSpan.FromSeconds(measurementTimeInSecs);
                FramesCount = _deviceClient.FramesCount;
                LostFramesCount = _deviceClient.LostFramesCount;
                return _deviceClient.IsProcessInProgress;
            });
        }

        private TimeSpan measurementTimerValue;
        public TimeSpan MeasurementTimerValue
        {
            set
            {
                if (measurementTimerValue != value)
                {
                    measurementTimerValue = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasurementTimerValue)));
                }
            }
            get => measurementTimerValue;
        }
        private bool isProcessInProgress;
        public bool IsProcessInProgress
        {
            set
            {
                if (isProcessInProgress != value)
                {
                    isProcessInProgress = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessInProgress)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableStartMeasurementButton)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableSlider)));
                }
            }
            get => isProcessInProgress;
        }
        public bool EnableStartMeasurementButton
        {
            get => !IsProcessInProgress;
        }

        public bool EnableSlider
        {
            get => !IsProcessInProgress && IsTimerModeSelected;
        }
    }
}