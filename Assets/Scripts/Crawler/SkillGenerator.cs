using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillGenerator : MonoBehaviour
{
    //public List<BaseAction> ActionsPool;
    public GameObject CardPrefab;
    private static SkillGenerator instance;
    public static SkillGenerator GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public void GenerateOptionsForCurrentFloorAndWaitForSelection(Action onSelectionComplete)
    {
        var floorManager = FloorManager.GetInstance();
        if (floorManager.currentFloor <= 5)
        {
            var drops =  Resources.LoadAll<BaseAction>("Crawler/Droptables/1-5");
            var ActionsPool = drops.ToList();

            int randomIndex = UnityEngine.Random.Range(0, ActionsPool.Count);
            var skill1 = ActionsPool[randomIndex];
            randomIndex = UnityEngine.Random.Range(0, ActionsPool.Count);
            var skill2 = ActionsPool[randomIndex];
            randomIndex = UnityEngine.Random.Range(0, ActionsPool.Count);
            var skill3 = ActionsPool[randomIndex];

            GenerateSkillsAndWaitForSelection(skill1, skill2, skill3, onSelectionComplete);
        }
    }

    public void GenerateSkillsAndWaitForSelection(BaseAction skill1, BaseAction skill2, BaseAction skill3, Action onSelectionComplete)
    {
        var UICard1 = Instantiate(CardPrefab, Vector2.zero, Quaternion.identity);
        UICard1.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);
        UICard1.GetComponent<RectTransform>().localPosition = Vector3.zero;
        var handler = UICard1.GetComponent<ButtonHandler>();
        handler.referencedAction = skill1;
        handler.InitCard();

        var UICard2 = Instantiate(CardPrefab, Vector2.zero, Quaternion.identity);
        UICard2.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);
        UICard2.GetComponent<RectTransform>().localPosition = Vector3.right * 250;
        handler = UICard2.GetComponent<ButtonHandler>();
        handler.referencedAction = skill2;
        handler.InitCard();

        var UICard3 = Instantiate(CardPrefab, Vector2.zero, Quaternion.identity);
        UICard3.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);
        UICard3.GetComponent<RectTransform>().localPosition = Vector3.right * -250;
        handler = UICard3.GetComponent<ButtonHandler>();
        handler.referencedAction = skill3;
        handler.InitCard();

        UICard1.GetComponent<Button>().onClick.AddListener(() =>
        {
            onClickCardHandler(skill1);
            Destroy(UICard1.gameObject);
            Destroy(UICard2.gameObject);
            Destroy(UICard3.gameObject);
            onSelectionComplete.Invoke();
        });

        UICard2.GetComponent<Button>().onClick.AddListener(() =>
        {
            onClickCardHandler(skill2);
            Destroy(UICard1.gameObject);
            Destroy(UICard2.gameObject);
            Destroy(UICard3.gameObject);
            onSelectionComplete.Invoke();
        });

        UICard3.GetComponent<Button>().onClick.AddListener(() =>
        {
            onClickCardHandler(skill3);
            Destroy(UICard1.gameObject);
            Destroy(UICard2.gameObject);
            Destroy(UICard3.gameObject);
            onSelectionComplete.Invoke();
        });

        //muhahaha
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

    public BaseAction GenerateSkillFromBase()
    {
        //var random = new System.Random();
        //var baseSkill = BaseSkillsForGeneration[random.Next(BaseSkillsForGeneration.Count)];
        //var clone = Instantiate(baseSkill);
        //ModifyFloats(ref clone.Damage, ref clone.PostureDamage, ref clone.KnockbackForce);
        return null;
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
