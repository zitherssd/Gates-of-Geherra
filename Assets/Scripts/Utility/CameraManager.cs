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

        public CameraType cameraType = CameraType.Main;

        private BattleManager battleManager;
        [SerializeField]
        [Range(-1, 15)]
        private float UpDistance;
        [Range(1, 15)]
        [SerializeField]
        private float BackDistance;
        private new Camera camera;
        public bool Override = false;

        private Transform leftObj;
        private SpriteRenderer leftSObj;
        private Transform rightObj;
        private SpriteRenderer rightSObj;

        // Use this for initialization
        void Start()
        {
            battleManager = BattleManager.instance;
            leftObj = battleManager.PlayerActors[0].transform;
            rightObj = battleManager.EnemyActors[0].transform;
            camera = gameObject.GetComponent<Camera>();

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 targetpos;

            var leftobjpoint = camera.WorldToScreenPoint(leftObj.position);
            var rightobpoint = camera.WorldToScreenPoint(rightObj.position);
            if(leftobjpoint.x > rightobpoint.x)
            {
               //Switch();
            }

            var distvector = (rightObj.position + leftObj.position) / 2; //start point
            distvector = new Vector3(distvector.x, 0, distvector.z);
            var directionvector = (rightObj.position - leftObj.position) / 2;
            directionvector = new Vector3(directionvector.x, 0, directionvector.z);

            if(!Override)
            {
                var input = Mathf.Clamp((directionvector * 2).magnitude, 3, 40);
                UpDistance = LinearMap(input, 3, 40, 2, 6);
                BackDistance = LinearMap(input, 3, 40, 5, 20);
            }

            directionvector = Vector3.ProjectOnPlane(directionvector, Vector3.up).normalized;
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

            if (cameraType == CameraType.Main)
            {
                transform.position = Vector3.Lerp(transform.position, targetpos, 0.1f);
            }
            else transform.position = targetpos;
            transform.LookAt(distvector + Vector3.up * 1.5f);

        }

        private void UpdateOrientation(Transform gameobject)
        {
            Vector3 cameraRight = transform.right;
            float dotProduct = Vector3.Dot(gameobject.transform.forward, cameraRight.normalized);
            if(dotProduct > 0f) gameObject.GetComponentInChildren<SpriteRenderer>().flipX = true;
            else gameObject.GetComponentInChildren<SpriteRenderer>().flipX = false;
        }

        public void LateUpdate()
        {
            
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

        private void Switch()
        {
            var aux = rightObj;
            rightObj = leftObj;
            leftObj = aux;

            var leftSObj = leftObj.GetComponentInChildren<SpriteRenderer>();
            var rightSObj = rightObj.GetComponentInChildren<SpriteRenderer>();

            if (leftSObj.flipX == true) leftSObj.flipX = false;
            else leftSObj.flipX = true;

            if (rightSObj.flipX == true) rightSObj.flipX = false;
            else rightSObj.flipX = true;
        }


        public enum CameraType
        {
            Main, UI
        }
    }
}