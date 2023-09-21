using System.Collections;
using UnityEngine;

namespace Assets
{
    public class CameraManager : MonoBehaviour
    {
        private Vector3 targetPosition;
        private Vector3 targetRotation;



        private BattleManager battleManager;
        [SerializeField]
        [Range(1, 15)]
        private float UpDistance;
        [Range(1, 15)]
        [SerializeField]
        private float BackDistance;
        public bool Override = false;

        // Use this for initialization
        void Start()
        {
            battleManager = BattleManager.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            var player = battleManager.PlayerActors[0].transform.position;
            var enemy = battleManager.EnemyActors[0].transform.position;

            var distvector = (enemy + player) / 2; //start point
            var directionvector = (enemy - player) / 2;

            if(!Override)
            {
                var input = Mathf.Clamp((directionvector * 2).magnitude, 3, 40);
                UpDistance = LinearMap(input, 3, 40, 1, 6);
                BackDistance = LinearMap(input, 3, 40, 5, 20);
            }

            var blue = Vector3.Cross(directionvector, Vector3.up).normalized;
            var newposition = distvector + Vector3.up * UpDistance + -blue * BackDistance;

            transform.position = newposition;


            transform.LookAt(distvector + Vector3.up * 1.5f);

        }

        float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }
    }
}