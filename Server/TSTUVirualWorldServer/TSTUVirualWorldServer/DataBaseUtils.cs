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

        private void LogMessage(string message)
        {
            form.richTextBox1.Invoke(new MethodInvoker(() =>
            {
                form.richTextBox1.Text += $"{message}\n";
            }));
        }
    }
}
