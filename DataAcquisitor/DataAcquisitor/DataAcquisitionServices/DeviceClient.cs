using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DataAcquisitor.Globals;
using DataAcquisitor.Models;
using DataAcquisitor.Services;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace DataAcquisitor.DataAcquisitionServices
{
    public class DeviceClient
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint _device = new IPEndPoint(IPAddress.Parse("192.168.4.2"), 2000);
        private UdpClient _listener = new UdpClient(new IPEndPoint(IPAddress.Any, 2000));

        Task ListenerTask;

        private CancellationTokenSource _listenerCancellationTokenSource;

        IFilesStorageService _filesStorageService = DependencyService.Get<IFilesStorageService>();
        IMessageService _messageService = DependencyService.Get<IMessageService>();


        private bool _shouldListen = true;
        public int FramesCounter = 0;
        public int LostFramesCount = 0;
        public int LastFramesCounter = 0;

        public bool isProcessInProgress = false;
        public bool IsProcessInProgress
        {
            set
            {
                if (isProcessInProgress != value)
                {
                    isProcessInProgress = value;

                    IsProcessInProgressChangedEvent?.Invoke(this, new IsProcessInProgressChangedEventArgs(IsProcessInProgress));
                }
            }
            get => isProcessInProgress;
        }


        public event EventHandler FileSavedEvent;
        public event EventHandler IsProcessInProgressChangedEvent;

        public List<MeasurementFrame> FramesList = new List<MeasurementFrame>();
        public List<BasicMeasurementFrame> BasicFramesList = new List<BasicMeasurementFrame>();

        public void StartConnection()
        {
            _shouldListen = true;
            IsProcessInProgress = true;
            if (ListenerTask == null)
            {
                _listenerCancellationTokenSource = new CancellationTokenSource();
                ListenerTask = Task.Run(() => ListenForMeasurements(_listener, _device), _listenerCancellationTokenSource.Token);
            }

            try
            {
                s.SendTo(CommunicationCommands.DATA_ON, _device);
                Task.Delay(200);
                s.SendTo(CommunicationCommands.DATA_ON, _device);
                Task.Delay(200);
                s.SendTo(CommunicationCommands.DATA_ON, _device);
            }
            catch (Exception)
            {
                _shouldListen = false;
                IsProcessInProgress = false;
                Task.Delay(200);
                _listenerCancellationTokenSource.Cancel();
                ResetClient();
                _messageService.LongAlert("Cannot connect with the device!");
            }
        }

        public void StopDataStream()
        {
            try
            {
                s.SendTo(CommunicationCommands.DATA_OFF, _device);
                Task.Delay(200);
                s.SendTo(CommunicationCommands.DATA_OFF, _device);
                Task.Delay(200);
                s.SendTo(CommunicationCommands.DATA_OFF, _device);
                Task.Delay(200);
                AfterStreamFinished();
            }
            catch (Exception)
            {
                _messageService.LongAlert("Cannot connect with the device!");
                AfterStreamFinished();
            }

            SaveMeasurementsInFile();
        }

        public void AfterStreamFinished()
        {
            _shouldListen = false;
            Task.Delay(200);
            _listenerCancellationTokenSource.Cancel();
            ResetClient();
        }

        private async Task ListenForMeasurements(UdpClient listener, IPEndPoint endpoint)
        {
            var packetsList = new List<byte[]>();
            while (_shouldListen)
            {
                byte[] bytes = listener.Receive(ref endpoint);
                packetsList.Add(bytes);

                if (packetsList.Count > 100)
                {
                    ProcessPackets(packetsList);
                    packetsList = new List<byte[]>();
                }
            }
            ProcessPackets(new List<byte[]>(packetsList));
        }

        private void SaveMeasurementsInFile()
        {
            var measurementsOutput = new List<byte>(new byte[4500]);
            measurementsOutput.AddRange(BasicFramesList.SelectMany(f => f.CounterBytes.Concat(f.Measurements).ToList()));

            var measurementsJson = JsonConvert.SerializeObject(FramesList);
            _filesStorageService.SaveFile(DateTime.Now.ToString("o") + "Measurements.txt", measurementsOutput.ToArray());
            FileSavedEvent.Invoke(this, new FileSavedEventArgs(FramesList.Count));
        }

        private void ProcessPackets(List<byte[]> packets)
        {
            foreach (var packet in packets)
            {
                ProcessPacket(packet);
            }
        }

        private void ProcessPacket(byte[] packet)
        {
            var packetAsList = packet.ToList();
            if (packetAsList.Count == 272 || packetAsList.Count == 408)
            {
                int framesCount = packetAsList.Count / 136;
                for (int i = 0; i < framesCount; i++)
                {
                    var frameBytes = packetAsList.Skip(i * 136).Take(136).ToArray();
                    ParseFrame(frameBytes);
                }
            }
            else
            {
                if (packet[0] == '*' && packet[1] == '*' && packet[2] == '*' && packet[3] == '*')
                {
                    int framesCount = packet.Length / 136;
                    var clearedPacket = packet.Skip(4);
                    for (int i = 0; i < framesCount; i++)
                    {
                        var frameBytes = clearedPacket.ToList().Skip(i * 136).Take(136).ToArray();
                        ParseFrame(frameBytes);
                    }
                }

                if (packet[0] == '#' && packet[1] == '#' && packet[2] == '#' && packet[3] == '#')
                {
                    int framesCount = packet.Length / 136;
                    var clearedPacket = packet.Skip(4);
                    for (int i = 0; i < framesCount; i++)
                    {
                        var frameBytes = clearedPacket.ToList().Skip(i * 136).Take(136).ToArray();
                        ParseFrame(frameBytes);
                    }
                }
            }
        }

        private void ParseFrame(byte[] bytes)
        {
            // var measurementFrame = new MeasurementFrame(bytes);
            var measurementFrame = new BasicMeasurementFrame(bytes.ToList());
            BasicFramesList.Add(measurementFrame);

            if (LastFramesCounter != measurementFrame.Counter - 1)
            {
                var framesDiff = measurementFrame.Counter - LastFramesCounter + 1;

                if (framesDiff < 0)
                {
                    framesDiff = ushort.MaxValue + measurementFrame.Counter - LastFramesCounter + 1;
                }

                LostFramesCount += framesDiff;
            }

            LastFramesCounter = measurementFrame.Counter;
            FramesCounter = measurementFrame.Counter;
        }

        private void ResetClient()
        {
            _shouldListen = true;
            IsProcessInProgress = false;
            FramesCounter = 0;
            LostFramesCount = 0;
            LastFramesCounter = 0;
        }

        private DeviceClient()
        {
        }

        private static DeviceClient _instance;

        public Task ProcessingTask { get; private set; }

        public static DeviceClient GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DeviceClient();
            }
            return _instance;
        }
    }
}

public class FileSavedEventArgs : EventArgs
{
    public long FramesCount { get; set; }

    public FileSavedEventArgs(long framesCount)
    {
        FramesCount = framesCount;
    }
}

public class IsProcessInProgressChangedEventArgs : EventArgs
{
    public bool IsInProgress { get; set; }

    public IsProcessInProgressChangedEventArgs(bool isInProgress)
    {
        IsInProgress = isInProgress;
    }
}