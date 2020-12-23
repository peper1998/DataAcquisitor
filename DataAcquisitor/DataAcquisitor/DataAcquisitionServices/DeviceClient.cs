﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DataAcquisitor.Globals;
using DataAcquisitor.Models;

namespace DataAcquisitor.DataAcquisitionServices
{
    public class DeviceClient
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private IPEndPoint _device = new IPEndPoint(IPAddress.Parse("192.168.4.2"), 2000);
        private UdpClient _listener = new UdpClient(new IPEndPoint(IPAddress.Any, 2000));

        Task ListenerTask;
        Task ProcesserTask;

        Queue<byte[]> packetsBuffer = new Queue<byte[]>();
        List<byte> bytesList = new List<byte>();
        private bool _isProcessInProgress = true;
        private bool _shouldListen = true;
        public int FramesCount = 0;

        public event EventHandler FrameCountChanged;


        public void StartConnection()
        {
            _shouldListen = true;
            _isProcessInProgress = true;

            if (ListenerTask == null)
            {
                ListenerTask = Task.Run(() => ListenForMeasurements(_listener, _device));
            }

            if (ProcesserTask == null)
            {
                ProcesserTask = Task.Run(() => ProcessData());
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
        }

        private async Task ListenForMeasurements(UdpClient listener, IPEndPoint endpoint)
        {
            while (_shouldListen)
            {
                byte[] bytes = listener.Receive(ref endpoint);
                Debug.Write(bytes.Length);
                packetsBuffer.Enqueue(bytes);
            }
            _isProcessInProgress = false;
            //string fileName = "/storage/emulated/0/Android/data/com.companyname.dataacquisitor/files/outputfile.txt";

            //File.WriteAllBytes(fileName, packetsBuffer.ToArray());
        }

        private async Task ProcessData()
        {
            var framesList = new List<MeasurementFrame>();
            while (_isProcessInProgress)
            {
                if (packetsBuffer.Count > 0)
                {
                    var packet = packetsBuffer.Dequeue();

                    if (packet.Length == 272 || packet.Length == 408)
                    {
                        int framesCount = packet.Length / 136;
                        for (int i = 0; i < framesCount; i++)
                        {
                            var frameBytes = packet.ToList().Skip(i * 136).Take(136).ToList();
                            var measurementFrame = new MeasurementFrame(frameBytes.ToArray());
                            framesList.Add(measurementFrame);

                            FramesCount = measurementFrame.Counter;
                            FrameCountChanged.Invoke(this, EventArgs.);
                            Debug.Write("Added frame with length: " + packet.Length);
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
                                Debug.Write("Added frame with lengthh: " + packet.Length);
                            }
                        }

                        if (packet[0] == '#' && packet[1] == '#' && packet[2] == '#' && packet[3] == '#')
                        {
                            // koniec strumienia
                        }
                    }
                }
            }
        }

        private DeviceClient() { }

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