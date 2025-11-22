using Assets.Scripts.Save;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSlot : MonoBehaviour
{
    public SaveManager saveManager;
    public int slotNumber;
    private TMPro.TextMeshProUGUI slotText;

    void Start()
    {
        slotText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (saveManager.SlotExists(slotNumber))
        {
            var save = saveManager.LoadGame(slotNumber);
            slotText.text = $"{save.player.Name}, {save.currentFloor}";
        }
        else
        {
            slotText.text = "Empty Slot";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
