using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TSTU.Controller
{
    public class Trader : Interacteble
    {
        public int id = -1;
        public Item[] Items {
            get;
            private set;
        }
        

        private int money = int.MaxValue;




        public async Task UpdateInventory()
        {                         
            var a = await GameController.Instance.GameServer.GetDealerInventory(id);
            var list = new List<Item>();
            foreach (var item in a.inventory)
            {
                list.Add(Inventory.GetItem(item));
            }
            Debug.Log($"list Trader {list.Count}");
            Items = list.ToArray();
        } 

        public int Money {
            get
            {
                return money;
            }
            private set { }
        }

        public override void Interact(PlayerController player)
        {
            base.Interact(player);

            player.StartTrading(this);
        }

       
    }
}
