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
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        actorData = actor.GetBaseActor();
        hpBar.value = actorData.GetCurrentHP() / actorData.baseHP;
        postureBar.value = actorData.GetCurrentPosture() / actorData.basePosture;

    }
}
