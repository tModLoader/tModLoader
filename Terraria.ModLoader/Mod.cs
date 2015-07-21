using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;

namespace Terraria.ModLoader {
public abstract class Mod
{
    internal string file;
    private string name;
    public string Name
    {
        get
        {
            return name;
        }
        private set
        {
            name = value;
        }
    }
    private string version;
    public string Version
    {
        get
        {
            return version;
        }
        private set
        {
            name = value;
        }
    }
    private string author;
    public string Author
    {
        get
        {
            return author;
        }
        private set
        {
            name = value;
        }
    }
    internal readonly List<ModRecipe> recipes = new List<ModRecipe>();
    internal readonly IDictionary<string, ModItem> items = new Dictionary<string, ModItem>();
    internal GlobalItem globalItem;

    /*
     * Initializes the mod's information, such as its name.
     */
    internal void Init()
    {
        SetModInfo(out name, ref version, ref author);
    }

    public abstract void SetModInfo(out string name, ref string version, ref string author);

    public abstract void Load();

    public virtual void AddRecipes() {}

    public void AddItem(string name, ModItem item, string texture)
    {
        int id = ItemLoader.ReserveItemID();
        item.item.name = name;
        item.item.ResetStats(id);
        items[name] = item;
        ItemLoader.items[id] = item;
        item.texture = texture;
        item.mod = this;
    }

    public ModItem GetItem(string name)
    {
        if (items.ContainsKey(name))
        {
            return items[name];
        }
        else
        {
            return null;
        }
    }

    public int ItemType(string name)
    {
        ModItem item = GetItem(name);
        if(item == null)
        {
            return 0;
        }
        return item.item.type;
    }

    public void SetGlobalItem(GlobalItem globalItem)
    {
        globalItem.mod = this;
        this.globalItem = globalItem;
    }

    public GlobalItem GetGlobalItem()
    {
        return this.globalItem;
    }

    public void AddEquipTexture(EquipType type, string texture, string armTexture = "", string femaleTexture = "")
    {
        int slot = EquipLoader.ReserveEquipID(type);
        EquipLoader.equips[type][texture] = slot;
        if(type == EquipType.Body)
        {
            EquipLoader.armTextures[slot] = armTexture;
            EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;
        }
    }

    internal void SetupContent()
    {
        foreach(ModItem item in items.Values)
        {
            Main.itemTexture[item.item.type] = ModLoader.GetTexture(item.texture);
            Main.itemName[item.item.type] = item.item.name;
            item.SetDefaults();
            DrawAnimation animation = item.GetAnimation();
            if(animation != null)
            {
                Main.RegisterItemAnimation(item.item.type, animation);
                ItemLoader.animations.Add(item.item.type);
            }
        }
    }

    internal void Unload()
    {
        recipes.Clear();
        items.Clear();
        globalItem = null;
    }
}}