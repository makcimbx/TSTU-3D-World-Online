using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSTUVirualWorldServer
{
    public class Dealer
    {
        public int DealerId;
        public List<Entity> inventory;

        public Dealer(int DealerId)
        {
            this.DealerId = DealerId;
        }
    }
}
