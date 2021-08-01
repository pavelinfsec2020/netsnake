using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace NetSnake
{ 
    public partial class Form2 : Form
    {
        private string pathOfTranslFile = Directory.GetCurrentDirectory() + @"\Interface2.lng";
        private string[] stringsOfInterface;
        private SenderOfPackets senderPackets = new SenderOfPackets("DDDDDDDDDDDD", "EEEEEEEEEEEE");
        private byte[] payloadDataTcp;
        private byte[] payloadDataUdp;
        private OpenFileDialog openPayload = new OpenFileDialog();
        public Form2()
        {
            InitializeComponent();
        }
        private void TranslateToSelectedLanguage()
        {

            stringsOfInterface = File.ReadAllLines(pathOfTranslFile, Encoding.UTF8);

            //берем правую часть после = из файла,затем удаляем кавычки строки перевода
            this.Text = stringsOfInterface[0].Split('=')[1].Trim('"');
            groupBox2.Text = stringsOfInterface[1].Split('=')[1].Trim('"');
            label1.Text = stringsOfInterface[2].Split('=')[1].Trim('"');
            dataGridView1.Columns[0].HeaderText = stringsOfInterface[3].Split('=')[1].Trim('"');
            dataGridView1.Columns[1].HeaderText = stringsOfInterface[4].Split('=')[1].Trim('"');
            dataGridView1.Columns[2].HeaderText = stringsOfInterface[5].Split('=')[1].Trim('"');
            dataGridView1.Columns[3].HeaderText = stringsOfInterface[6].Split('=')[1].Trim('"');
            dataGridView1.Columns[4].HeaderText = stringsOfInterface[7].Split('=')[1].Trim('"');
            dataGridView1.Columns[5].HeaderText = stringsOfInterface[8].Split('=')[1].Trim('"');
            dataGridView1.Columns[6].HeaderText = stringsOfInterface[9].Split('=')[1].Trim('"');
            dataGridView1.Columns[7].HeaderText = stringsOfInterface[10].Split('=')[1].Trim('"');
            groupBox1.Text = stringsOfInterface[11].Split('=')[1].Trim('"');
            groupBox3.Text = stringsOfInterface[12].Split('=')[1].Trim('"');
            contextMenuStrip1.Items[0].Text = stringsOfInterface[13].Split('=')[1].Trim('"');
            groupBox4.Text = stringsOfInterface[14].Split('=')[1].Trim('"');
            label2.Text = stringsOfInterface[15].Split('=')[1].Trim('"');
            label7.Text = stringsOfInterface[16].Split('=')[1].Trim('"');
            radioButton1.Text = stringsOfInterface[17].Split('=')[1].Trim('"');
            radioButton2.Text = stringsOfInterface[18].Split('=')[1].Trim('"');
            label13.Text = stringsOfInterface[19].Split('=')[1].Trim('"');
            label10.Text = stringsOfInterface[20].Split('=')[1].Trim('"');
            button5.Text = stringsOfInterface[21].Split('=')[1].Trim('"');
            label19.Text = stringsOfInterface[22].Split('=')[1].Trim('"');
            label16.Text = stringsOfInterface[23].Split('=')[1].Trim('"');
            button6.Text = stringsOfInterface[24].Split('=')[1].Trim('"');
            label20.Text = stringsOfInterface[25].Split('=')[1].Trim('"');
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                TranslateToSelectedLanguage();
            }
            catch (Exception a)
            {

                MessageBox.Show(
                    a.Message,
                    "Warning!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        //******************************************************************************
        //* Получает массив байт для полезной нагрузки из файла для пакета TCP или UDP *
        //******************************************************************************
        private void GetPayloadFromFile(out byte[] payloadData)
        {
            openPayload = new OpenFileDialog();
            openPayload.ShowDialog();
            payloadData = File.ReadAllBytes(openPayload.FileName);

        }

        //*********************************************
        //* Событие нажатия на кнопку отправки пакета *
        //*********************************************
        private void button4_Click_1(object sender, EventArgs e)
        {
            //если выбран ARP пакет
            if (radioButton6.Checked)
            {
               try
               {
                    senderPackets.SendPacket(
                        radioButton2.Checked,
                        textBox5.Text,
                        textBox4.Text,
                        textBox2.Text,
                        textBox3.Text,
                        Convert.ToInt32(textBox14.Text)
                       );
               } 
                catch (Exception m) 
                {
                    MessageBox.Show(m.Message); 
                }               
            }
            
            //если выбран TCP пакет
            if (radioButton7.Checked)
            {
                try
                {
                    string tcpFlag = String.Empty;
                    if (radioButton3.Checked) tcpFlag = "ACK";
                    else if (radioButton4.Checked) tcpFlag = "FIN";
                    else tcpFlag = "SYN";
                     
                        senderPackets.SendPacket(
                        textBox6.Text,
                       (ushort)Convert.ToInt32(textBox7.Text),
                        textBox8.Text,
                       (ushort) Convert.ToInt32( textBox9.Text),
                        tcpFlag,
                        payloadDataTcp,
                        Convert.ToInt32(textBox14.Text)
                       );
                }
                catch (Exception m)
                {
                    MessageBox.Show(m.Message);
                }
            }

            //если выбран UDP пакет
            if (radioButton8.Checked)
            {
             //   try
                //{
                    senderPackets.SendPacket(
                    textBox10.Text,
                   (ushort)Convert.ToInt32(textBox11.Text),
                    textBox12.Text,
                   (ushort)Convert.ToInt32(textBox13.Text),
                    payloadDataUdp,
                    Convert.ToInt32(textBox14.Text)
                   );
                //}
                //catch (Exception m)
                //{
                  //  MessageBox.Show(m.Message);
              //  }
            }
        }
       
        //*********************************************
        //* Событие загрузки полезной нагрузки tcp *
        //*********************************************
        private void button5_Click(object sender, EventArgs e)
        {
            GetPayloadFromFile(out payloadDataTcp);
            button5.Text = Path.GetFileName(openPayload.FileName);
        }
      
        //*********************************************
        //* Событие загрузки полезной нагрузки udp *
        //*********************************************
        private void button6_Click(object sender, EventArgs e)
        {
            GetPayloadFromFile(out payloadDataUdp);
            button6.Text = Path.GetFileName(openPayload.FileName);
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}
