using Assets.Scripts.Battle.Actor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Image titleScreenImage;
    public Button StartButton;
    public Button SandboxButton;
    public Button[] Slots ;


    public void Start()
    {
        Slots = transform.GetComponentsInChildren<Button>();
        LeanTween.move(titleScreenImage.rectTransform, Vector3.up * -170, 3.5f).setEaseInOutSine().setOnComplete(() =>
        {
            StartButton.gameObject.SetActive(true);
            SandboxButton.gameObject.SetActive(true);
        });

        foreach (var item in Slots)
        {
            var text = item.GetComponentInChildren<TextMeshPro>();

            //If save is empty in that slot set item.
            //text.text = "Empty";
            //text.fontStyle = FontStyles.Italic;

            //item.onClick += SaveInSlot(slotIndex);
        }
    }

    public void SaveInSlot(int slotIndex)
    {
        string key = $"SaveSlot_{slotIndex}";
        var save = JsonUtility.ToJson(Resources.Load<ActorData>(key));
        PlayerPrefs.SetString(key, save);

    }

    public void StartFight()
    {
        SceneManager.LoadScene("DebugScene");
    }
    public void StartSandbox()
    {
        SceneManager.LoadScene("SandboxScene");
    }
}
