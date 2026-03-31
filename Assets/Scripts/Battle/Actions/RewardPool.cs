using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "RewardPool", menuName = "ScriptableObjects/Action/RewardPool")]
    public class RewardPool : ScriptableObject
    {
        public List<BaseAction> Actions;
    }

}