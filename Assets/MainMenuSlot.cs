using Assets.Scripts.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSlot : MonoBehaviour
{
    public SaveManager saveManager;
    public int slotNumber;
    private TMPro.TextMeshProUGUI slotText;
    public GameObject DeleteButton;

    void Start()
    {
        slotText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (saveManager.SlotExists(slotNumber))
        {
            DeleteButton.SetActive(true);
            var save = saveManager.LoadGame(slotNumber);
            slotText.text = $"{save.player.Name}, {save.currentFloor}";
            //enable delete button
        }
        else
        {
            DeleteButton.SetActive(false);
            slotText.text = "Empty Slot";
        }
    }

    public void Delete()
    {
        saveManager.DeleteSave(slotNumber);
        Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
