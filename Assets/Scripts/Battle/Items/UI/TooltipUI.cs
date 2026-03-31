using TMPro;
using UnityEngine;

namespace Assets.Scripts.Battle.Items.UI
{
    public class TooltipUI : MonoBehaviour
    {
        public static TooltipUI instance;

        [SerializeField] private GameObject root;
        [SerializeField] private GameObject rootPrompt;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI promptText;

        private void Awake()
        {
            instance = this;
            root.SetActive(false);
            rootPrompt.SetActive(false);
        }

        public void ShowPrompt(string prompt)
        {
            promptText.text = prompt;
            rootPrompt.SetActive(true);
        }

        public void Show(string message, Vector3 position)
        {
            text.text = message;
            root.SetActive(true);
            root.transform.position = new Vector3(position.x,position.y, 10);
            LeanTween.scale(root, Vector3.one, 0.2f).setEaseOutBack();
        }
        public void ShowThenHide(string message, Vector3 position)
        {
            text.text = message;
            root.SetActive(true);
            root.transform.position = position;
            LeanTween.scale(root, Vector3.one, 0.2f).setEaseOutBack().setOnComplete(() =>
            {
                LeanTween.scale(root, Vector3.zero, 5f).setEaseInBack().setOnComplete(() => Hide());
            });
        }

        public void Hide()
        {
            LeanTween.scale(root, Vector3.zero, 0.1f).setEaseInBack().setOnComplete(() =>
            {
                root.SetActive(false);
            });
        }
    }
}
