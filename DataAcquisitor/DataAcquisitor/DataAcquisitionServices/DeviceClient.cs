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

        private CancellationTokenSource _listenerCancellationTokenSource;

        private bool _shouldListen = true;
        public int FramesCount = 0;
        public int LostFramesCount = 0;
        public int LastFramesCount = 0;

        public bool IsProcessInProgress = false;

        IFilesStorageService _filesStorageService;

        public event EventHandler FileSavedEvent;

        public List<MeasurementFrame> FramesList = new List<MeasurementFrame>();

        public void StartConnection()
        {
            _shouldListen = true;
            IsProcessInProgress = true;
            if (ListenerTask == null)
            {
                _listenerCancellationTokenSource = new CancellationTokenSource();
                ListenerTask = Task.Run(() => ListenForMeasurements(_listener, _device), _listenerCancellationTokenSource.Token);
            }

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
            var packetsList = new List<byte[]>();
            while (_shouldListen)
            {
                byte[] bytes = listener.Receive(ref endpoint);
                packetsList.Add(bytes);
                Debug.Write(packetsList.Count);

                if (packetsList.Count > 100)
                {
                    ProcessPackets(packetsList);
                    packetsList = new List<byte[]>();
                }
            }
            ProcessPackets(new List<byte[]>(packetsList));

            var measurementsJson = JsonConvert.SerializeObject(FramesList);
            _filesStorageService.SaveFile(DateTime.Now.ToString("o") + "Measurements.txt", measurementsJson);
            FileSavedEvent.Invoke(this, new FileSavedEventArgs(FramesList.Count));
            IsProcessInProgress = false;
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
                    var frameBytes = packetAsList.Skip(i * 136).Take(136).ToList();
                    var measurementFrame = new MeasurementFrame(frameBytes.ToArray());
                    FramesList.Add(measurementFrame);

                    if (LastFramesCount != measurementFrame.Counter - 1)
                    {
                        var framesDiff = measurementFrame.Counter - LastFramesCount + 1;
                        LostFramesCount += framesDiff;
                    }

                    LastFramesCount = measurementFrame.Counter;
                    FramesCount = measurementFrame.Counter;
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
                        FramesList.Add(new MeasurementFrame(frameBytes.ToArray()));
                    }
                }

                if (packet[0] == '#' && packet[1] == '#' && packet[2] == '#' && packet[3] == '#')
                {
                    int framesCount = packet.Length / 136;
                    var clearedPacket = packet.Skip(4);
                    for (int i = 0; i < framesCount; i++)
                    {
                        var frameBytes = clearedPacket.ToList().Skip(i * 136).Take(136).ToList();
                        FramesList.Add(new MeasurementFrame(frameBytes.ToArray()));
                    }
                }
            }
        }

        private DeviceClient()
        {
            _filesStorageService = DependencyService.Get<IFilesStorageService>();
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