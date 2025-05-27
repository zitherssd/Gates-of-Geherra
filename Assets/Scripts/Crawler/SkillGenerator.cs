using System;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Crawler
{
    public class SkillGenerator : MonoBehaviour
    {
        //public List<BaseAction> ActionsPool;
        public GameObject CardPrefab;
        public static SkillGenerator instance;
        public string resourcePath = "Actions/Droptable";
        public Canvas chooseSkillsUI;
        BaseAction[] allActions; 

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            allActions = Resources.LoadAll<BaseAction>(resourcePath);

        }

        public BaseAction[] GetRandomActions(int floor)
        {
            // Load all ScriptableObjects from the specified folder

            if (allActions.Length == 0)
            {
                Debug.LogWarning("No actions found in the folder: " + resourcePath);
                return new BaseAction[0]; // Return empty array if no actions exist
            }

            // Shuffle the array and pick up to 3 actions
            return allActions.OrderBy(a => UnityEngine.Random.value).Take(Mathf.Min(3, allActions.Length)).ToArray();
        }

        public void DrawSkillsFromSelectionAndWaitForSelection(BaseAction[] skills, Action onSelectionComplete)
        {
            if (skills == null || skills.Length == 0)
            {
                Debug.LogWarning("No skills available for selection.");
                onSelectionComplete?.Invoke();
                return;
            }

            // UI Parent
            Transform parent = chooseSkillsUI.transform;

            for (int i = 0; i < skills.Length; i++)
            {
                var UICard = Instantiate(CardPrefab, Vector3.zero, Quaternion.identity, parent);
                UICard.transform.localScale = new Vector3(1, 0, 1);
                var handler = UICard.GetComponent<ActionCardHandler>();
                handler.ReferencedAction = skills[i];

                // Assign button click event
                UICard.GetComponent<Button>().onClick.AddListener(() =>
                {
                    BattleManager.instance.PlayerActors[0].ActorData.actions.Add(handler.ReferencedAction);
                    UIManager.instance.DrawActions(BattleManager.instance.PlayerActors[0].ActorData.actions);

                    // Destroy all skill cards after selection
                    foreach (Transform child in parent)
                    {
                        Destroy(child.gameObject);
                    }

                    onSelectionComplete?.Invoke();
                });
                LeanTween.scale(UICard.gameObject, Vector3.one * 2, 1f).setEaseOutBack().setIgnoreTimeScale(true);
            }
        }

        void onClickCardHandler(BaseAction action)
        {
            BattleManager.instance.PlayerActors[0].ActorData.actions.Add(action);
        }

        void ModifyFloats(ref float field1, ref float field2, ref float field3)
        {
            System.Random random = new System.Random();
            int randomIndex1 = random.Next(3); // Random index between 0 and 2
            int randomIndex2;

            do
            {
                randomIndex2 = random.Next(3); // Ensure randomIndex2 is different from randomIndex1
            }
            while (randomIndex2 == randomIndex1);

            if (randomIndex1 == 0)
            {
                field1 *= 1.2f;
                field2 *= 0.9f;
            }
            else if (randomIndex1 == 1)
            {
                field2 *= 1.2f;
                field3 *= 0.9f;
            }
            else
            {
                field3 *= 1.2f;
                field1 *= 0.9f;
            }
        }

        //public void UpgradeSkillAtRandom(ref Assets.Scripts.Actions.BaseAction skill)
        //{
        //    var random = new System.Random();
        //    int chance = random.Next(1, 101);
        //    bool tagAdded = false;

        //    if (chance <= 10 && skill.TotalUses != 0)
        //    {
        //        // 10% chance to increase speed by 1
        //        skill.Speed++;
        //        Debug.Log("Skill speed increased by 1.");
        //    }
        //    else if (chance <= 20)
        //    {
        //        // 10% chance to increase Total Uses by 1
        //        skill.TotalUses++;
        //        Debug.Log("Skill Total Uses increased by 1.");
        //    }
        //    else if (chance <= 30)
        //    {
        //        // 10% chance to increase Range by 0.5f
        //        skill.Range += 0.5f;
        //        Debug.Log("Skill Range increased by 0.5f.");
        //    }
        //    else if (chance <= 40)
        //    {
        //        // Check if the skill already has the MOVE_NEAR_ENEMY_BEFORE_ATTACK tag
        //        if (!skill.Tags.Contains(TAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK))
        //        {
        //            // Add the MOVE_NEAR_ENEMY_BEFORE_ATTACK tag
        //            skill.Tags.Add(TAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK);
        //            Debug.Log("Added MOVE_NEAR_ENEMY_BEFORE_ATTACK tag to the skill.");
        //            tagAdded = true;
        //        }
        //    }

        //    // If the 10% chance for adding a tag wasn't used, redistribute the probability
        //    if (!tagAdded && chance <= 90)
        //    {
        //        chance += 10;
        //    }

        //    if (!tagAdded)
        //    {
        //        // The skill already has the MOVE_NEAR_ENEMY_BEFORE_ATTACK tag, choose another tag to add
        //        bool foundTag = false;
        //        List<TAG> randomTagsToAdd = new List<TAG>() { TAG.MOVE_OFFSET_BEHIND, TAG.MOVE_OFFSET_INFRONT };  // Replace with your actual list of tags
        //        randomTagsToAdd = randomTagsToAdd.OrderBy(item => random.Next()).ToList();

        //        foreach (var tag in randomTagsToAdd)
        //        {
        //            if (!skill.Tags.Contains(tag))
        //            {
        //                skill.Tags.Add(tag);
        //                Debug.Log("Added " + tag + " tag to the skill.");
        //                foundTag = true;
        //                break;
        //            }
        //        }

        //        if (!foundTag)
        //        {
        //            // Unable to find a tag to add, redistributing the probability
        //            chance -= 10;
        //        }
        //    }

        //    // Continue with the remaining upgrade logic based on the adjusted chance
        //    if (chance <= 100)
        //    {
        //        // 60% chance to upgrade one stat (Damage, PostureDamage, KnockbackForce) by 20%
        //        int statUpgradeChance = random.Next(1, 4); // Randomly select the stat to upgrade

        //        switch (statUpgradeChance)
        //        {
        //            case 1:
        //                skill.Damage = Mathf.CeilToInt(skill.Damage * 1.2f);
        //                Debug.Log("Skill damage increased by 20%.");
        //                break;

        //            case 2:
        //                skill.PostureDamage = Mathf.CeilToInt(skill.PostureDamage * 1.2f);
        //                Debug.Log("Skill posture damage increased by 20%.");
        //                break;

        //            case 3:
        //                skill.KnockbackForce = Mathf.CeilToInt(skill.KnockbackForce * 1.2f);
        //                Debug.Log("Skill knockback force increased by 20%.");
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}

        //public void UpgradeReactionAtRandom(ref BaseReaction reaction)
        //{
        //    var random = new System.Random();
        //    int chance = random.Next(1, 101);

        //    if (reaction.TotalUses != 0 && chance <= 10)
        //    {
        //        // 10% chance to increase Total Uses by 1 if TotalUses is not 0
        //        reaction.TotalUses++;
        //        Debug.Log("Reaction Total Uses increased by 1.");
        //    }
        //    else if (chance <= 20)
        //    {
        //        // 10% chance to increase Speed by 1
        //        reaction.Speed++;
        //        Debug.Log("Reaction speed increased by 1.");
        //    }
        //    else if (chance <= 80)
        //    {
        //        // 80% chance to reduce one stat (KnockbackModifier, DamageModifier, PostureModifier) by 20%
        //        int statReductionChance = random.Next(1, 4); // Randomly select the stat to reduce

        //        switch (statReductionChance)
        //        {
        //            case 1:
        //                reaction.knockbackModifier = (reaction.knockbackModifier * 0.8f);
        //                Debug.Log("Reaction knockback modifier reduced by 20%.");
        //                break;

        //            case 2:
        //                reaction.damageModifier = (reaction.damageModifier * 0.8f);
        //                Debug.Log("Reaction damage modifier reduced by 20%.");
        //                break;

        //            case 3:
        //                reaction.postureModifier = (reaction.postureModifier * 0.8f);
        //                Debug.Log("Reaction posture modifier reduced by 20%.");
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
