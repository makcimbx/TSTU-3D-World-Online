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

        public long Money;

        public Dictionary<int, Entity> inventory;

        public Player(int Id)
        {
            this.Id = Id;
        }

        public KeyValuePair<int,Entity> FindElementByEId(long eid)
        {
            foreach (var item in inventory)
            {
                if(item.Value.eId == eid)
                {
                    return item;
                }
            }
            return new KeyValuePair<int, Entity>(-1, null);
        }
    }
}
