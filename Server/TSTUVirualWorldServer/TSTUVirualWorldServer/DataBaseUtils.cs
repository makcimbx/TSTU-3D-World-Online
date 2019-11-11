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
        private Form1 form;

        public DataBaseUtils(TSTUDataBaseDataSetTableAdapters.UsersTableAdapter usersTableAdapter, Form1 form)
        {
            this.form = form;
            this.usersTableAdapter = usersTableAdapter;
        }

        public bool CheckLoginAccess(string login, string password)
        {
            var usersDataTable = usersTableAdapter.GetData();

            bool loginAccess = false;
            foreach (DataRow row in usersDataTable.Rows)
            {
                if(string.Equals(row["Login"].ToString(), login) && string.Equals(row["Password"].ToString(), password))
                {
                    loginAccess = true;
                    break;
                }
            }

            return loginAccess;
        }

        public bool AddNewRegistration(string login, string password)
        {
            if (CheckLoginExists(login)) return false;

            usersTableAdapter.Insert(GetMaxID() + 1, login, password);

            return true;
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
