using System.Collections;
using UnityEngine;

namespace Assets
{
    public class CameraManager : MonoBehaviour
    {
        private BattleManager battleManager;
        [SerializeField]
        [Range(1, 15)]
        private float UpDistance;
        [Range(1, 15)]
        [SerializeField]
        private float BackDistance;

        // Use this for initialization
        void Start()
        {
            battleManager = BattleManager.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            var player = battleManager.PlayerActors[0].transform.position;
            var enhemy = battleManager.EnemyActors[0].transform.position;
            var distvector = (enhemy + player) / 2; //start point
            var directionvector = (enhemy - player) / 2;
            var blue = Vector3.Cross(directionvector, Vector3.up).normalized;
            var newposition = distvector + Vector3.up * UpDistance + -blue * BackDistance;
            transform.position = newposition;


            transform.LookAt(distvector + Vector3.up * 1.5f);

        }
    }
}