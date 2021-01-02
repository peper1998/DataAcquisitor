using System;
using System.Collections.Generic;

namespace DataAcquisitor.Models
{
    public class BasicMeasurementFrame
    {
        public List<byte> CounterBytes { get; set; }
        public ushort Counter { get; set; }
        public List<byte> Measurements { get; set; }

        public BasicMeasurementFrame(List<byte> bytes)
        {
            CounterBytes = bytes.GetRange(128, 2);
            Counter = BitConverter.ToUInt16(bytes.ToArray(), 128);
            Measurements = bytes.GetRange(0, 128);
        }
    }
}
