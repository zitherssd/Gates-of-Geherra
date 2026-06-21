using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Game
{
    /// <summary>
    /// Sandbox-only debug dropdown that drives scene-authored UI buttons.
    /// </summary>
    public class SandboxDebugMenuController : MonoBehaviour
    {
        private const string SandboxSceneName = "SandboxScene";

        [Header("Scene UI References")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button fightDummyButton;
        [SerializeField] private Button fightOldManButton;
        [SerializeField] private Button fightManyButton;
        [SerializeField] private Button giveAllAbilitiesButton;
        [SerializeField] private Button openReassignMenuButton;

        private BattleDefinition dummyBattle;
        private BattleDefinition oldManBattle;
        private BattleDefinition manyBattle;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != SandboxSceneName)
            {
                Destroy(gameObject);
                return;
            }

            ResolveBattlePresets();
            ResolveUiReferences();
            WireButtons();
            EnsureButtonLabels();

            if (menuPanel != null)
                menuPanel.SetActive(false);
        }

        private void ToggleMenu()
        {
            if (menuPanel == null) return;
            menuPanel.SetActive(!menuPanel.activeSelf);
        }

        private void ResolveUiReferences()
        {
            var root = transform;
            if (toggleButton == null)
                toggleButton = root.Find("DebugMenuToggle")?.GetComponent<Button>();

            if (menuPanel == null)
                menuPanel = root.Find("DebugMenuPanel")?.gameObject;

            var panelTransform = menuPanel != null ? menuPanel.transform : null;
            if (panelTransform != null)
            {
                if (fightDummyButton == null)
                    fightDummyButton = panelTransform.Find("FightDummyButton")?.GetComponent<Button>();
                if (fightOldManButton == null)
                    fightOldManButton = panelTransform.Find("FightOldManButton")?.GetComponent<Button>();
                if (fightManyButton == null)
                    fightManyButton = panelTransform.Find("FightManyButton")?.GetComponent<Button>();
                if (giveAllAbilitiesButton == null)
                    giveAllAbilitiesButton = panelTransform.Find("GiveAllAbilitiesButton")?.GetComponent<Button>();
                if (openReassignMenuButton == null)
                    openReassignMenuButton = panelTransform.Find("OpenReassignMenuButton")?.GetComponent<Button>();
            }
        }

        private void WireButtons()
        {
            WireButton(toggleButton, ToggleMenu);
            WireButton(fightDummyButton, () => StartBattle(dummyBattle));
            WireButton(fightOldManButton, () => StartBattle(oldManBattle));
            WireButton(fightManyButton, () => StartBattle(manyBattle));
            WireButton(giveAllAbilitiesButton, GiveAllAbilities);
            WireButton(openReassignMenuButton, OpenReassignMenu);
        }

        private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void EnsureButtonLabel(Button button, string label)
        {
            if (button == null) return;

            var text = button.GetComponentInChildren<Text>();
            if (text == null)
            {
                var textGo = new GameObject("Label");
                textGo.transform.SetParent(button.transform, false);
                var rect = textGo.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(8f, 4f);
                rect.offsetMax = new Vector2(-8f, -4f);

                text = textGo.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleCenter;
            }

            text.text = label;
        }

        private void EnsureButtonLabels()
        {
            EnsureButtonLabel(toggleButton, "Debug Menu");
            EnsureButtonLabel(fightDummyButton, "Fight dummy");
            EnsureButtonLabel(fightOldManButton, "Fight oldman");
            EnsureButtonLabel(fightManyButton, "Fight many");
            EnsureButtonLabel(giveAllAbilitiesButton, "Give all abilities");
            EnsureButtonLabel(openReassignMenuButton, "Open reassign menu");
        }

        private void ResolveBattlePresets()
        {
            var battles = Resources.LoadAll<BattleDefinition>("Battles").ToList();
            if (battles.Count == 0)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: No BattleDefinition assets found under Resources/Battles.");
                return;
            }

            dummyBattle = battles.FirstOrDefault(b => b != null && b.name.ToLowerInvariant().Contains("sandbox"))
                         ?? battles.FirstOrDefault(b => b != null && b.enemyActors != null && b.enemyActors.Count == 1)
                         ?? battles.FirstOrDefault();

            oldManBattle = battles.FirstOrDefault(b =>
                b != null
                && b.enemyActors != null
                && b.enemyActors.Any(e => e != null && (e.name.ToLowerInvariant().Contains("old") || e.name.ToLowerInvariant().Contains("prisoner"))))
                ?? dummyBattle;

            manyBattle = battles
                .Where(b => b != null && b.enemyActors != null)
                .OrderByDescending(b => b.enemyActors.Count)
                .FirstOrDefault()
                ?? dummyBattle;
        }

        private void StartBattle(BattleDefinition battle)
        {
            if (battle == null)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: Requested battle is null.");
                return;
            }

            if (BattleManager.instance == null)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: No BattleManager available.");
                return;
            }

            BattleManager.instance.Enter(battle, null);
            menuPanel?.SetActive(false);
        }

        private void GiveAllAbilities()
        {
            var player = BattleManager.instance != null ? BattleManager.instance.Player : null;
            if (player == null || player.Runtime == null)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: Cannot grant abilities, player runtime is missing.");
                return;
            }

            var templates = Resources.LoadAll<BaseAction>("Actions/Droptable");
            if (templates == null || templates.Length == 0)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: No ability templates found in Resources/Actions/Droptable.");
                return;
            }

            var existingGuids = new HashSet<string>(player.Runtime.actions.Where(a => a != null).Select(a => a.guid));
            var added = 0;

            foreach (var template in templates)
            {
                if (template == null || existingGuids.Contains(template.guid))
                    continue;

                var clone = ScriptableObject.Instantiate(template);
                player.Runtime.actions.Add(clone);
                existingGuids.Add(template.guid);
                added++;
            }

            if (UIManager.instance != null)
            {
                var currentLayout = GameSession.Instance.GetActiveLoadout();
                UIManager.instance.InitializePlayerActionButtonPrefabs(player.Runtime.actions, currentLayout);
            }

            Debug.Log($"{nameof(SandboxDebugMenuController)}: Added {added} abilities.");
            menuPanel?.SetActive(false);
        }

        private void OpenReassignMenu()
        {
            if (UIManager.instance == null)
            {
                Debug.LogWarning($"{nameof(SandboxDebugMenuController)}: No UIManager available.");
                return;
            }
            var currentLayout = GameSession.Instance.GetActiveLoadout();
            var player = BattleManager.instance != null ? BattleManager.instance.Player : null;

            UIManager.instance.InitializePlayerActionButtonPrefabs(player.Runtime.actions, currentLayout);
            UIManager.instance.RestingUI.SetActive(true);
            UIManager.instance.DisableBattleSkills();
            UIManager.instance.ShowUIIgnoreLocked();

            if (UIManager.instance.SkillsInventoryViewport != null)
                UIManager.instance.SkillsInventoryViewport.SetActive(true);

            menuPanel?.SetActive(false);
        }
    }
}
