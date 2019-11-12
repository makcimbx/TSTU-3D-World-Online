using UnityEngine;

namespace TSTU.Model
{
    public class Player
    {
        public int Id;

        public Vector3 PositionOnMap;

        public Player(int Id)
        {
            this.Id = Id;
        }
    }
}