using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Utility;
using Assets.Scripts;
using Assets.Scripts.UI;

public class WorldActionTrigger : MonoBehaviour
{
    [Header("Action Data")]
    public BaseAction worldAction;
    public bool oneTimeUse = false;
    public float activationRange = 4f;

    [Header("UI")]
    public Canvas uiCanvas;
    public CanvasGroup uiCanvasGroup;
    public GameObject actionButton; // The ActionButton prefab (child of the canvas)

    private Actor player;
    private bool isVisible = false;
    private bool uiLocked = false;
    private bool shouldShow = false;
    private ActionButtonHandler handler;

    void Start()
    {
        // Find player
        player = BattleManager.instance.PlayerActors[0];

        // Ensure references
        if (!uiCanvas)
            uiCanvas = GetComponentInChildren<Canvas>(true);
        if (!uiCanvasGroup)
            uiCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (!uiCanvas)
        {
            Debug.LogError($"{name}: No Canvas found under WorldAction!");
            return;
        }

        if (!actionButton)
            actionButton = uiCanvas.GetComponentInChildren<Button>(true)?.gameObject;

        if (!actionButton)
        {
            Debug.LogError($"{name}: No ActionButton found under Canvas!");
            return;
        }
        handler = GetComponentInChildren<ActionButtonHandler>();
        handler.Init(ScriptableObject.Instantiate(worldAction));


        // Hide initially
        uiCanvasGroup.blocksRaycasts = false;

        UIManager.OnHideUI += OnUIHidden;
        UIManager.OnShowUI += OnUIShown;
    }

    void Update()
    {
        if (handler) handler.referencedAction.UpdateCooldown();
        if (player == null || uiCanvas == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if(!uiLocked)
            shouldShow = distance <= activationRange;

        if (shouldShow && !isVisible)
            ShowUI();
        else if (!shouldShow && isVisible)
            HideUI();

        if (isVisible)
        {
            var cam = Camera.main;
            uiCanvas.transform.LookAt(cam.transform);
            uiCanvas.transform.Rotate(0, 180, 0);
        }
    }

    private void ShowUI()
    {
        if (isVisible) return;
        LeanTween.cancel(uiCanvas.gameObject);

        uiCanvasGroup.blocksRaycasts = true;
        uiCanvas.transform.localScale = Vector3.zero;

        LeanTween.scale(uiCanvas.gameObject, Vector3.one * 0.008f, 0.2f)
            .setEaseOutBack()
            .setIgnoreTimeScale(true);

        isVisible = true;
        uiCanvasGroup.interactable = true;
    }

    private void HideUI()
    {
        if (!isVisible) return;
        LeanTween.cancel(uiCanvas.gameObject);

        LeanTween.scale(uiCanvas.gameObject, Vector3.zero, 0.1f)
            .setEaseInBack()
            .setIgnoreTimeScale(true)
            .setOnComplete(() => uiCanvasGroup.blocksRaycasts = false);

        isVisible = false;
        uiCanvasGroup.interactable = false;
    }

    private void OnUIHidden()
    {
        uiLocked = true;
        shouldShow = false;
    }

    private void OnUIShown()
    {
        uiLocked = false;
    }
}
