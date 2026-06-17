using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsKiller : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OutOfBoundsKiller hit: " + collision.gameObject.name);
        var actor = collision.gameObject.GetComponent<Actor>();
        if (actor != null)
        {
            var damageInstance = new DamageInstance
            {
                Damage = 999,
                PostureDamage = 0,
            };
            actor.ApplyDamageInstance(damageInstance, actor, null);
        }
    }
}
