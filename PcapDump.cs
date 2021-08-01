using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;

namespace NetSnake
{
    //*******************************************************************************************
    //* Данный статический класс реализует автоматическое создание дампа перехваченных пакетов  *
    //*******************************************************************************************
    static class PcapDump
    {
        private static CaptureFileWriterDevice writerPackets;
        private static string dumpName = Directory.GetCurrentDirectory() + @"\DumpsFolder\";


        public static void WriteBytesInPcapFile(List<byte[]> dumpOfpackets)
        {
            dumpName += DateTime.Now.ToShortDateString() +
            "__" +
            DateTime.Now.ToLongTimeString().Replace(':', '-') +
            ".pcap";
            writerPackets = new CaptureFileWriterDevice(dumpName, System.IO.FileMode.Create);

            foreach (byte[] packetBytes in dumpOfpackets)
            {
                writerPackets.Write(packetBytes);
            }
            
         }
        
    }
}
