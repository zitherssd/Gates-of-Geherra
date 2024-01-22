using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarsHandler : MonoBehaviour
{
    [SerializeField]
    private BaseActorBattler actor;
    private Actor actorData;
    [SerializeField]
    private Slider hpBar;
    [SerializeField]
    private Slider postureBar;
    [SerializeField]
    private Slider buildupBar;
    // Start is called before the first frame update
    void Start()
    {
        actor = gameObject.GetComponentInParent<BaseActorBattler>();
    }

    // Update is called once per frame
    void Update()
    {
        actorData = actor.GetBaseActor();
        hpBar.value = actorData.currentHp / actorData.maxHp;
        postureBar.value = actorData.currentPosture / actorData.maxPosture;
        buildupBar.value = actorData.currentBuildup / actorData.maxBuildup;
    }
}
