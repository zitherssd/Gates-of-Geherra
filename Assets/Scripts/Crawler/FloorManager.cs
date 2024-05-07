using Assets;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public int currentFloor = 1;
    private UIManager uiManager;

    public List<ActorData> secondFloorEnemies;
    public List<ActorData> thirdFloorEnemies;
    public List<ActorData> fourthFloorEnemies;

    private static FloorManager instance;
    public static FloorManager GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        uiManager = UIManager.GetInstance();
    }

    public void ProgressToNextFloor()
    {
        currentFloor++;
        uiManager.Fade(true, () =>
        {
            var random = new System.Random();
            var line = lines[Random.Range(0, lines.Length)];
            line = line.Replace("{numberth}", GetOrdinal(currentFloor));

            var bm = BattleManager.instance;
            bm.PlayerActors[0].transform.position = Vector3.zero;
            bm.EnemyActors[0].transform.position = Vector3.right * 10;
            bm.EnemyActors[0].PlayAnimation("Idle");

            SkillGenerator.GetInstance().GenerateOptionsForCurrentFloorAndWaitForSelection(() =>
            {
                StartCoroutine(uiManager.TypeTextMiddleLetterByLetter(line, () =>
                {
                    StartCoroutine(uiManager.FadeMiddleText(1));
                    bm.SetupBattleWithEnemy(GetActorForFloor());


                }));
            });
        });
    }

    public ActorData GetActorForFloor()
    {
        if (currentFloor == 2)
            return secondFloorEnemies[Random.Range(0, secondFloorEnemies.Count)];
        else if (currentFloor == 3)
            return thirdFloorEnemies[Random.Range(0, thirdFloorEnemies.Count)];
        else
            return fourthFloorEnemies[Random.Range(0, fourthFloorEnemies.Count)];
    }



    string[] lines = {
            "A combatant approaches...",
            "On the {numberth} floor someone challenges me.",
            "A rival awaits me on the {numberth} floor.",
            "A contender on the {numberth} floor challenges my presence.",
            "A challenger emerges before me...",
            "On the {numberth} floor, an adversary appears.",
            "Someone on the {numberth} floor steps up to the challenge.",
            "A foe confronts me on the {numberth} floor.",
            "An opponent stands ready on the {numberth} floor."
        };

    static string GetOrdinal(int number)
    {
        if (number <= 0)
            return number.ToString();

        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }

        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }



}
