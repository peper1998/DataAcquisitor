using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
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
                Debug.Write("start");

            });

            StopRecieving = new Command(() =>
            {
                _deviceClient.StopDataStream();
            });

            _framesCountChangedEventAsObservable = Observable.FromEventPattern(ev => _deviceClient.FrameCountChanged += ev,
                                                                                    ev => _deviceClient.FrameCountChanged -= ev);
            _framesCountChangedEventAsObservable.Subscribe(x => FramesCount = ((FramesCountChangesArgs)x.EventArgs).Count);

            _lostFramesCountChangedEventAsObservable = Observable.FromEventPattern(ev => _deviceClient.LostFramesCountChanged += ev,
                                                                                    ev => _deviceClient.LostFramesCountChanged -= ev);
            _lostFramesCountChangedEventAsObservable.Subscribe(x => FramesCount = ((FramesCountChangesArgs)x.EventArgs).Count);
        }

        public UdpReceiverViewModel()
        {
        }

        public Command StartRecieving { get; }
        public Command StopRecieving { get; }

        private IObservable<EventPattern<object>> _framesCountChangedEventAsObservable;
        private IObservable<EventPattern<object>> _lostFramesCountChangedEventAsObservable;

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
    }
}