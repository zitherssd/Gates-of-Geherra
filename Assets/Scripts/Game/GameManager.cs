using Assets.Scripts.Save;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum Phase { Rest, Battle }
    public Phase CurrentPhase;
    public int CurrentFloor = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        instance = this;
    }

    public void StartNewGame(int slot)
    {
        //SaveManager.instance.currentSaveSlot = slot;

        //if (SaveManager.instance.SlotExists(slot))
        //    LoadGame(slot);
        //else
            ////CreateNewGame();
        SaveManager.instance.currentSaveSlot = slot;
        SceneManager.LoadScene("CaveScene");
    }

    void LoadGame(int slot)
    {
        var save = SaveManager.instance.LoadFromSlot(slot);
        CurrentFloor = save.currentFloor;
        CurrentPhase = CurrentFloor == 0 ? Phase.Battle : Phase.Rest;
    }

    void CreateNewGame()
    {
        CurrentFloor = 0;
        CurrentPhase = Phase.Rest;
    }
}
