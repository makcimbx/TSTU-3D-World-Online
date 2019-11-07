using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSTUVirualWorldServer
{
    public class GameServer
    {
        private int localPort;
        private Form1 form;

        private UdpClient receiver;

        public GameServer(Form1 form, int localPort)
        {
            this.form = form;
            this.localPort = localPort;
        }

        public void StartListening()
        {
            StopListening();
            receiver = new UdpClient(localPort); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.Unicode.GetString(data);
                    form.richTextBox1.Invoke(new MethodInvoker(() =>
                    {
                        form.richTextBox1.Text += $"Собеседник: {message}\n";
                    }));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        public void StopListening()
        {
            if(receiver != null)
                receiver.Close();
        }

        private void SendMessage(string message ,string remoteAddress, int remotePort)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                sender.Send(data, data.Length, remoteAddress, remotePort); // отправка
            }
            catch (Exception ex)
            {
                form.richTextBox1.Invoke(new MethodInvoker(() =>
                {
                    form.richTextBox1.Text += $"{ex}\n";
                }));
            }
            finally
            {
                sender.Close();
            }
        }
    }
}
