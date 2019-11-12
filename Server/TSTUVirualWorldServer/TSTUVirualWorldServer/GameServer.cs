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
    public enum ServerState
    {
        Login = 1,
        Registration,
        StartPlayerInfoUpdate,
        PlayerInfoUpdate
    }

    public class GameServer
    {
        private int localPort;
        private Form1 form;
        private DataBaseUtils dataBaseUtils;

        private Dictionary<int, IPEndPoint> usersIdIpList = new Dictionary<int, IPEndPoint>();

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
                    else if(state == 3)
                    {
                        StartPlayerInfoStream(jsonMessage, remoteIp);
                    }
                    else if(state == 4)
                    {
                        UpdatePlayerInfoStream(jsonMessage, remoteIp);
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
            var userId = dataBaseUtils.CheckLoginAccess(jsonNode["login"], jsonNode["password"]);
            LogMessage($"Попытка входа! Результат: {userId != -1}; Отправляем результаты клиенту!");
            JSONObject answer = new JSONObject();
            answer["answer"] = userId != -1;
            answer["user_id"] = userId;
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

        private void StartPlayerInfoStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Начало постоянной трансляции игровых данных! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            if (usersIdIpList.ContainsKey(jsonNode["id"].AsInt))
            {
                LogMessage("Информация уже транслируется! ОТМЕНА!");
                answer["answer"] = false;
            }
            else
            {
                answer["answer"] = true;
            }
            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void UpdatePlayerInfoStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Игрок отправляет свою позицию! Id: {jsonNode["id"]}; PosX: {jsonNode["pos_x"]}; PosY: {jsonNode["pos_y"]}; PosZ: {jsonNode["pos_z"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");
            
            JSONObject answer = new JSONObject();
            answer["answer"] = true;
            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);

            List<int> errorIdList = new List<int>();
            int counter = 0;
            foreach (var item in usersIdIpList)
            {
                if (string.Equals(item.Value.ToString(), remoteIp.Address.ToString())) continue;

                JSONObject message = new JSONObject();
                message["state"] = (int)ServerState.PlayerInfoUpdate;
                message["id"] = item.Key;
                message["pos_x"] = jsonNode["pos_x"];
                message["pos_y"] = jsonNode["pos_y"];
                message["pos_z"] = jsonNode["pos_z"];
                var success = SendMessage(message.ToString(), item.Value.ToString(), remoteIp.Port);

                if(!success)
                {
                    errorIdList.Add(item.Key);
                }
                counter++;
            }

            if(errorIdList.Count > 0)
            {
                LogMessage($"Не можем отправить информацию {errorIdList.Count} клиентам! ОЧИСТКА ИХ!");
                errorIdList.ForEach(item => usersIdIpList.Remove(item));
            }
        }

        private bool SendMessage(string message ,string remoteAddress, int remotePort)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            bool haveError = false;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                sender.Send(data, data.Length, remoteAddress, remotePort); // отправка
            }
            catch (Exception ex)
            {
                haveError = true;

                foreach (var item in usersIdIpList)
                {
                    if (string.Equals(item.Value.Address.ToString(), remoteAddress))
                    {
                        usersIdIpList.Remove(item.Key);
                        break;
                    }
                }

                form.richTextBox1.Invoke(new MethodInvoker(() =>
                {
                    form.richTextBox1.Text += $"{ex}\n";
                }));
            }
            finally
            {
                sender.Close();
            }
            return !haveError;
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
