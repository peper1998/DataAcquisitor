using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DataAcquisitor.DataAcquisitionServices;
using DataAcquisitor.Services;
using Xamarin.Forms;

namespace DataAcquisitor.ViewModels
{
    public class UdpReceiverViewModel : INotifyPropertyChanged
    {
        private DeviceClient _deviceClient = DeviceClient.GetInstance();
        private IMessageService _messageService = DependencyService.Get<IMessageService>();

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
            _fileSavedEventAsObservable.Subscribe(_ =>
            {
                _messageService.ShortAlert("File saved!");
                ResetViewModel();
            });

            _isProcessInProgressChangedEventAsObservable = Observable.FromEventPattern(ev => _deviceClient.IsProcessInProgressChangedEvent += ev,
                                                                                    ev => _deviceClient.IsProcessInProgressChangedEvent -= ev);
            _isProcessInProgressChangedEventAsObservable.Subscribe(e => IsProcessInProgress = ((IsProcessInProgressChangedEventArgs)(e.EventArgs)).IsInProgress);
        }

        public UdpReceiverViewModel()
        {
        }

        public Command StartRecieving { get; }
        public Command StopRecieving { get; }

        private IObservable<EventPattern<object>> _fileSavedEventAsObservable;
        private IObservable<EventPattern<object>> _isProcessInProgressChangedEventAsObservable;

        public event PropertyChangedEventHandler PropertyChanged;

        private void ResetViewModel()
        {
            FramesCounter = 0;
            LostFramesCount = 0;
            MeasurementTimerValue = TimeSpan.FromSeconds(0);
            MeasurementTime = 1;
            IsTimerModeSelected = false;
        }

        private int framesCounter;
        public int FramesCounter
        {
            set
            {
                if (framesCounter != value)
                {
                    framesCounter = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FramesCounter)));
                }
            }
            get => framesCounter;
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
                FramesCounter = _deviceClient.FramesCounter;
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