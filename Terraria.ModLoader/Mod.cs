using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;

namespace Terraria.ModLoader {
public abstract class Mod
{
    internal string file;
    internal Assembly code;
    private string name;
    public string Name
    {
        get
        {
            return name;
        }
    }
    private ModProperties properties;
    public ModProperties Properties
    {
        get
        {
            return properties;
        }
    }
    internal readonly List<ModRecipe> recipes = new List<ModRecipe>();
    internal readonly IDictionary<string, ModItem> items = new Dictionary<string, ModItem>();
    internal GlobalItem globalItem;
    internal readonly IDictionary<string, ModDust> dusts = new Dictionary<string, ModDust>();
    internal readonly IDictionary<string, ModTile> tiles = new Dictionary<string, ModTile>();
    internal GlobalTile globalTile;
    internal GlobalNPC globalNPC;

    /*
     * Initializes the mod's information, such as its name.
     */
    internal void Init()
    {
        ModProperties properties = new ModProperties();
        properties.Autoload = false;
        SetModInfo(out name, ref properties);
        this.properties = properties;
    }

    public abstract void SetModInfo(out string name, ref ModProperties properties);

    public virtual void Load() { }

    public virtual void AddRecipes() {}

    internal void Autoload()
    {
        Type[] classes = code.GetTypes();
        foreach(Type type in classes)
        {
            if(type.IsSubclassOf(typeof(ModItem)))
            {
                AutoloadItem(type);
            }
            if(type.IsSubclassOf(typeof(ModDust)))
            {
                AutoloadDust(type);
            }
            if(type.IsSubclassOf(typeof(ModTile)))
            {
                AutoloadTile(type);
            }
        }
    }

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

    public int AddEquipTexture(EquipType type, string texture, string armTexture = "", string femaleTexture = "")
    {
        int slot = EquipLoader.ReserveEquipID(type);
        EquipLoader.equips[type][texture] = slot;
        if(type == EquipType.Body)
        {
            EquipLoader.armTextures[slot] = armTexture;
            EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;
        }
        return slot;
    }

    private void AutoloadItem(Type type)
    {
        ModItem item = (ModItem)Activator.CreateInstance(type);
        item.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        EquipType? equip = null;
        if (item.Autoload(ref name, ref texture, ref equip))
        {
            AddItem(name, item, texture);
            if (equip.HasValue)
            {
                string equipTexture = texture + "_" + equip.Value;
                string armTexture = texture + "_Arms";
                string femaleTexture = texture + "_FemaleBody";
                item.AutoloadEquip(ref equipTexture, ref armTexture, ref femaleTexture);
                int slot = AddEquipTexture(equip.Value, equipTexture, armTexture, femaleTexture);
                EquipLoader.idToType[item.item.type] = equip.Value;
                EquipLoader.idToSlot[item.item.type] = slot;
            }
        }
    }

    public void AddDust(string name, ModDust dust, string texture = "")
    {
        dust.Name = name;
        if(texture.Length > 0)
        {
            dust.Texture = ModLoader.GetTexture(texture);
        }
        else
        {
            dust.Texture = Main.dustTexture;
        }
        dust.mod = this;
        dusts[name] = dust;
    }

    private void AutoloadDust(Type type)
    {
        ModDust dust = (ModDust)Activator.CreateInstance(type);
        dust.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        if(dust.Autoload(ref name, ref texture))
        {
            AddDust(name, dust, texture);
        }
    }

    public void AddTile(string name, ModTile tile, string texture)
    {
        int id = TileLoader.ReserveTileID();
        tile.Name = name;
        tile.Type = (ushort)id;
        tiles[name] = tile;
        TileLoader.tiles[id] = tile;
        tile.texture = texture;
        tile.mod = this;
    }

    public ModTile GetTile(string name)
    {
        if (tiles.ContainsKey(name))
        {
            return tiles[name];
        }
        else
        {
            return null;
        }
    }

    public int TileType(string name)
    {
        ModTile tile = GetTile(name);
        if (tile == null)
        {
            return 0;
        }
        return (int)tile.Type;
    }

    public void SetGlobalTile(GlobalTile globalTile)
    {
        globalTile.mod = this;
        this.globalTile = globalTile;
    }

    public GlobalTile GetGlobalTile()
    {
        return this.globalTile;
    }

    private void AutoloadTile(Type type)
    {
        ModTile tile = (ModTile)Activator.CreateInstance(type);
        tile.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        if (tile.Autoload(ref name, ref texture))
        {
            AddTile(name, tile, texture);
        }
    }

    public void SetGlobalNPC(GlobalNPC globalNPC)
    {
        globalNPC.mod = this;
        this.globalNPC = globalNPC;
    }

    public GlobalNPC GetGlobalNPC()
    {
        return this.globalNPC;
    }

    internal void SetupContent()
    {
        foreach(ModItem item in items.Values)
        {
            Main.itemTexture[item.item.type] = ModLoader.GetTexture(item.texture);
            Main.itemName[item.item.type] = item.item.name;
            EquipLoader.SetSlot(item.item);
            item.SetDefaults();
            DrawAnimation animation = item.GetAnimation();
            if(animation != null)
            {
                Main.RegisterItemAnimation(item.item.type, animation);
                ItemLoader.animations.Add(item.item.type);
            }
        }
        foreach(ModTile tile in tiles.Values)
        {
            Main.tileTexture[tile.Type] = ModLoader.GetTexture(tile.texture);
            tile.SetDefaults();
        }
        if(globalTile != null)
        {
            globalTile.SetDefaults();
        }
    }

    internal void Unload() //I'm not sure why I have this
    {
        recipes.Clear();
        items.Clear();
        globalItem = null;
        dusts.Clear();
        tiles.Clear();
        globalNPC = null;
    }
}}