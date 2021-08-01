using System;
using System.Windows.Forms;
using SharpPcap;
using System.Collections.Generic;
using System.Threading.Tasks;
using PacketDotNet;
using System.Drawing;
using System.Text;

namespace NetSnake
{
    //**************************************************************************
    //* Данный класс реализует модуль перехвата и анализа содержимого  пакетов *
    //**************************************************************************
    class MonitorPackets
    {
        private const int columnMatrixCount = 16;
        private int countOfPackets = 0;
        private ICaptureDevice chosenDevace;
        public  static List<byte[]> packetsData;
        private List<string[]> tableOfPackets;
        private TableFilter filter;
        private TextBox filterBox;
        private DataGridView dataPacketViewerHEX;
        private DataGridView dataPacketViewerASCII;
        private DataGridView viewerOfPackets;
        private Button startBtnCapture;
        private Button stopBtnCapture;
        private Button saveBtnDump;
        private ContextMenuStrip menu;
        public static bool IsActiveCapture = false;
        private bool IsUtfClicked = false;
        private bool IsHexClicked = false;
        private delegate void autoScrollPacketTable(int rowIndex);
        private delegate void showInDataGrid(
            string countOfPacket,
            string timeValue,
            string IPSource,
            string PortSource,
            string IPDestin,
            string PortDestin,
            string protocol,
            string length
            );

        public MonitorPackets(
            ICaptureDevice chosenDevace,
            TextBox filterBox,
            DataGridView dataPacketViewerHEX,
            DataGridView dataPacketViewerASCII,
            DataGridView viewerOfPackets,
            Button startCapture,
            Button stopCapture,
            Button saveDump,
            ContextMenuStrip menu
              )
        {
            this.chosenDevace = chosenDevace;
            this.filterBox = filterBox;
            this.viewerOfPackets = viewerOfPackets;
            this.dataPacketViewerHEX = dataPacketViewerHEX;
            this.dataPacketViewerASCII = dataPacketViewerASCII;
            this.startBtnCapture = startCapture;
            this.stopBtnCapture = stopCapture;
            this.saveBtnDump = saveDump;
            tableOfPackets = new List<string[]>();
            filter = new TableFilter(filterBox,tableOfPackets,viewerOfPackets);
            this.menu = menu;
            packetsData = new List<byte[]>();

            StarsCatchPackets();
            InicializeInterfaceEvents();
        }
        ~MonitorPackets()
        {
            chosenDevace.StopCapture(); 
            chosenDevace =null;
             packetsData=null;
             tableOfPackets = null ;
             filter=null;      
    }

        //------------------------------------------------------------
        //Присваиваем методы, реализующие события winforms интерфейсов
        //------------------------------------------------------------
        private void InicializeInterfaceEvents()
        {
            chosenDevace.OnPacketArrival += new PacketArrivalEventHandler(GetPacketsEvent);
            viewerOfPackets.CellClick += new DataGridViewCellEventHandler(PacketCellClick);

            dataPacketViewerASCII.SelectionChanged += new EventHandler(SelectedUTFPackets);
            dataPacketViewerASCII.MouseClick += new MouseEventHandler(ASCIICellsCopy);
            dataPacketViewerASCII.Scroll += new ScrollEventHandler(SynchronizedScrollWithHEX);

            dataPacketViewerHEX.MouseClick += new MouseEventHandler(HexCellsCopy);
            dataPacketViewerHEX.Scroll += new ScrollEventHandler(SynchronizedScrollWithUTF);

            menu.ItemClicked += new ToolStripItemClickedEventHandler(MenuItemClick);
            filterBox.TextChanged += new EventHandler(FilterTextChanged);
            startBtnCapture.Click += new EventHandler(ClickStartCapture);
            stopBtnCapture.Click += new EventHandler(ClickStopCapture);
            saveBtnDump.Click += new EventHandler(DumpClick);
        }

        //---------------------------------------------------
        // Происходит при нажатии пункта в меню ПКМ
        //---------------------------------------------------
        private void DumpClick(object sender, EventArgs e)
        {
            PcapDump.WriteBytesInPcapFile(packetsData);
        }
        
        //---------------------------------------------------
        // Происходит при нажатии пункта в меню ПКМ
        //---------------------------------------------------
        private void MenuItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (IsUtfClicked)
            {
                CopySelectedCells(dataPacketViewerASCII);
                IsUtfClicked = false;
            }
            if (IsHexClicked)
            {
                CopySelectedCells(dataPacketViewerHEX);
                IsHexClicked = false;
            }
        }
        //---------------------------------------------------
        // Общий случай копирования значений выделенных ячеек
        //---------------------------------------------------
        private void CopySelectedCells(DataGridView DG)
        {
            string buffer = string.Empty;
            for (int rowIndex = 0; rowIndex < DG.RowCount; rowIndex++)
            {
                string tempOfSelRow = String.Empty;
                for (int columnIndex = 0; columnIndex < DG.ColumnCount; columnIndex++)
                {
                    if (DG.Rows[rowIndex].Cells[columnIndex].Selected &&
                       DG.Rows[rowIndex].Cells[columnIndex].Value != null
                        )
                        tempOfSelRow += DG.Rows[rowIndex].Cells[columnIndex].Value.ToString();
                }
                if (tempOfSelRow != String.Empty)
                    buffer += (tempOfSelRow) + "\n";
            }
            Clipboard.SetText(buffer);
        }

        //-----------------------------------------------------
        // Копирует содержимое выделенных пакетов в UTF в буфер
        //-----------------------------------------------------
        private void ASCIICellsCopy(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                IsUtfClicked = true;
                menu.Show(Control.MousePosition);
                
            }

        }

        //---------------------------------------------
        // Копирует содержимое выделенных пакетов в HEX
        //---------------------------------------------
        private void HexCellsCopy(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                IsHexClicked = true;
                menu.Show(Control.MousePosition);
            }
        }

        //------------------------------------------
        // Нажатие на кнопку запуска захвата пакетов
        //------------------------------------------
        private void ClickStartCapture(object sender, EventArgs e)
        {
            ClearAllDataGrids();
            packetsData.Clear();
            tableOfPackets.Clear();
            countOfPackets = 0;
            IsActiveCapture = true;
        }

        //---------------------------------------------
        // Очищает все таблицы интерфейса
        //---------------------------------------------
        private void ClearAllDataGrids()
        {
            viewerOfPackets.Rows.Clear();
            dataPacketViewerHEX.Rows.Clear();
            dataPacketViewerASCII.Rows.Clear();
        }

        //---------------------------------------------
        // Нажатие на кнопку завершения захвата пакетов
        //---------------------------------------------
        private void ClickStopCapture(object sender, EventArgs e)
        {
            IsActiveCapture = false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------
        // Изменяет цвет фона поля фильтра на красный, если ключевая команда введена неверно, на зеленый-верно, белый- поле фильтра пусто
        //-------------------------------------------------------------------------------------------------------------------------------
        private void FilterTextChanged(object senderm, EventArgs e)
        {
            dataPacketViewerHEX.FirstDisplayedScrollingRowIndex = dataPacketViewerASCII.FirstDisplayedScrollingRowIndex;
        }

        //----------------------------------------------------------------------------------------
        // При прокрутке ползунка в окне вывода UTF,автоматически прокручивает и в HEX окне
        //----------------------------------------------------------------------------------------
        private void SynchronizedScrollWithHEX(object senderm, ScrollEventArgs e)
        {
            dataPacketViewerHEX.FirstDisplayedScrollingRowIndex = dataPacketViewerASCII.FirstDisplayedScrollingRowIndex;
        }

        //----------------------------------------------------------------------------------------
        // При прокрутке ползунка в окне вывода HEX,автоматически прокручивает и в UTF окне
        //----------------------------------------------------------------------------------------
        private void SynchronizedScrollWithUTF(object senderm, ScrollEventArgs e)
        {
            dataPacketViewerASCII.FirstDisplayedScrollingRowIndex = dataPacketViewerHEX.FirstDisplayedScrollingRowIndex;
        }

        //-------------------------------------------------------
        //Происходиит при выделении содержимого пакета в окне utf 
        //-------------------------------------------------------
        private void SelectedUTFPackets(object sender, EventArgs e)
        {

            SelectCellsInAnotherDG(dataPacketViewerASCII, dataPacketViewerHEX);

        }

        //-------------------------------------------------------------
        //Выделяет соответствующие hex значения выделенных символов utf
        //-------------------------------------------------------------
        private void SelectCellsInAnotherDG(DataGridView DG1, DataGridView DG2)
        {

            DG2.ClearSelection();
            for (int rowIndex = 0; rowIndex < DG1.RowCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < DG1.ColumnCount; columnIndex++)
                {
                    if (DG1.Rows[rowIndex].Cells[columnIndex].Selected)
                    {
                        DG2.Rows[rowIndex].Cells[columnIndex].Selected = true;
                    }
                }
            }
        }

        //-----------------------------------------------------------
        //Возвращает матрицу из 18 колонок hex значений из hex строки
        //-----------------------------------------------------------
        private string[][] GetHEXArray(string hexString)
        {

            string[] arrHEX = hexString.Split('-');
            int arrHEXLength = arrHEX.Length;
            int indexOfArrHEX = 0;
            int rowMatrixCount = (arrHEXLength / columnMatrixCount) + 1;
            string[][] matrixArrHEX = new string[rowMatrixCount][];

            for (int rowCount = 0; rowCount < rowMatrixCount; rowCount++)
                matrixArrHEX[rowCount] = new string[columnMatrixCount];

            for (int rowIndex = 0; rowIndex < rowMatrixCount; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columnMatrixCount; columnIndex++)
                {
                    if (indexOfArrHEX < arrHEXLength)
                    {
                        matrixArrHEX[rowIndex][columnIndex] = arrHEX[indexOfArrHEX];
                    }
                    else return matrixArrHEX;
                    indexOfArrHEX++;

                }
            }
            return matrixArrHEX;
        }
        //------------------------------------------------------------------------
        //При нажатии на перехваченный пакет показывает его содержимое в utf и HEX
        //------------------------------------------------------------------------
        private void PacketCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 )  return;
            dataPacketViewerHEX.Rows.Clear();
            dataPacketViewerASCII.Rows.Clear();
            byte[] selPackBytes = packetsData[Convert.ToInt32(viewerOfPackets.Rows[e.RowIndex].Cells[0].Value)];
            string HEXString = BitConverter.ToString(selPackBytes);
            string[][] matrixHEXArray = GetHEXArray(HEXString);
            string[][] matrixUTF8Array = ConvertHEXtoASCII(matrixHEXArray);
           
            for (int rowIndex = 0; rowIndex < matrixHEXArray.GetLength(0); rowIndex++)
                dataPacketViewerHEX.Rows.Add(matrixHEXArray[rowIndex]);

            for (int rowIndex = 0; rowIndex < matrixUTF8Array.GetLength(0); rowIndex++)
                dataPacketViewerASCII.Rows.Add(matrixUTF8Array[rowIndex]);
        
           
        }
        //--------------------------------------------------------------------------------------------
        //Преобразует двумерный массив hex значений в двумерный массив с такм же размепрм utf значений
        //--------------------------------------------------------------------------------------------
        private string[][] ConvertHEXtoASCII(string[][] matrixHEXArray)
        {
            int rowMatrixCount = matrixHEXArray.GetLength(0);
            string[][] packetDataInASCII= new string[rowMatrixCount][];
            for (int rowCount = 0; rowCount < rowMatrixCount; rowCount++)
            {
                packetDataInASCII[rowCount] = new string[columnMatrixCount];
            }    
            
            for (int rowIndex = 0; rowIndex < matrixHEXArray.GetLength(0); rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < columnMatrixCount; columnIndex++)
                {              
                    byte[] fromHEX = { Convert.ToByte(matrixHEXArray[rowIndex][columnIndex], 16) };
                    string ASCII = Encoding.ASCII.GetString(fromHEX);
                    //если символ не определился по кодировке ASCII, то символ заменяем на '.' для простоты чтения строки
                    if ((int)fromHEX[0]<32 || (int)fromHEX[0] >127) ASCII = ".";

                    packetDataInASCII[rowIndex][columnIndex] = ASCII;    
                }
            }
            return packetDataInASCII;
        }

        //-------------------------------------------------------------------------------------------
        //Асинхронный запуск потока перехвата пакетов, не нарушая работу пользовательскому интерфейсу
        //-------------------------------------------------------------------------------------------
        private async void StarsCatchPackets()
        {                   
            await Task.Run(() =>
                {
                    try 
                    { 
                        chosenDevace.Capture(); 
                    } 
                    catch (Exception e)
                    { }
                    
                });   
        }

        //-------------------------------------------------------------------------------------------
        //Асинхронный запуск потока перехвата пакетов, не нарушая работу пользовательскому интерфейсу
        //-------------------------------------------------------------------------------------------
        private async  void StopCatchPackets()
        {
            await Task.Run(() =>
            {
                chosenDevace.StopCapture();
            });
        }

        //------------------------------------------------------------------------------------
        //Через делегаты вводит в ДатаГрид информацию о перехваченном пакете из другого потока
        //------------------------------------------------------------------------------------
        private void EnterInfoPacketsInTable(
            string time,
            string IPSource,
            string PortSourse,
            string IPDestination,
            string PortDestination,
            string Protocol,
            string Length,
            Color RowBackColor
            )
        {
            viewerOfPackets.Invoke(new showInDataGrid((A, B, C, D, E, F, G,H) =>
                    viewerOfPackets.Rows.Add(A, B, C, D, E, F, G,H)),
                    countOfPackets.ToString(),
                    time,
                    IPSource,
                    PortSourse,
                    IPDestination,
                    PortDestination,
                    Protocol,
                    Length
                    );
            viewerOfPackets.Rows[viewerOfPackets.RowCount - 2].DefaultCellStyle.BackColor = RowBackColor;
            tableOfPackets.Add(
                new string[] {
                    countOfPackets.ToString(),
                    time,
                    IPSource,
                    PortSourse,
                    IPDestination,
                    PortDestination,
                    Protocol,
                    Length }
            );
            //автоматическая прокрутка вниз при добавлении нового значения из другого потока, через делегает
            viewerOfPackets.Invoke(new autoScrollPacketTable((A) =>
            viewerOfPackets.FirstDisplayedScrollingRowIndex = A),
            viewerOfPackets.RowCount - 1
            );
            countOfPackets++;
        }
       
            //----------------------------------------------------------
            //Событие,захватываюшщее пакеты м определяющее его параметры
            //----------------------------------------------------------
            private async void GetPacketsEvent(object sender, CaptureEventArgs e)
            {
            if (!IsActiveCapture) return;
            try {
                string time = "0." + e.Packet.Timeval.MicroSeconds, IPSource = "", PortSourse = "", IPDestination = "", PortDestination = "", Protocol = "", Length = e.Packet.Data.Length.ToString();
                Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var arpPacket = ARPPacket.GetEncapsulated(packet);
                var tcpPacket = TcpPacket.GetEncapsulated(packet);
                var udpPacket = UdpPacket.GetEncapsulated(packet);
                var ipPacket = IpPacket.GetEncapsulated(packet);
                Color packetBackCol = Color.White;
                //В следующих блоках определяется, какой тип пакета перехвачен
                //если перехвачен ip пакет
                if (ipPacket != null)
                {
                    IPSource = ipPacket.SourceAddress.ToString();
                    PortSourse = "----";
                    IPDestination = ipPacket.DestinationAddress.ToString();
                    PortDestination = "----";
                    Protocol = ipPacket.Protocol.ToString();
                    if (Protocol == "ICMPV6") packetBackCol = Color.LightGreen;
                    else packetBackCol = Color.LightYellow;
                }

                //если перехвачен tcp пакет
                if (tcpPacket != null)
                {
                    PortSourse = tcpPacket.SourcePort.ToString();
                    PortDestination = tcpPacket.DestinationPort.ToString();
                    Protocol = "TCP";
                    packetBackCol = Color.Aquamarine;
                }

                //если перехвачен udp пакет
                if (udpPacket != null)
                {
                    PortSourse = udpPacket.SourcePort.ToString();
                    PortDestination = udpPacket.DestinationPort.ToString();
                    Protocol = "UDP";
                    packetBackCol = Color.LightSalmon;
                }

                //если перехвачен arp пакет
                if (arpPacket != null)
                {
                    Protocol = "ARP";
                    IPSource = arpPacket.SenderProtocolAddress.MapToIPv4().ToString();
                    IPDestination = arpPacket.TargetProtocolAddress.MapToIPv4().ToString();
                    PortSourse = "----";
                    PortDestination = "----";
                    packetBackCol = Color.LightPink;
                }
                EnterInfoPacketsInTable(
                    time,
                    IPSource,
                    PortSourse,
                    IPDestination,
                    PortDestination,
                    Protocol,
                    Length,
                    packetBackCol
                    );
                packetsData.Add(e.Packet.Data);
                await Task.Delay(1000);
            }
            catch (Exception) 
            {
            
            }
            
            
        }
        
    }
}
