using System.Collections.Generic;
namespace TSTU.Controller
{
    public class Trader : Interacteble
    {
        public List<Item> items = new List<Item>();
        private int money = int.MaxValue;
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
