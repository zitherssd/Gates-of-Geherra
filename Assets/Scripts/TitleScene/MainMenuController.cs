using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Image titleScreenImage;
    public Button StartButton;
    public Button SandboxButton;


    public void Start()
    {
        LeanTween.move(titleScreenImage.rectTransform, Vector3.up * -266f, 2f).setEaseInOutSine().setOnComplete(() =>
        {
            StartButton.gameObject.SetActive(true);
            SandboxButton.gameObject.SetActive(true);
        });
    }

    public void StartFight()
    {
        SceneManager.LoadScene("DebugScene");
    }
    public void StartSandbox()
    {
        SceneManager.LoadScene("PhysicsDebug" +
            "");
    }
}
