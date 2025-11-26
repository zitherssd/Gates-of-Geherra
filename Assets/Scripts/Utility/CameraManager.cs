using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager instance = null;
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0.7f;
        private float dampingSpeed = 1.0f;
        private float CameraHeight;

        public CameraType cameraType = CameraType.Main;
        public GameObject directionalLight;
        public float directionalLightRotationSpeed = 5f;

        [SerializeField]
        [Range(-1, 15)]
        private float UpDistance;
        [Range(1, 15)]
        [SerializeField]
        private float BackDistance;
        private new Camera camera;
        public bool Override = false;
        public bool SlowTrack = false;
        private Actor playerActorTarget;
        public Transform playerTransform;
        private Actor playerActor;
        public Transform enemyTransform;


        Vector3 distvector;
        Vector3 directionvector;
        Vector3 blue;
        Vector3 newposition;
        Vector3 rightObjPoint;

        public Vector3 NEWdistvector { get; private set; }

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        // Use this for initialization
        void Start()
        {
            playerTransform = BattleManager.instance.PlayerActors[0].transform;
            playerActor = BattleManager.instance.PlayerActors[0];
            enemyTransform = BattleManager.instance.EnemyActors[0].transform;
            camera = gameObject.GetComponent<Camera>();

        }

        public void ResetForNewBattle()
        {
            transform.position = new Vector3(3.68f, 2.4f, -5.17f);
        }

        void Update()
        {
            Vector3 targetpos;
            Vector3 playerTarget;
            if (playerActor.target.TargetPosition != null)
                playerTarget = playerActor.target.TargetPosition;
            else
                playerTarget = playerTransform.position + playerTransform.forward * 1f;

            distvector = (playerTarget + playerTransform.position) / 2; //start point 
            distvector = new Vector3(distvector.x, 0, distvector.z);
            directionvector = (playerTarget - playerTransform.position) / 2;
            directionvector = new Vector3(directionvector.x, 0, directionvector.z);

            //Debug.Log($"distvector Vector: {directionvector}, directionvector Vector: {directionvector}");

            if (!Override)
            {
                var input = Mathf.Clamp((directionvector * 2).magnitude, 1, 30);
                UpDistance = LinearMap(input, 1, 30, 1.7f, 8);
                BackDistance = LinearMap(input, 1, 30, 3.3f, 20);
            }

            directionvector = Vector3.ProjectOnPlane(directionvector, Vector3.up).normalized;
            if (Vector3.Dot(directionvector, transform.right) < 0)
            {
                // Flip directionvector to align with cameraMain.right
                directionvector = -directionvector;
            }
            //if (!SlowTrack)
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
                    transform.position = Vector3.Lerp(transform.position, targetpos, 0.1f);
                //  }
                else
                {
                    float distance = Vector3.Distance(transform.position, targetpos);
                    float speed = Mathf.Lerp(0.03f, 6f, distance / 10f);
                    // small distance → slow, big distance → fast

                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        targetpos,
                        speed * Time.unscaledDeltaTime
                    );
                }


            }
            else transform.position = targetpos;
            if (!SlowTrack)
                transform.LookAt(distvector + Vector3.up * 0.5f);

            else
            {
                var playertarget = BattleManager.instance.PlayerActors[0].target.target;
                if (playertarget != null)
                {
                    NEWdistvector = (playertarget.transform.position + playerTransform.position) / 2; //start point 
                    NEWdistvector = new Vector3(distvector.x, 0, distvector.z);
                }
                else
                {
                    NEWdistvector = (playerTransform.position + playerTransform.forward);
                    NEWdistvector = new Vector3(distvector.x, 0, distvector.z);
                }
                Vector3 targetRotation = (NEWdistvector + Vector3.up * 0.5f) - transform.position;
                Quaternion endRotation = Quaternion.LookRotation(targetRotation);

                // Smoothly rotate towards the target rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, endRotation, 0.1f);
            }
            if (directionalLight)
            {
                float targetY = transform.eulerAngles.y;
                float currentY = directionalLight.transform.eulerAngles.y;

                float newY = Mathf.LerpAngle(
                    currentY,
                    targetY,
                    Time.unscaledDeltaTime * directionalLightRotationSpeed
                );

                directionalLight.transform.rotation = Quaternion.Euler(0f, newY, 0f);
            }

        }
    void OnGUI()
        {
            if (camera != null)
            {
                // Convert world points to screen points
                Vector3 leftScreenPoint = camera.WorldToScreenPoint(playerTransform.position);
                Vector3 rightScreenPoint = camera.WorldToScreenPoint(rightObjPoint);
                // Vector3 playertargetPoint = camera.WorldToScreenPoint(playerActor.target.target.transform.position);

                // Screen space adjustment (invert y-axis for GUI coordinates)
                leftScreenPoint.y = Screen.height - leftScreenPoint.y;
                rightScreenPoint.y = Screen.height - rightScreenPoint.y;
                //playertargetPoint.y = Screen.height - playertargetPoint.y;

                // Draw green dot for leftobjpoint
                GUI.color = Color.green;
                GUI.DrawTexture(new Rect(leftScreenPoint.x - .2f, leftScreenPoint.y - .2f, .2f, .4f), Texture2D.whiteTexture);

                // Draw blue dot for rightobpoint
                GUI.color = Color.blue;
                GUI.DrawTexture(new Rect(rightScreenPoint.x - .2f, rightScreenPoint.y - .2f, .2f, .4f), Texture2D.whiteTexture);

                // Draw RED dot for PLAYERTARGETPOINT
                GUI.color = Color.red;
                //GUI.DrawTexture(new Rect(playertargetPoint.x - 0.1f, playertargetPoint.y - 0.1f, 0.2f, 0.2f), Texture2D.whiteTexture);
            }
        }

        float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
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