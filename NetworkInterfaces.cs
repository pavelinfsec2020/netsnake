using System;
using System.Windows.Forms;
using SharpPcap;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;


namespace NetSnake
{
    //******************************************************************
    //* Данный класс реализует модуль управления сетевыми интерфейсами *
    //******************************************************************
    class NetworkInterfaces
    {
        private CaptureDeviceList deviceList;
        private MonitorPackets netPackets;
        private Label macAdress, recievePackets, droppPackets, typeInterface,labelTime;
        private Chart graphic;
        private ListBox listOfInerfaces;
        private  List<ICaptureDevice> devaces;
        public static ICaptureDevice chosenDevice;
        private DateTime startTime;
        private Button buttonPackets;
        private int lastCountPackets, newCountPackets;
        
        public NetworkInterfaces(
            Label macAdress,
            Label recievePackets,
            Label droppPackets,
            ListBox listOfInterfaces,
            Chart graphic,
            Label typeInterface,
            Label labelTime,
            Button buttonPackets
            )
        {
            this.macAdress = macAdress;
            this.recievePackets = recievePackets;
            this.droppPackets = droppPackets;
            this.graphic = graphic;
            this.listOfInerfaces = listOfInterfaces;
            this.typeInterface = typeInterface;
            this.labelTime = labelTime;
            this.buttonPackets = buttonPackets;
            startTime = new DateTime(0,0);
            deviceList = CaptureDeviceList.Instance;
            CreateInterfaces();
            listOfInerfaces.SelectedIndexChanged += SelectedNetInterface;
            buttonPackets.Click += AnalyzPacketsClick;
        }
       
        ~NetworkInterfaces()
        {
            devaces = null;
            chosenDevice = null;
            netPackets = null;
            deviceList = null;
        }
       
        //---------------------------------------------------
        // Событие при нажатии на один из сетевых интерфейсов
        //---------------------------------------------------
        private void AnalyzPacketsClick(object sender, System.EventArgs e)
        {
            //если не выбран сетевой  интерфейс
            if (listOfInerfaces.SelectedIndex == -1)
            { 
                MessageBox.Show(
                    "No selected NetworkInterfaces!",
                    "Warning!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return; 
            }
            chosenDevice = devaces[listOfInerfaces.SelectedIndex];
            Form2 f2 = new Form2();
            netPackets = null;
            netPackets = new MonitorPackets(
                chosenDevice,
                f2.textBox1,
                f2.dataGridView2,
                f2.dataGridView3,
                f2.dataGridView1,
                f2.button1,
                f2.button2,
                f2.button3,
                f2.contextMenuStrip1
                );
            
            f2.Show();
         
        }
       
        private void SelectedNetInterface(object sender, System.EventArgs e)
        {
            int selectedIndex = listOfInerfaces.SelectedIndex;
            GetMacAdress(selectedIndex);
            GetTypeOfInterface(selectedIndex);
            GetInfoPackets(selectedIndex);
            BuildGraphic(selectedIndex);
            StarTimer(selectedIndex);
        }
        
        //-------------------------------------------------------------------
        //Построение графика на основе числа принятых пакетов за одну секунлу
        //-------------------------------------------------------------------
        private  async void BuildGraphic(int selectedIndex)
        {
         await  Task.Delay(1000);
            int i = 0;lastCountPackets = 0; newCountPackets = 0; graphic.Series[0].Points.Clear();
            while (selectedIndex == listOfInerfaces.SelectedIndex)
            {
                lastCountPackets = Convert.ToInt32(devaces[selectedIndex].Statistics.ReceivedPackets);
                await Task.Delay(1000);
                newCountPackets = Convert.ToInt32(devaces[selectedIndex].Statistics.ReceivedPackets);
                graphic.Series[0].Points.AddXY(i, newCountPackets - lastCountPackets);
                i++;
              
            }
        }
        
        //-----------------------------------------------------------------------
        //Запуск секундомера от начала мониторинга выбранного сетевого интерфейса
        //-----------------------------------------------------------------------
        private async void StarTimer(int selectedIndex)
        {
            startTime = new DateTime(0,0);
            while (selectedIndex == listOfInerfaces.SelectedIndex)
            {
                startTime = startTime.AddMilliseconds(100);
                labelTime.Text = startTime.ToString("HH:mm:ss:fff");
                await Task.Delay(100);
            }
                

        }
      
        //---------------------------------------------------
        //Вывод на экран числа принятых и отброшенных пакетов
        //---------------------------------------------------
        private async void GetInfoPackets( int selectedIndex)
        {
            while (selectedIndex == listOfInerfaces.SelectedIndex) 
            {
               
                    recievePackets.Text = devaces[selectedIndex].Statistics.ReceivedPackets.ToString();
                    droppPackets.Text =   devaces[selectedIndex].Statistics.DroppedPackets.ToString();
                
                await Task.Delay(1000);
            }     
        }
        
        //----------------------------------------------------------------
        //Вывод на экран физического адреса выбранного сетевого интерфейса
        //----------------------------------------------------------------
        private void GetMacAdress(int indexOfInterf)
        {          
            
            if(devaces[indexOfInterf].MacAddress!=null)
                macAdress.Text = devaces[indexOfInterf].MacAddress.ToString();
            else
                macAdress.Text = "unknown";    
        }
       
        //---------------------------------------------------------------------
        //Вывод на экран типа выбранного сетевого интерфейса,например, Ethernet
        //---------------------------------------------------------------------
        private void GetTypeOfInterface(int indexOfInterf)
        { 
          typeInterface.Text = devaces[indexOfInterf].LinkType.ToString();
        }
       
        //-------------------------------------------------------------
        //Инициалищация каждого сетевеого интерфейса для их мониторинга
        //-------------------------------------------------------------
        private void CreateInterfaces()
        {
            devaces = new List<ICaptureDevice>();
            for (int i = 0; i < deviceList.Count; i++)
            {
                devaces.Add(deviceList[i]);
                listOfInerfaces.Items.Add(deviceList[i].Description);
                devaces[i].Open(DeviceMode.Promiscuous, 1000);
              
            }
        }
    }
}
