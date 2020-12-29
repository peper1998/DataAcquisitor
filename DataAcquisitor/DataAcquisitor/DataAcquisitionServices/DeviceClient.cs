using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Task ProcesserTask;

        private CancellationTokenSource _listenerCancellationTokenSource;

        Queue<byte[]> packetsBuffer = new Queue<byte[]>();
        List<byte> bytesList = new List<byte>();
        private bool _isProcessInProgress = true;
        private bool _shouldListen = true;
        public int FramesCount = 0;
        public int LostFramesCount = 0;
        public int LastFramesCount = 0;

        IFilesStorageService _filesStorageService;

        public event EventHandler FrameCountChanged;
        public event EventHandler LostFramesCountChanged;
        public event EventHandler IsProcessInProgressChanged;


        public void StartConnection()
        {
            _shouldListen = true;
            IsProcessInProgress = true;

            if (ListenerTask == null)
            {
                _listenerCancellationTokenSource = new CancellationTokenSource();
                ListenerTask = Task.Run(() => ListenForMeasurements(_listener, _device), _listenerCancellationTokenSource.Token);
            }

            //if (ProcesserTask == null)
            //{
            //    ProcesserTask = Task.Run(() => ProcessData());
            //}

            s.SendTo(CommunicationCommands.DATA_ON, _device);
            Task.Delay(200);
            s.SendTo(CommunicationCommands.DATA_ON, _device);
            Task.Delay(200);
            s.SendTo(CommunicationCommands.DATA_ON, _device);
        }

        public void StopDataStream()
        {
            s.SendTo(CommunicationCommands.DATA_OFF, _device);
            Task.Delay(200);
            s.SendTo(CommunicationCommands.DATA_OFF, _device);
            Task.Delay(200);
            s.SendTo(CommunicationCommands.DATA_OFF, _device);
            Task.Delay(200);

            _shouldListen = false;
            _listenerCancellationTokenSource.Cancel();
            IsProcessInProgress = false;
        }

        private async Task ListenForMeasurements(UdpClient listener, IPEndPoint endpoint)
        {
            var framesList = new List<MeasurementFrame>();
            while (_shouldListen)
            {
                byte[] bytes = listener.Receive(ref endpoint);
                Debug.Write(bytes.Length);
                packetsBuffer.Enqueue(bytes);
                if (packetsBuffer.Count > 0)
                {
                    var packet = bytes; 
                    // Debug.Write(packet.Length);
                    if (packet.Length == 272 || packet.Length == 408)
                    {
                        int framesCount = packet.Length / 136;
                        for (int i = 0; i < framesCount; i++)
                        {
                            var frameBytes = packet.ToList().Skip(i * 136).Take(136).ToList();
                            var measurementFrame = new MeasurementFrame(frameBytes.ToArray());
                            framesList.Add(measurementFrame);
                            //Debug.Write(measurementFrame.Counter);

                            if (LastFramesCount != measurementFrame.Counter - 1)
                            {
                                var framesDiff = measurementFrame.Counter - LastFramesCount + 1;
                                LostFramesCount += framesDiff;
                            }

                            LastFramesCount = measurementFrame.Counter;
                            FramesCount = measurementFrame.Counter;
                            // Debug.Write("Added frame with length: " + packet.Length);
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
                                var frameBytes = clearedPacket.ToList().Skip(i * 136).Take(136).ToList();
                                framesList.Add(new MeasurementFrame(frameBytes.ToArray()));
                                //Debug.Write("Added frame with lengthh: " + packet.Length);
                            }
                        }

                        if (packet[0] == '#' && packet[1] == '#' && packet[2] == '#' && packet[3] == '#')
                        {
                            break;
                        }
                    }
                }
            }
            var measurementsJson = JsonConvert.SerializeObject(framesList);
            _filesStorageService.SaveFile(DateTime.Now.ToString("o") + "Measurements.txt", measurementsJson);
            IsProcessInProgress = false;
        }

        private async Task ProcessData()
        {
            var framesList = new List<MeasurementFrame>();
            while (IsProcessInProgress)
            {
                //if (packetsBuffer.Count > 0)
                //{
                //    var packet = packetsBuffer.Dequeue();
                //    Debug.Write(packet.Length);
                //    if (packet.Length == 272 || packet.Length == 408)
                //    {
                //        int framesCount = packet.Length / 136;
                //        for (int i = 0; i < framesCount; i++)
                //        {
                //            var frameBytes = packet.ToList().Skip(i * 136).Take(136).ToList();
                //            var measurementFrame = new MeasurementFrame(frameBytes.ToArray());
                //            framesList.Add(measurementFrame);
                //            Debug.Write(measurementFrame.Counter);

                //            if (LastFramesCount != measurementFrame.Counter - 1)
                //            {
                //                var framesDiff = measurementFrame.Counter - LastFramesCount + 1;
                //                LostFramesCount += framesDiff;
                //            }

                //            LastFramesCount = measurementFrame.Counter;
                //            FramesCount = measurementFrame.Counter;
                //            // Debug.Write("Added frame with length: " + packet.Length);
                //        }
                //    }
                //    else
                //    {
                //        if (packet[0] == '*' && packet[1] == '*' && packet[2] == '*' && packet[3] == '*')
                //        {
                //            int framesCount = packet.Length / 136;
                //            var clearedPacket = packet.Skip(4);
                //            for (int i = 0; i < framesCount; i++)
                //            {
                //                var frameBytes = clearedPacket.ToList().Skip(i * 136).Take(136).ToList();
                //                framesList.Add(new MeasurementFrame(frameBytes.ToArray()));
                //                Debug.Write("Added frame with lengthh: " + packet.Length);
                //            }
                //        }

                //        if (packet[0] == '#' && packet[1] == '#' && packet[2] == '#' && packet[3] == '#')
                //        {
                //            // koniec strumienia
                //        }
                //    }
                //}
            }

            var measurementsJson = JsonConvert.SerializeObject(framesList);
            _filesStorageService.SaveFile(DateTime.Now.ToString("o") + "Measurements.txt", measurementsJson);
        }

        public bool IsProcessInProgress
        {
            set
            {
                _isProcessInProgress = value;
                IsProcessInProgressChanged.Invoke(this, new IsProcessInProgressChangedArgs(_isProcessInProgress));
            }
            get => _isProcessInProgress;
        }
        private DeviceClient()
        {
            _filesStorageService = DependencyService.Get<IFilesStorageService>();
        }

        private static DeviceClient _instance;

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

public class FramesCountChangesArgs : EventArgs
{
    public int Count { get; set; }

    public FramesCountChangesArgs(int count)
    {
        Count = count;
    }
}

public class IsProcessInProgressChangedArgs : EventArgs
{
    public bool IsInProgress { get; set; }

    public IsProcessInProgressChangedArgs(bool isInProgress)
    {
        IsInProgress = isInProgress;
    }
}