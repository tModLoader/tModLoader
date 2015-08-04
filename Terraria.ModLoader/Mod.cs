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
    internal readonly IDictionary<string, GlobalItem> globalItems = new Dictionary<string, GlobalItem>();
    internal readonly IDictionary<string, ModDust> dusts = new Dictionary<string, ModDust>();
    internal readonly IDictionary<string, ModTile> tiles = new Dictionary<string, ModTile>();
    internal readonly IDictionary<string, GlobalTile> globalTiles = new Dictionary<string, GlobalTile>();
    internal readonly IDictionary<string, GlobalNPC> globalNPCs = new Dictionary<string, GlobalNPC>();

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
            if(type.IsSubclassOf(typeof(GlobalItem)))
            {
                AutoloadGlobalItem(type);
            }
            if(type.IsSubclassOf(typeof(ModDust)))
            {
                AutoloadDust(type);
            }
            if(type.IsSubclassOf(typeof(ModTile)))
            {
                AutoloadTile(type);
            }
            if(type.IsSubclassOf(typeof(GlobalTile)))
            {
                AutoloadGlobalTile(type);
            }
            if(type.IsSubclassOf(typeof(GlobalNPC)))
            {
                AutoloadGlobalNPC(type);
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

    public void AddGlobalItem(string name, GlobalItem globalItem)
    {
        globalItem.mod = this;
        globalItem.Name = name;
        this.globalItems[name] = globalItem;
        ItemLoader.globalItems.Add(globalItem);
    }

    public GlobalItem GetGlobalItem(string name)
    {
        if(this.globalItems.ContainsKey(name))
        {
            return this.globalItems[name];
        }
        else
        {
            return null;
        }
    }

    public int AddEquipTexture(ModItem item, EquipType type, string texture, string armTexture = "", string femaleTexture = "")
    {
        int slot = EquipLoader.ReserveEquipID(type);
        EquipLoader.equips[type][texture] = slot;
        if(type == EquipType.Body)
        {
            EquipLoader.armTextures[slot] = armTexture;
            EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;
        }
        if(type == EquipType.Head || type == EquipType.Body || type == EquipType.Legs)
        {
            EquipLoader.slotToId[type][slot] = item.item.type;
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
                int slot = AddEquipTexture(item, equip.Value, equipTexture, armTexture, femaleTexture);
                EquipLoader.idToType[item.item.type] = equip.Value;
                EquipLoader.idToSlot[item.item.type] = slot;
            }
        }
    }

    private void AutoloadGlobalItem(Type type)
    {
        GlobalItem globalItem = (GlobalItem)Activator.CreateInstance(type);
        globalItem.mod = this;
        string name = type.Name;
        if(globalItem.Autoload(ref name))
        {
            AddGlobalItem(name, globalItem);
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

    public void AddGlobalTile(string name, GlobalTile globalTile)
    {
        globalTile.mod = this;
        globalTile.Name = name;
        this.globalTiles[name] = globalTile;
        TileLoader.globalTiles.Add(globalTile);
    }

    public GlobalTile GetGlobalTile(string name)
    {
        if(this.globalTiles.ContainsKey(name))
        {
            return globalTiles[name];
        }
        else
        {
            return null;
        }
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

    private void AutoloadGlobalTile(Type type)
    {
        GlobalTile globalTile = (GlobalTile)Activator.CreateInstance(type);
        globalTile.mod = this;
        string name = type.Name;
        if(globalTile.Autoload(ref name))
        {
            AddGlobalTile(name, globalTile);
        }
    }

    public void AddGlobalNPC(string name, GlobalNPC globalNPC)
    {
        globalNPC.mod = this;
        globalNPC.Name = name;
        this.globalNPCs[name] = globalNPC;
        NPCLoader.globalNPCs.Add(globalNPC);
    }

    public GlobalNPC GetGlobalNPC(string name)
    {
        if(this.globalNPCs.ContainsKey(name))
        {
            return this.globalNPCs[name];
        }
        else
        {
            return null;
        }
    }

    private void AutoloadGlobalNPC(Type type)
    {
        GlobalNPC globalNPC = (GlobalNPC)Activator.CreateInstance(type);
        globalNPC.mod = this;
        string name = type.Name;
        if(globalNPC.Autoload(ref name))
        {
            AddGlobalNPC(name, globalNPC);
        }
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
            TileLoader.SetDefaults(tile);
        }
        foreach(GlobalTile globalTile in globalTiles.Values)
        {
            globalTile.SetDefaults();
        }
    }

    internal void Unload() //I'm not sure why I have this
    {
        recipes.Clear();
        items.Clear();
        globalItems.Clear();
        dusts.Clear();
        tiles.Clear();
        globalTiles.Clear();
        globalNPCs.Clear();
    }
}}