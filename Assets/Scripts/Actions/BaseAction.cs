using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    [CreateAssetMenu(fileName = "Action", menuName = "ScriptableObjects/Action", order = 1)]
    
    public class BaseAction : ScriptableObject
    {
        public string Name;
        public RARITY Rarity;
        public int TotalUses;
        //public int Range;
        //public SpriteRenderer sprite;
        public int remainingUses;
    }



    public enum RARITY { COMMON, UNCOMMON, RARE, EPIC, LEGENDARY};
}