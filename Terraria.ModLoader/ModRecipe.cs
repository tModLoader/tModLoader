using System;
using System.Collections.Generic;
using Terraria;

namespace Terraria.ModLoader {
public class ModRecipe : Recipe
{
    public readonly Mod mod;
    internal readonly IList<CraftGroup> craftGroups = new List<CraftGroup>();
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

    public void SetResult(ModItem item, int stack = 1)
    {
        this.SetResult(item.item.type, stack);
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

    public void AddIngredient(ModItem item, int stack = 1)
    {
        this.AddIngredient(item.item.type, stack);
    }

    public void AddCraftGroup(CraftGroup group, int stack = 1)
    {
        this.AddIngredient(group.Items[0], stack);
        this.craftGroups.Add(group);
    }

    public void AddCraftGroup(Mod mod, string name, int stack = 1)
    {
        if(mod == null)
        {
            mod = this.mod;
        }
        this.AddCraftGroup(mod.GetCraftGroup(name), stack);
    }

    public void AddTile(int tileID)
    {
        this.requiredTile[numTiles] = tileID;
        numTiles++;
    }

    public void AddTile(Mod mod, string tileName)
    {
        if(mod == null)
        {
            mod = this.mod;
        }
        this.AddTile(mod.TileType(tileName));
    }

    public void AddTile(ModTile tile)
    {
        this.AddTile(tile.Type);
    }

    //in Terraria.Recipe.Create before alchemy table check add
    //  ModRecipe modRecipe = this as ModRecipe;
    //  if(modRecipe != null) { num = modRecipe.ConsumeItem(item.type, item.stack); }
    public virtual int ConsumeItem(int type, int numRequired)
    {
        return numRequired;
    }

    //for Terraria.Recipe add public bool ItemMatches(Item invItem, Item reqItem)
    //replace || chains with Terraria.Recipe.ItemMatches
    //add this to the || chain in Terraria.Recipe.ItemMatches
    public bool UseCraftGroup(int invType, int reqType)
    {
        foreach(CraftGroup group in craftGroups)
        {
            if(group.Items.Contains(invType) && group.Items.Contains(reqType))
            {
                return true;
            }
        }
        return false;
    }

    //add to craft group tooltip in Terraria.Main.DrawInventory
    public void CraftGroupDisplayName(int reqIndex)
    {
        foreach(CraftGroup group in craftGroups)
        {
            if(group.Items.Contains(requiredItem[reqIndex].type))
            {
                Main.toolTip.name = group.DisplayName;
                return;
            }
        }
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
        if(Recipe.numRecipes >= Recipe.maxRecipes)
        {
            Recipe.maxRecipes += 500;
            Array.Resize(ref Main.recipe, Recipe.maxRecipes);
            Array.Resize(ref Main.availableRecipe, Recipe.maxRecipes);
            Array.Resize(ref Main.availableRecipeY, Recipe.maxRecipes);
            for(int k = Recipe.numRecipes; k < Recipe.maxRecipes; k++)
            {
                Main.recipe[k] = new Recipe();
                Main.availableRecipeY[k] = 65f * k;
            }
        }
        Main.recipe[Recipe.numRecipes] = this;
        this.RecipeIndex = Recipe.numRecipes;
        mod.recipes.Add(this);
        Recipe.numRecipes++;
    }
}}