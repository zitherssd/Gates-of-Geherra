using System.Collections;
using UnityEngine;

namespace Assets
{
    public class CameraManager : MonoBehaviour
    {
        private Vector3 targetPosition;
        private Vector3 targetRotation;
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0.7f;
        private float dampingSpeed = 1.0f;


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
            Vector3 targetpos;
            var player = battleManager.PlayerActors[0].transform.position;
            var enemy = battleManager.EnemyActors[0].transform.position;

            var distvector = (enemy + player) / 2; //start point
            distvector = new Vector3(distvector.x, 0, distvector.z);
            var directionvector = (enemy - player) / 2;
            directionvector = new Vector3(directionvector.x, 0, directionvector.z);

            if(!Override)
            {
                var input = Mathf.Clamp((directionvector * 2).magnitude, 3, 40);
                UpDistance = LinearMap(input, 3, 40, 1, 6);
                BackDistance = LinearMap(input, 3, 40, 5, 20);
            }

            var blue = Vector3.Cross(directionvector, Vector3.up).normalized;
            var newposition = distvector + Vector3.up * UpDistance + -blue * BackDistance;

            if (shakeDuration > 0)
            {
                targetpos = newposition + Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                targetpos = newposition;
            }

            transform.position = Vector3.Lerp(transform.position, targetpos, 0.1f);
            transform.LookAt(distvector + Vector3.up * 1.5f);

        }

        float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
        }

        public void TriggerShake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }
    }
}