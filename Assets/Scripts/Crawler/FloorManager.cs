using System.Collections.Generic;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actor;
using UnityEngine;

namespace Assets.Scripts.Crawler
{
    public class FloorManager : MonoBehaviour
    {
        public int currentFloor = 1;
        private UIManager uiManager;

        [Header("Floor Enemies")]
        public List<FloorEnemies> floorEnemiesList; // List of all floors and their enemies

        public static FloorManager instance;

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

            var random = new System.Random();
            var line = lines[Random.Range(0, lines.Length)];
            line = line.Replace("{numberth}", GetOrdinal(currentFloor));

            var bm = BattleManager.instance;
            bm.PlayerActors[0].transform.position = Vector3.zero;
            bm.EnemyActors[0].transform.position = Vector3.right * 10;
            bm.EnemyActors[0].PlayAnimation("Idle");

            StartCoroutine(uiManager.TypeTextMiddleLetterByLetter(line, () =>
            {
                StartCoroutine(uiManager.FadeMiddleText(1));
                bm.SetupBattleWithEnemies(GetActorsForFloor());
            }));

        }

        public List<ActorData> GetActorsForFloor()
        {
            // Find the floor in the list and return its enemies
            FloorEnemies floorData = floorEnemiesList.Find(f => f.floorNumber == currentFloor);

            if (floorData != null)
            {
                return floorData.enemies;
            }
            else
            {
                // If the floor is not found, return an empty list or handle as needed
                return new List<ActorData>();
            }
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
            "An opponent stands ready on the {numberth} floor.",
            "Bla bla bla"
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

    [System.Serializable]
    public class FloorEnemies
    {
        public int floorNumber; // The floor number (e.g., 2 for second floor, etc.)
        public List<ActorData> enemies; // List of enemies for this floor
    }
}