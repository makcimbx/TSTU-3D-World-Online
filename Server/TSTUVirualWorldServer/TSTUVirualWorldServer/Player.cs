using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSTUVirualWorldServer
{
    public class Player
    {
        public int Id;

        public float posX;
        public float posY;
        public float posZ;

        public string playerModel;
        public string modelMD5Hash;

        public Player(int Id)
        {
            this.Id = Id;
        }
    }
}
