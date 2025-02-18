using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    [Serializable]
    public class HpBar
    {
        private float _currentHp;

        public float maxHp;

        [HideInInspector]
        public float currentHp
        {
            get => _currentHp; set
            {
                _currentHp = value;
                _currentHp = Mathf.Clamp(_currentHp, 0, maxHp);
                if (_currentHp == 0) alive = false;
            }
        }
        [HideInInspector]
        public bool alive;
    }
}