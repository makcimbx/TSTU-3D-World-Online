using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSTUVirualWorldServer
{
    public partial class Form1 : Form
    {
        private static GameServer gameServer;
        private static Thread thread;

        private DataBaseEditorForm dataBaseEditorForm;
        private int localPort;

        public Form1()
        {
            InitializeComponent();

            localPort = int.Parse(textBox1.Text);

            dataBaseEditorForm = new DataBaseEditorForm();
            
            gameServer = new GameServer(this, localPort, dataBaseEditorForm.usersTableAdapter);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            thread = new Thread(gameServer.StartListening);
            thread.Start();
            richTextBox1.Text += $"Начинаем прослушивать порт {localPort}\n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gameServer.StopListening();
            if (thread != null) thread.Abort();
            richTextBox1.Text += $"Перестаем прослушивать порт {localPort}\n";
        }

        private void OnApplicationClosed(object sender, FormClosedEventArgs e)
        {
            gameServer.StopListening();
            if (thread != null) thread.Abort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dataBaseEditorForm.Show();
        }
    }
}
