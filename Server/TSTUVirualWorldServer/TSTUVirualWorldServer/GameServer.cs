using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleJSON;

namespace TSTUVirualWorldServer
{
    public class GameServer
    {
        private int localPort;
        private Form1 form;
        private DataBaseUtils dataBaseUtils;

        private UdpClient receiver;

        public GameServer(Form1 form, int localPort, TSTUDataBaseDataSetTableAdapters.UsersTableAdapter usersTableAdapter)
        {
            this.form = form;
            this.localPort = localPort;

            dataBaseUtils = new DataBaseUtils(usersTableAdapter, form);
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
                    string message = Encoding.UTF8.GetString(data);
                    var jsonMessage = JSON.Parse(message);
                    var state = jsonMessage["state"].AsInt;
                    if(state == 1)
                    {
                        ReadLoginMessage(jsonMessage, remoteIp);
                    }
                    else if(state == 2)
                    {
                        ReadRegistrationMessage(jsonMessage, remoteIp);
                    }
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

        private void ReadLoginMessage(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Попытка входа! Логин: {jsonNode["login"]}; Пароль: {jsonNode["password"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");
            var loginTry = dataBaseUtils.CheckLoginAccess(jsonNode["login"], jsonNode["password"]);
            LogMessage($"Попытка входа! Результат: {loginTry}; Отправляем результаты клиенту!");
            JSONObject answer = new JSONObject();
            answer["answer"] = loginTry;
            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void ReadRegistrationMessage(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Попытка регистрации! Логин: {jsonNode["login"]}; Пароль: {jsonNode["password"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");
            var registration = dataBaseUtils.AddNewRegistration(jsonNode["login"], jsonNode["password"]);
            LogMessage($"Попытка регистрации! Результат: {registration}; Отправляем результаты клиенту!");
            JSONObject answer = new JSONObject();
            answer["answer"] = registration;
            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void SendMessage(string message ,string remoteAddress, int remotePort)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
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

        private void LogMessage(string message)
        {
            form.richTextBox1.Invoke(new MethodInvoker(() =>
            {
                form.richTextBox1.Text += $"{message}\n";
            }));
        }
    }
}
