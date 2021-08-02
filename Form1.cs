using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace NetSnake
{
    public partial class Form1 : Form
    {
        NetworkInterfaces netInterfaces;

        private string pathOfTranslFile = Directory.GetCurrentDirectory() + @"\Interface1.lng";
        private string[] stringsOfInterface;
        
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
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
            chart1.ChartAreas[0].BackImage = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\graphicBackImage.jpg";
              netInterfaces = new NetworkInterfaces(label3, label1,label2,listBox1, chart1,label5,label6,button1);   
        }
        private void TranslateToSelectedLanguage()
        {
           
            stringsOfInterface = File.ReadAllLines(pathOfTranslFile, Encoding.UTF8);
            this.Text = stringsOfInterface[0].Split('=')[1].Trim('"');
            groupBox1.Text = stringsOfInterface[1].Split('=')[1].Trim('"');
            label7.Text = stringsOfInterface[2].Split('=')[1].Trim('"');
            label8.Text = stringsOfInterface[3].Split('=')[1].Trim('"');
            groupBox5.Text = stringsOfInterface[4].Split('=')[1].Trim('"');
            label9.Text = stringsOfInterface[5].Split('=')[1].Trim('"');
            label10.Text = stringsOfInterface[6].Split('=')[1].Trim('"');
            groupBox3.Text = stringsOfInterface[7].Split('=')[1].Trim('"');
            groupBox2.Text = stringsOfInterface[8].Split('=')[1].Trim('"');
            groupBox4.Text = stringsOfInterface[9].Split('=')[1].Trim('"');
            button1.Text = stringsOfInterface[10].Split('=')[1].Trim('"');


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Описание проекта
            Process.Start("https://sourceforge.net/projects/netsnake-sniffer/");
        }
    }
}
