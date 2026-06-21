using Assets.Scripts.Battle.Manager;
using UnityEngine;

public class DirectionalLightController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    public float directionalLightRotationSpeed = 5f;
    
    // Arena-specific tilt angles
    private float GetLightTiltAngle()
    {
        if (BattleManager.instance == null)
            return 0.5f; // Default tilt
        
        try
        {
            var battleDef = BattleManager.instance.GetCurrentBattleDefinition();
            if (battleDef == null)
                return 0.5f;
            
            // Set tilt angle based on arena
            return battleDef.arena switch
            {
                Arena.Crossing => 77f,
                Arena.Cave => 0.4f,
                _ => 0.5f // Default for other arenas
            };
        }
        catch
        {
            return 0.5f; // Fallback if no battle is active
        }
    }
    
    private void OnEnable()
    {
        // If camera transform not set in inspector, try to find the main camera
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
    }

    private void Update()
    {
        if (cameraTransform == null)
            return;

        float targetY = cameraTransform.eulerAngles.y;
        float currentY = transform.eulerAngles.y;

        float newY = Mathf.LerpAngle(
            currentY,
            targetY,
            Time.unscaledDeltaTime * directionalLightRotationSpeed
        );

        float tiltAngle = GetLightTiltAngle();
        transform.rotation = Quaternion.Euler(tiltAngle, newY, 0f);
    }
}
