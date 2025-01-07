using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    [CreateAssetMenu(fileName = "HpBar", menuName = "ScriptableObjects/HpBar", order = 2)]
    public class HpBar : ScriptableObject
    {
        public float _currentHp;


        public string Name;
        public float maxHp;

        public float currentHp
        {
            get => _currentHp; set
            {
                _currentHp = value;
                _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
                if (_currentHp == 0) alive = false;
            }
        } 
        
        public bool alive;
    }
}