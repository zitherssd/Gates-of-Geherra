using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginPointHandler : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private RectTransform rtransform;
    public float padding = 100f;
    public bool Override;
    private BattleManager bm;

    void Start()
    {
        rtransform = GetComponent<RectTransform>();
        bm = BattleManager.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        if(!Override)
        transform.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 0.5f);

        if (rtransform.localPosition.x < -Screen.width / 2 + padding) rtransform.localPosition = new Vector3(-Screen.width / 2 + padding, rtransform.localPosition.y);
        if (rtransform.localPosition.x >  Screen.width / 2 - padding) rtransform.localPosition = new Vector3(Screen.width / 2 - padding, rtransform.localPosition.y);
        if (rtransform.localPosition.y >  Screen.height / 2 - padding) rtransform.localPosition = new Vector3(rtransform.localPosition.x, Screen.height / 2 - padding);
        if (rtransform.localPosition.y < -Screen.height / 2 + padding) rtransform.localPosition = new Vector3(rtransform.localPosition.x, -Screen.height / 2 + padding);


    }
}
