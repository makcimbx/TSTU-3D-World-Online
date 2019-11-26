using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        PlayerInfoUpdate,
        UpdatePlayerModels,
        AddEntityInInventory,
        DropEntityFromInventory,
        UpdateWorldEntityPositions,
        GetDealerInventory,
        AddMessageToChat,
        GetMessagesFromChat
    }

    public class GameServer
    {
        private const int StartMoneyCount = 1000;

        private int localPort;
        private Form1 form;
        private DataBaseUtils dataBaseUtils;

        private Dictionary<Player, IPEndPoint> usersIdIpList = new Dictionary<Player, IPEndPoint>();
        private Dictionary<Player, System.Timers.Timer> usersTimeoutList = new Dictionary<Player, System.Timers.Timer>();
        private List<Entity> worldMapEntityList = new List<Entity>();
        private List<string> chatMessagesList = new List<string>();

        private UdpClient receiver;

        private long itemEIdCounter;

        public GameServer(Form1 form, int localPort, TSTUDataBaseDataSetTableAdapters.UsersTableAdapter usersTableAdapter,
            TSTUDataBaseDataSetTableAdapters.ItemsTableAdapter itemsTableAdapter)
        {
            this.form = form;
            this.localPort = localPort;

            dataBaseUtils = new DataBaseUtils(usersTableAdapter, itemsTableAdapter, form);
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
                    string message = StringCompressor.Unzip(data); //Encoding.UTF8.GetString(data);
                    var jsonMessage = JSON.Parse(message);
                    var state = jsonMessage["state"].AsInt;
                    if(state == (int)ServerState.Login)
                    {
                        ReadLoginMessage(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.Registration)
                    {
                        ReadRegistrationMessage(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.StartPlayerInfoUpdate)
                    {
                        StartPlayerInfoStream(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.PlayerInfoUpdate)
                    {
                        UpdatePlayerInfoStream(jsonMessage, remoteIp);
                    }
                    else if (state == (int)ServerState.UpdatePlayerModels)
                    {
                        UpdatePlayerModelsStream(jsonMessage, remoteIp);
                    }
                    else if (state == (int)ServerState.AddEntityInInventory)
                    {
                        AddEntityInInventoryStream(jsonMessage, remoteIp);
                    }
                    else if (state == (int)ServerState.DropEntityFromInventory)
                    {
                        DropEntityFromInventoryStream(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.UpdateWorldEntityPositions)
                    {
                        UpdateWorldEntityPositionsStream(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.GetDealerInventory)
                    {
                        GetDealerInventoryStream(jsonMessage, remoteIp);
                    }
                    else if(state == (int)ServerState.AddMessageToChat) 
                    {

                    }
                    else if (state == (int)ServerState.GetMessagesFromChat)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
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
                Player player = new Player(jsonNode["id"].AsInt);
                player.playerModel = jsonNode["model"].GetStringOrDefault(string.Empty);
                player.modelMD5Hash = GetMd5Hash(MD5.Create(), player.playerModel);
                player.inventory = new Dictionary<int, Entity>();
                player.Money = StartMoneyCount;
                usersIdIpList.Add(player, remoteIp);

                //
                form.listBox1.Invoke(new MethodInvoker(() =>
                {
                    form.listBox1.Items.Add(jsonNode["id"].AsInt);
                }));
                //

                answer["answer"] = true;
                answer["money"] = player.Money;
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

                var entityList = jsonNode["entity_list"].AsArray;
                for (int i = 0; i < entityList.Count; i++)
                {
                    var eId = entityList["entity_id"].AsInt;
                    var item = worldMapEntityList.Find(entity => entity.eId == eId);
                    item.posX = entityList["pos_x"].AsFloat;
                    item.posY = entityList["pos_y"].AsFloat;
                    item.posZ = entityList["pos_z"].AsFloat;
                }

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
                    message["player_massive"][counter]["model_hash"] = item.Key.modelMD5Hash;
                    counter++;
                }

                counter = 0;
                foreach (var item in worldMapEntityList)
                {
                    message["entity_list"][counter]["entity_id"] = item.eId;
                    message["entity_list"][counter]["item_id"] = item.itemId;
                    message["entity_list"][counter]["pos_x"] = item.posX;
                    message["entity_list"][counter]["pos_y"] = item.posY;
                    message["entity_list"][counter]["pos_z"] = item.posZ;
                    counter++;
                }
                //LogMessage($"Отправляем информацию о {counter} игроках!");
            }
            
            SendMessage(message.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void UpdatePlayerModelsStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос об обновлении моделей! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var playerList = jsonNode["player_list"].AsArray;
            for (int i = 0; i < playerList.Count; i++)
            {
                int id = playerList[i]["id"].AsInt;
                Player player = new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == id);
                if(player == null)
                {
                    answer["answer"] = false;
                    break;
                }
                answer["player_list"][i]["id"] = player.Id;
                answer["player_list"][i]["model"] = player.playerModel;
            }

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void AddEntityInInventoryStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос о добавлении предмета в инвентарь! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var playerId = jsonNode["id"].AsInt;
            var itemId = jsonNode["item_id"].AsInt;
            var eid = jsonNode["entity_id"].AsInt;
            var price = dataBaseUtils.GetItemPriceFromId(itemId);
            var placeNumber = jsonNode["place_number"].AsInt;

            var player = new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == playerId);
            if (eid == -1) // Покупка у NPC
            {
                itemEIdCounter++;
                player.inventory.Add(placeNumber, new Entity(itemEIdCounter, itemId));
                player.Money -= price;
            }
            else // Подбираем из мира
            {
                var entity = worldMapEntityList.Find(item => item.eId == eid);
                worldMapEntityList.Remove(entity);
                player.inventory.Add(placeNumber, entity);
            }

            int counter = 0;
            foreach (var item in player.inventory)
            {
                answer["inventory"][counter]["place_number"] = item.Key;
                answer["inventory"][counter]["entity_id"] = item.Value.eId;
                answer["inventory"][counter]["item_id"] = item.Value.itemId;
                counter++;
            }
            answer["money"] = player.Money;

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void DropEntityFromInventoryStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос о дропе предмета из инвентаря! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var playerId = jsonNode["id"].AsInt;
            var itemId = jsonNode["item_id"].AsInt;
            var eid = jsonNode["entity_id"].AsInt;
            var npcSell = jsonNode["npc_sell"].AsBool;
            var price = dataBaseUtils.GetItemPriceFromId(itemId);
            var posX = jsonNode["pos_x"].AsFloat;
            var posY = jsonNode["pos_y"].AsFloat;
            var posZ = jsonNode["pos_z"].AsFloat;

            var player = new List<Player>(usersIdIpList.Keys.ToArray()).Find(item => item.Id == playerId);
            if (npcSell) // Продажа у NPC
            {
                var element = player.FindElementByEId(eid);
                player.Money += price;
                player.inventory.Remove(element.Key);
            }
            else // Выбрасываем в мир
            {
                var element = player.FindElementByEId(eid);
                player.inventory.Remove(element.Key);

                var entity = element.Value;
                entity.posX = posX;
                entity.posY = posY;
                entity.posZ = posZ;
                worldMapEntityList.Add(entity);
            }

            int counter = 0;
            foreach (var item in player.inventory)
            {
                answer["inventory"][counter]["place_number"] = item.Key;
                answer["inventory"][counter]["entity_id"] = item.Value.eId;
                answer["inventory"][counter]["item_id"] = item.Value.itemId;
                counter++;
            }
            answer["money"] = player.Money;

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void UpdateWorldEntityPositionsStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            //LogMessage($"Запрос о дропе предмета из инвентаря! Id: {jsonNode["id"]};");
            //LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var playerId = jsonNode["id"].AsInt;
            var updateEntityList = jsonNode["entity_list"].AsArray;
            for (int i = 0; i < updateEntityList.Count; i++)
            {
                var eid = updateEntityList[i]["entity_id"].AsInt;
                var posX = updateEntityList[i]["pos_x"].AsFloat;
                var posY = updateEntityList[i]["pos_y"].AsFloat;
                var posZ = updateEntityList[i]["pos_z"].AsFloat;
                var entity = worldMapEntityList.Find(item => item.eId == eid);
                entity.posX = posX;
                entity.posY = posY;
                entity.posZ = posZ;
            }

            int counter = 0;
            foreach (var item in worldMapEntityList)
            {
                answer["entity_list"][counter]["entity_id"] = item.eId;
                answer["entity_list"][counter]["item_id"] = item.itemId;
                answer["entity_list"][counter]["pos_x"] = item.posX;
                answer["entity_list"][counter]["pos_y"] = item.posY;
                answer["entity_list"][counter]["pos_z"] = item.posZ;
                counter++;
            }

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void GetDealerInventoryStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос инвентаря торговца! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var playerId = jsonNode["id"].AsInt;
            var dealerId = jsonNode["dealer_id"].AsInt;

            Dealer dealer = dataBaseUtils.GetDealerFromId(dealerId);
            answer["dealer_id"] = dealer.DealerId;
            int counter = 0;
            foreach (var item in dealer.inventory)
            {
                answer["dealer_inventory"][counter]["price"] = item.price;
                answer["dealer_inventory"][counter]["item_id"] = item.itemId;
                counter++;
            }

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void AddMessageToChatStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос на добавление сообщения в чат! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject();
            answer["answer"] = true;

            var message = jsonNode["message"].GetStringOrDefault(string.Empty);
            chatMessagesList.Add(message);

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private void GetMessagesFromChatStream(JSONNode jsonNode, IPEndPoint remoteIp)
        {
            LogMessage($"Запрос на получение сообщений из чата! Id: {jsonNode["id"]};");
            LogMessage($"Информация о клиенте! IPAdress: {remoteIp.Address}; Port: {remoteIp.Port};");

            JSONObject answer = new JSONObject(); 

            var messageCount = jsonNode["message_count"].AsInt;
            int counter = 0;
            for (int i = 0; i < messageCount; i++)
            {
                if (chatMessagesList.Count - i - 1 < 0) break;
                answer["messages"][i] = chatMessagesList[chatMessagesList.Count - i - 1];
                counter++;
            }
            answer["answer"] = counter > 0;

            SendMessage(answer.ToString(), remoteIp.Address.ToString(), remoteIp.Port);
        }

        private bool SendMessage(string message ,string remoteAddress, int remotePort)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            bool haveError = false;
            try
            {
                byte[] data = StringCompressor.Zip(message); //Encoding.UTF8.GetBytes(message);
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

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
