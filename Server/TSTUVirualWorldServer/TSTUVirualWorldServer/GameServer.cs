using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

        private Dictionary<Player, IPEndPoint> usersIdIpList = new Dictionary<Player, IPEndPoint>();
        private Dictionary<Player, System.Timers.Timer> usersTimeoutList = new Dictionary<Player, System.Timers.Timer>();

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
                LogMessage(ex.Message);
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
            if (new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == jsonNode["id"].AsInt) != null)
            {
                LogMessage("Информация уже транслируется! ОТМЕНА!");
                answer["answer"] = false;
            }
            else
            {
                usersIdIpList.Add(new Player(jsonNode["id"].AsInt), remoteIp);

                //
                form.listBox1.Invoke(new MethodInvoker(() =>
                {
                    form.listBox1.Items.Add(jsonNode["id"].AsInt);
                }));
                //

                answer["answer"] = true;
            }
            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void UpdatePlayerInfoStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            //LogMessage($"Игрок отправляет свою позицию! Id: {jsonNode["id"]}; PosX: {jsonNode["pos_x"]}; PosY: {jsonNode["pos_y"]}; PosZ: {jsonNode["pos_z"]};");
            //LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject message = new JSONObject();
            message["state"] = (int)ServerState.PlayerInfoUpdate;
            Player player = new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == jsonNode["id"].AsInt);
            if (player == null)
            {
                LogMessage($"Информация о игроке {jsonNode["id"].AsInt} не найдена!");
                message["answer"] = false;
            }
            else
            {
                message["answer"] = true;

                player.posX = jsonNode["pos_x"].AsFloat;
                player.posY = jsonNode["pos_y"].AsFloat;
                player.posZ = jsonNode["pos_z"].AsFloat;

                // TimeOut

                System.Timers.Timer beforeTimer = null;
                Player beforePlayer = null;
                foreach (var item in usersTimeoutList)
                {
                    if(item.Key.Id == player.Id)
                    {
                        beforeTimer = item.Value;
                        beforePlayer = item.Key;
                        break;
                    }
                }
                if(beforeTimer != null)
                {
                    beforeTimer.Stop();
                    usersTimeoutList.Remove(beforePlayer);
                }

                var _countdownTimer = new System.Timers.Timer(1000);
                _countdownTimer.Elapsed += ((x,y) => RemovePlayer(player));
                _countdownTimer.Start();
                usersTimeoutList.Add(player, _countdownTimer);

                //

                ////

                //int selectedId = -1;
                //form.listBox1.Invoke(new MethodInvoker(() =>
                //{
                //    if(form.listBox1.SelectedItem != null)
                //        selectedId = (int)form.listBox1.SelectedItem;
                //}));

                //if (selectedId != -1)
                //{
                //    Player selectedP = new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == selectedId);
                //    if (selectedP == null)
                //    {
                //        form.listBox1.Invoke(new MethodInvoker(() =>
                //        {
                //            form.listBox1.Items.Remove(selectedId);
                //        }));
                //    }
                //    else
                //    {
                //        form.richTextBox1.Invoke(new MethodInvoker(() =>
                //        {
                //            form.richTextBox2.Text = $"PosX: {selectedP.posX}\n PosY: {selectedP.posY}\n PosY: {selectedP.posZ}\n";
                //        }));
                //    }
                //}

                ////

                int counter = 0;
                foreach (var item in usersIdIpList)
                {
                    if (item.Key.Id == jsonNode["id"].AsInt) continue;

                    message["player_massive"][counter]["id"] = item.Key.Id;
                    message["player_massive"][counter]["pos_x"] = item.Key.posX;
                    message["player_massive"][counter]["pos_y"] = item.Key.posY;
                    message["player_massive"][counter]["pos_z"] = item.Key.posZ;
                    counter++;
                }
                //LogMessage($"Отправляем информацию о {counter} игроках!");
            }
            
            SendMessage(message.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
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
                LogMessage(ex.Message);
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

        private void RemovePlayer(Player player)
        {
            Player playerToRemove = null;
            foreach (var item in usersIdIpList)
            {
                if(item.Key.Id == player.Id)
                {
                    playerToRemove = item.Key;
                    break;
                }
            }
            if (playerToRemove != null)
            {
                form.listBox1.Invoke(new MethodInvoker(() =>
                {
                    form.listBox1.Items.Remove(playerToRemove.Id);
                }));
                usersIdIpList.Remove(playerToRemove);
            }
        }
    }
}
