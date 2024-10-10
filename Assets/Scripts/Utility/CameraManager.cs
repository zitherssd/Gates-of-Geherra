using System.Collections;
using UnityEngine;

namespace Assets
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager instance = null;
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0.7f;
        private float dampingSpeed = 1.0f;
        private float CameraHeight;

        public CameraType cameraType = CameraType.Main;

        [SerializeField]
        [Range(-1, 15)]
        private float UpDistance;
        [Range(1, 15)]
        [SerializeField]
        private float BackDistance;
        private new Camera camera;
        public bool Override = false;
        public bool SlowTrack = false;

        public Transform leftObj;
        public Transform rightObj;


        Vector3 distvector;
        Vector3 directionvector;
        Vector3 blue;
        Vector3 newposition;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        // Use this for initialization
        void Start()
        {
            leftObj = BattleManager.instance.PlayerActors[0].transform;
            rightObj = BattleManager.instance.EnemyActors[0].transform;
            camera = gameObject.GetComponent<Camera>();

        }

        public void ResetForNewBattle()
        {
            transform.position = new Vector3(3.68f, 2.4f, -5.17f);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 targetpos;

            var leftobjpoint = camera.WorldToScreenPoint(leftObj.position);
            var rightobpoint = camera.WorldToScreenPoint(rightObj.position);
            if(leftobjpoint.x > rightobpoint.x)
            {
               Switch();
            }

            distvector = (rightObj.position + leftObj.position) / 2; //start point
            distvector = new Vector3(distvector.x, 0, distvector.z);
            directionvector = (rightObj.position - leftObj.position) / 2;
            directionvector = new Vector3(directionvector.x, 0, directionvector.z);

            if(!Override)
            {
                var input = Mathf.Clamp((directionvector * 2).magnitude, 3, 40);
                UpDistance = LinearMap(input, 3, 40, 1.5f, 6);
                BackDistance = LinearMap(input, 3, 40, 4, 20);
            }

            directionvector = Vector3.ProjectOnPlane(directionvector, Vector3.up).normalized;
            if(!SlowTrack)
            blue = Vector3.Cross(directionvector, Vector3.up).normalized;
            newposition = distvector + Vector3.up * UpDistance + -blue * BackDistance;

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
                if (!SlowTrack)
                //if (Vector3.Distance(transform.position, targetpos) > 3f)
                  //  {
                        //transform.position = Vector3.MoveTowards(transform.position, targetpos, 4f * Time.unscaledDeltaTime);
                   // }
                //else
                  //  {
                  transform.position = Vector3.Lerp(transform.position, targetpos, 0.03f);
                  //  }
                else
                    transform.position = Vector3.MoveTowards(transform.position, targetpos, 0.1f * Time.unscaledDeltaTime);


            }
            else transform.position = targetpos;
            if(!SlowTrack)
            transform.LookAt(distvector + Vector3.up * 0.5f);
            else
            {
                Vector3 targetRotation = (distvector + Vector3.up * 0.5f) - transform.position;
                Quaternion endRotation = Quaternion.LookRotation(targetRotation);

                // Smoothly rotate towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, 0.1f);
            }

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

        public void Switch()
        {
            var aux = rightObj;
            rightObj = leftObj;
            leftObj = aux;
            var leftSObj = leftObj.Find("Billboard");
            var rightSObj = rightObj.Find("Billboard");


            leftSObj.transform.localScale = new Vector3(1, 1, 1);
            rightSObj.transform.localScale = new Vector3(-1, 1, 1);
        }


        public enum CameraType
        {
            Main, UI
        }

        public void SetCameraParameters(float distance, out float upDistance, out float backDistance, out float fov)
        {
            // Define the key points and corresponding values for upDistance, backDistance, and fov
            float[] distances = { 0.8f, 3f, 5f, 10f };
            float[] upDistances = { 1f, 1.4f, 2.2f, 3.4f };
            float[] backDistances = { 3f, 3.5f, 4.5f, 6f };
            float[] fovs = { 45f, 45f, 50f, 60f };

            // If distance is below the first point, clamp to the first values
            if (distance <= distances[0])
            {
                upDistance = upDistances[0];
                backDistance = backDistances[0];
                fov = fovs[0];
                return;
            }

            // If distance is beyond the last point, clamp to the last values
            if (distance >= distances[distances.Length - 1])
            {
                upDistance = upDistances[upDistances.Length - 1];
                backDistance = backDistances[backDistances.Length - 1];
                fov = fovs[fovs.Length - 1];
                return;
            }

            // Find the two points between which the current distance lies
            for (int i = 0; i < distances.Length - 1; i++)
            {
                if (distance >= distances[i] && distance <= distances[i + 1])
                {
                    // Interpolate upDistance, backDistance, and fov
                    float t = (distance - distances[i]) / (distances[i + 1] - distances[i]); // Normalized interpolation factor

                    upDistance = Mathf.Lerp(upDistances[i], upDistances[i + 1], t);
                    backDistance = Mathf.Lerp(backDistances[i], backDistances[i + 1], t);
                    fov = Mathf.Lerp(fovs[i], fovs[i + 1], t);

                    return;
                }
            }

            // Default case (should never hit)
            upDistance = upDistances[0];
            backDistance = backDistances[0];
            fov = fovs[0];
        }
    }
}