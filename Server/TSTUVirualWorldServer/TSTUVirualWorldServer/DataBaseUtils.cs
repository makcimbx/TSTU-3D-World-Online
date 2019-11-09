using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSTUVirualWorldServer
{
    class DataBaseUtils
    {
        public bool CheckLoginAccess(string login, string password)
        {
            return (login == "keliz" || login == "makcimbx") && (password == "123");
        }
    }
}
