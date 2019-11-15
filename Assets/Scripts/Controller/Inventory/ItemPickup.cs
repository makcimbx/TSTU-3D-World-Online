using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TSTU.Controller
{
    public class ItemPickup : Interacteble
    {
        public Item item;

        public override void Interact()
        {
            base.Interact();
            if (isInteracteble)
            {
                Inventory.instance.Add(item);
                Destroy(gameObject);
            }

        }
    }
}