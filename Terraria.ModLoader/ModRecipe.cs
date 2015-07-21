using System;
using Terraria;

namespace Terraria.ModLoader {
public class ModRecipe : Recipe
{
    public readonly Mod mod;
    private int numIngredients = 0;
    private int numTiles = 0;
    public int RecipeIndex
    {
        get;
        private set;
    }

    public ModRecipe(Mod mod)
    {
        this.mod = mod;
    }

    public void SetResult(int itemID, int stack = 1)
    {
        this.createItem.SetDefaults(itemID, false);
        this.createItem.stack = stack;
    }

    public void SetResult(string itemName, int stack = 1)
    {
        this.createItem.SetDefaults(itemName);
        this.createItem.stack = stack;
    }

    public void SetResult(Mod mod, string itemName, int stack = 1)
    {
        if(mod == null)
        {
            mod = this.mod;
        }
        this.SetResult(mod.ItemType(itemName), stack);
    }

    public void AddIngredient(int itemID, int stack = 1)
    {
        this.requiredItem[numIngredients].SetDefaults(itemID, false);
        this.requiredItem[numIngredients].stack = stack;
        numIngredients++;
    }

    public void AddIngredient(string itemName, int stack = 1)
    {
        this.requiredItem[numIngredients].SetDefaults(itemName);
        this.requiredItem[numIngredients].stack = stack;
        numIngredients++;
    }

    public void AddIngredient(Mod mod, string itemName, int stack = 1)
    {
        if(mod == null)
        {
            mod = this.mod;
        }
        this.AddIngredient(mod.ItemType(itemName), stack);
    }

    public void AddTile(int tileID)
    {
        this.requiredTile[numTiles] = tileID;
        numTiles++;
    }

    public void AddRecipe()
    {
        for(int k = 0; k < Recipe.maxRequirements; k++)
        {
            if(this.requiredTile[k] == 13)
            {
                this.alchemy = true;
                break;
            }
        }
        Main.recipe[Recipe.numRecipes] = this;
        this.RecipeIndex = Recipe.numRecipes;
        mod.recipes.Add(this);
        Recipe.numRecipes++;
    }
}}