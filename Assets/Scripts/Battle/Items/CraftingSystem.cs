using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Battle.Items
{
    public static class CraftingSystem
    {
        public static bool TryCraft(List<BaseItem> provided, Recipe recipe, out List<BaseItem> result)
        {
            result = null;

            // Ensure exact ingredient match
            if (!recipe.ingredients.All(i => provided.Contains(i)))
                return false;

            result = recipe.result;
            return true;
        }
    }

    [CreateAssetMenu(menuName = "Items/Recipe")]
    public class Recipe : ScriptableObject
    {
        public List<BaseItem> ingredients;
        public List<BaseItem> result;
    }
}
