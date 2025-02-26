using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private BillboardType billboardType;

        private Quaternion shurikenRotation = Quaternion.Euler(50f, 0, 0);
        private new Camera camera;
        private GameObject graphicObj;
        private float prevrotation;
        private Actor actor;

        public void Start()
        {
            camera = Camera.main;
            if (billboardType == BillboardType.Shuriken) return;
            graphicObj = transform.GetChild(0).gameObject;
            if(gameObject.name == "Billboard")
                actor = gameObject.GetComponentInParent<Actor>();
        }

        public enum BillboardType { LookAtCamera, CameraForward,
            Shuriken
        }

        private void Update()
        {
            UpdateOrientation();

        }

        private void UpdateOrientation()
        {
            if (actor != null)
            {
                Vector3 camerRight = Camera.main.transform.right;
                float dotProduct = Vector3.Dot(actor.transform.forward, camerRight.normalized);
                if (dotProduct > 0f)
                    transform.localScale = new Vector3(1, 1, 1);
                else
                    transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        void LateUpdate()
        {
            switch(billboardType)
            {
                case BillboardType.LookAtCamera:
                    transform.LookAt(camera.transform.position, Vector3.up);
                    break;
                case BillboardType.CameraForward:
                    transform.forward = camera.transform.forward;
                    break;
                case BillboardType.Shuriken:
                    // Rotate the object around its own up axis (Y-axis) over time
                    float rotationSpeed = 30f; // Adjust this value as needed
                    transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

                    // Align the object with the camera's forward direction
                    transform.forward = camera.transform.forward;
                    transform.rotation *= shurikenRotation;
                    break;
                default:
                    break;
            }
        }
        private void ApplyCameraFacingRotation()
        {
            if (actor != null)
            {
                // Calculate the dot product between actor's forward and camera's forward
                var cameraforwardflat = new Vector3(0, camera.transform.forward.y, camera.transform.forward.z);
                float dot = Vector3.Dot(actor.transform.forward, cameraforwardflat);

                if (dot is > -0.5f and < 0.5f)
                {
                    return;
                }

                // Map dot product (-1 to 1) to rotation range (-45 to 45)
                float rotationAmount = Mathf.Lerp(45f, -45f, (dot + 1) / 2f);

                // Adjust rotation direction based on local scale
                if (transform.localScale.x < 0)
                {
                    rotationAmount = -rotationAmount;
                }

                // Apply rotation
                transform.Rotate(0, rotationAmount, 0);

                // Convert current rotation to Euler angles
                Vector3 eulerRotation = transform.eulerAngles;

                // Lerp the Z rotation towards 0 based on the dot product
                eulerRotation.z = Mathf.LerpAngle(eulerRotation.z, 0f, (dot + 1) / 2f);

                // Apply the modified rotation
                transform.rotation = Quaternion.Euler(eulerRotation);
            }
        }
    }
}
