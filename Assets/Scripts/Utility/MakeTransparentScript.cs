using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class MakeTransparentScript : MonoBehaviour
    {
        [SerializeField] private List<GameObject> currentlyInTheWay;
        [SerializeField] private List<GameObject> alreadyTransparent;

        private Transform player;
        private Transform camera;


        // Use this for initialization
        void Awake()
        {
            currentlyInTheWay = new List<GameObject>();
            alreadyTransparent = new List<GameObject>();
            player = BattleManager.instance.PlayerActors[0].transform;
            camera = Camera.main.transform;
        }

private void GetAllObjectsInTheWay()
        {

        }
    }
}