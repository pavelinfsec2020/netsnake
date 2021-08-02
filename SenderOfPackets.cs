using PacketDotNet;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSnake
{
    //*****************************************************************************
    //* Данный класс реализует отправку ARP,TCP и UDP пакетов через сетевую карту *
    //*****************************************************************************
    class SenderOfPackets
    {
        private EthernetPacket ethernetPacket;
        public SenderOfPackets(string sourceMAC, string destinationMAC)
        {
            ethernetPacket = new EthernetPacket(
                PhysicalAddress.Parse(sourceMAC),
                PhysicalAddress.Parse(destinationMAC),
                EthernetPacketType.None
                ) ;
        }

        private async void IterationSend(int countOfPackets)
        {
            for (int i = 0; i < countOfPackets; i++)
            {
                NetworkInterfaces.chosenDevice.SendPacket(ethernetPacket);
                await Task.Delay(500);
            }
        }        
        ///<summary>
        /// Отправка ARP пакета перегруженным методом
        ///</summary>
        public  void SendPacket(
            bool IsResponce,
            string targetMAC,
            string targetIP,
            string senderMAC,
            string senderIP,
            int count
            )
        {
            ARPOperation respOrRequest;
            if (IsResponce) respOrRequest = ARPOperation.Response;
            else respOrRequest = ARPOperation.Request;
            
                ARPPacket arpPacket = new ARPPacket(
                respOrRequest,
                PhysicalAddress.Parse(targetMAC),
                IPAddress.Parse(targetIP),
                PhysicalAddress.Parse(senderMAC),
                IPAddress.Parse(senderIP)
                );        
            ethernetPacket.PayloadPacket = arpPacket;
            
            IterationSend(count);     
        }

        ///<summary>
        /// Отправка TCP пакета перегруженным методом
        ///</summary>
        public void SendPacket(         
            string destIP,
            ushort destPort,
            string sourceIP,
            ushort sourcePort,
            string tcpFlag,
            byte[] tcpPayload,
            int count
            )
        {
            int lastindex = 0;
            TcpPacket tcpPacket;
            IPv4Packet ipPacket;
            IPAddress ipv4source = IPAddress.Parse(sourceIP);
            IPAddress ipv4dest = IPAddress.Parse(destIP);
            //разбиваем большой массив передаваемых байт на максимально допустимый размер пакетов
            for (int i = 0; i < tcpPayload.Length - 1450; i += 1450)
            {
                
               
                byte[] temp = new byte[1450];
                int temp1 = 0;
                for (int j = i; j < i + 1450; j++)
                {
                    temp[temp1] = tcpPayload[j];
                    temp1++;
                }
                tcpPacket = new TcpPacket(sourcePort, destPort);
                tcpPacket.PayloadData = temp;

                switch (tcpFlag)
                {
                    case "ACK":
                        tcpPacket.Ack = true;
                        break;
                    case "FIN":
                        tcpPacket.Fin = true;
                        break;
                    case "SYN":
                        tcpPacket.Syn = true;
                        break;
                }
                ipPacket = new IPv4Packet(IPAddress.Parse(sourceIP), ipv4source);
                ipPacket.PayloadPacket = tcpPacket;
                ethernetPacket.PayloadPacket = ipPacket;

                IterationSend(count);
                lastindex = i+1450;
            }
            byte[] lastblock = new byte[tcpPayload.Length - lastindex];
            int temp_count = 0;

            for (int i = lastindex; i < tcpPayload.Length; i++)
            {
                lastblock[temp_count] = tcpPayload[i];
                temp_count++;
            }
            tcpPacket = new TcpPacket(sourcePort, destPort);
            tcpPacket.PayloadData = lastblock;

            ipPacket = new IPv4Packet(IPAddress.Parse(sourceIP), ipv4dest);
            ipPacket.PayloadPacket = tcpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            IterationSend(count);

            

           
        }

        ///<summary>
        /// Отправка UDP пакета перегруженным методом
        ///</summary>
        public void SendPacket(
            string destIP,
            ushort destPort,
            string sourceIP,
            ushort sourcePort,
            byte[] udpPayload,
            int count
            )
        {
            int lastindex = 0;
            UdpPacket udpPacket;
            IPv4Packet ipPacket;
            IPAddress ipv4source = IPAddress.Parse(sourceIP);
            IPAddress ipv4dest = IPAddress.Parse(destIP);
            //разбиваем большой массив передаваемых байт на максимально допустимый размер пакетов
            for (int i = 0; i < udpPayload.Length-1450; i+=1450)
            {
                byte[] temp = new byte[1450];
                int temp1 = 0;
                for (int j = i; j < i + 1450; j++)
                {
                    temp[temp1] = udpPayload[j];
                    temp1++;
                }
                 udpPacket = new UdpPacket(sourcePort, destPort);
                udpPacket.PayloadData = temp;

                ipPacket = new IPv4Packet(IPAddress.Parse(sourceIP), ipv4source);
                ipPacket.PayloadPacket = udpPacket;
                ethernetPacket.PayloadPacket = ipPacket;

                IterationSend(count);
                lastindex = i + 1450;
            }
            byte[] lastblock = new byte[udpPayload.Length-lastindex];
            int temp_count = 0;
            
            for (int i = lastindex; i < udpPayload.Length; i++)
            {
                lastblock[temp_count] = udpPayload[i];
                temp_count++;
            }
             udpPacket = new UdpPacket(sourcePort, destPort);
            udpPacket.PayloadData = lastblock;

             ipPacket = new IPv4Packet(IPAddress.Parse(sourceIP),ipv4dest);
            ipPacket.PayloadPacket = udpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            IterationSend(count);
       

        }

    }
}
