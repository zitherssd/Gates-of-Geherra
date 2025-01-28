using Assets.Scripts.Battle;
using Assets.Scripts.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBarHandler : MonoBehaviour
{
    public HpBar bar;
    private Slider slider;

    public void Start()
    {
        slider = gameObject.GetComponent<Slider>();
    }

    public void Update()
    {
        slider.value = bar.currentHp / bar.maxHp;
        //hpBarEase.value = Mathf.Lerp(hpBarEase.value, hpBar.value, 0.01f);
        //hpBarText.text = $"{ actorData.currentHp}/{actorData.maxHp}";
    }
}
