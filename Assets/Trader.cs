using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public override void Interact()
        {
            base.Interact();           
        }

        
    }
}
