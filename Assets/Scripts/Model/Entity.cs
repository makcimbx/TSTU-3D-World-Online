﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSTU.Model
{
    public class Entity
    {
        public long eId;
        public int itemId;
        public float posX;
        public float posY;
        public float posZ;
        public int price;

        public Entity(long eId, int itemId)
        {
            this.eId = eId;
            this.itemId = itemId;
        }
    }
}