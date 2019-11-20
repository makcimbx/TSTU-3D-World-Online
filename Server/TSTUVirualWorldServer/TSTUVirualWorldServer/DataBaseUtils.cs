using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TSTUVirualWorldServer
{
    class DataBaseUtils
    {
        private TSTUDataBaseDataSetTableAdapters.UsersTableAdapter usersTableAdapter;
        private TSTUDataBaseDataSetTableAdapters.ItemsTableAdapter itemsTableAdapter;
        private Form1 form;

        public DataBaseUtils(TSTUDataBaseDataSetTableAdapters.UsersTableAdapter usersTableAdapter, TSTUDataBaseDataSetTableAdapters.ItemsTableAdapter itemsTableAdapter, 
            Form1 form)
        {
            this.form = form;
            this.usersTableAdapter = usersTableAdapter;
            this.itemsTableAdapter = itemsTableAdapter;
        }

        public int CheckLoginAccess(string login, string password)
        {
            var usersDataTable = usersTableAdapter.GetData();

            int userId = -1;
            foreach (DataRow row in usersDataTable.Rows)
            {
                if(string.Equals(row["Login"].ToString(), login) && string.Equals(row["Password"].ToString(), password))
                {
                    userId = (int)row["Id"];
                    break;
                }
            }

            return userId;
        }

        public bool AddNewRegistration(string login, string password)
        {
            if (CheckLoginExists(login)) return false;

            usersTableAdapter.Insert(GetMaxID() + 1, login, password);

            return true;
        }

        public Dealer GetDealerFromId(int dealerId)
        {
            var itemsDataTable = itemsTableAdapter.GetData();
            Dealer dealer = new Dealer(dealerId);
            dealer.inventory = new List<Entity>();
            foreach (DataRow row in itemsDataTable.Rows)
            {
                if ((int)row["DealerId"] == dealerId)
                {
                    var item = new Entity(-1, (int)row["Id"]);
                    item.price = (int)row["Price"];
                    dealer.inventory.Add(item);
                }
            }

            return dealer;
        }

        public int GetItemPriceFromId(int itemId)
        {
            var itemsDataTable = itemsTableAdapter.GetData();
            
            foreach (DataRow row in itemsDataTable.Rows)
            {
                if ((int)row["Id"] == itemId)
                {
                    return (int)row["Price"];
                }
            }

            return 0;
        }

        private bool CheckLoginExists(string login)
        {
            var usersDataTable = usersTableAdapter.GetData();

            bool loginExists = false;
            foreach (DataRow row in usersDataTable.Rows)
            {
                if (string.Equals(row["Login"].ToString(), login))
                {
                    loginExists = true;
                    break;
                }
            }

            return loginExists;
        }

        private int GetMaxID()
        {
            var usersDataTable = usersTableAdapter.GetData();

            int maxId = 1;
            foreach (DataRow row in usersDataTable.Rows)
            {
                if ((int)row["Id"] > maxId) maxId = (int)row["Id"];
            }

            return maxId;
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
