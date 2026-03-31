using System.Collections.Generic;
using Assets.Scripts.Battle.Items;
using UnityEngine;

namespace Assets.Scripts.Battle.Manager
{
    [CreateAssetMenu(fileName = "RewardItemsPool", menuName = "ScriptableObjects/Item/RewardItemPool")]
    public class RewardItemsPool : ScriptableObject
    {
        public List<BaseItem> RewardItems;
        [Range(0, 1)]
        public float ChanceToReward = 1f;
    }

}
