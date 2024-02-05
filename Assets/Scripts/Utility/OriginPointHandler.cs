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
    private new Camera camera;

    void Start()
    {
        target.gameObject.GetComponent<BaseActorBattler>().originPointInUI = this;
        rtransform = GetComponent<RectTransform>();
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        var targetpoint = camera.WorldToScreenPoint(target.transform.position + Vector3.up * 1.5f);
        rtransform.position = targetpoint;

        //if (rtransform.localPosition.x < -Screen.width / 2 + padding) rtransform.localPosition = new Vector3(-Screen.width / 2 + padding, rtransform.localPosition.y);
        //if (rtransform.localPosition.x >  Screen.width / 2 - padding) rtransform.localPosition = new Vector3(Screen.width / 2 - padding, rtransform.localPosition.y);
        //if (rtransform.localPosition.y >  Screen.height / 2 - padding) rtransform.localPosition = new Vector3(rtransform.localPosition.x, Screen.height / 2 - padding);
        //if (rtransform.localPosition.y < -Screen.height / 2 + padding) rtransform.localPosition = new Vector3(rtransform.localPosition.x, -Screen.height / 2 + padding);
    }
}
