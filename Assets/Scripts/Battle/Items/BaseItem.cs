using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Battle.Items
{
    [CreateAssetMenu(fileName = "Material", menuName = "ScriptableObjects/Item/Material")]
    public class BaseItem : ScriptableObject
    {
        [Header("Item Info")]
        public string ItemName;
        public string Description;
        public Sprite Icon;

        public bool stackable;
    }
}
