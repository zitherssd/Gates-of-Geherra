using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        var velocityThreshold = 3f;
        if (collision.gameObject.CompareTag("Level"))
        {
            gameObject.GetComponentInChildren<BaseActorBattler>().MoveFromKnockback();
        }
    }
}
