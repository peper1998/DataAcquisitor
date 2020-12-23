using System;
using System.Collections.Generic;

namespace DataAcquisitor.Models
{
    public class MeasurementFrame
    {
        public ushort Counter { get; set; }
        List<ushort> Values { get; set; }
        byte[] MeasurementValue { get; set; }
        byte Mpx { get; set; }
        byte Separator { get; set; }

        public MeasurementFrame(byte[] bytes)
        {
            Values = new List<ushort>();
            for (int i = 0; i < 126; i += 2)
            {
                Values.Add(BitConverter.ToUInt16(bytes, i));
            }

            Counter = BitConverter.ToUInt16(bytes, 128);
            MeasurementValue = new byte[] { bytes[130], bytes[131], bytes[132], bytes[133] };
            Mpx = bytes[134];
            Separator = bytes[135];
        }
    }
}
