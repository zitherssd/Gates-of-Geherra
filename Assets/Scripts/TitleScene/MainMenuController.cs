using Assets.Scripts;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Game;
using Assets.Scripts.Save;
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
    public TMP_InputField NamePromptPanel;

    public void Awake()
    {
        LeanTween.init(8000);
    }
    public void Start()
    {
        LeanTween.move(titleScreenImage.rectTransform, Vector3.up * -170, 3.5f).setEaseInOutSine().setOnComplete(() =>
        {
            StartButton.gameObject.SetActive(true);
            SandboxButton.gameObject.SetActive(true);
        });
    }

    public void StartNewGame(int slot)
    {
        SaveManager.instance.currentSaveSlot = slot;
        if (GameSession.Exists)
            GameSession.Instance.ClearRun();

        //SceneManager.LoadScene("CaveScene");
    }

    public void StartNewGameForReal()
    {
        SaveManager.instance.newGamePlayerName = NamePromptPanel.text;
        SceneManager.LoadScene("CaveScene");
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
