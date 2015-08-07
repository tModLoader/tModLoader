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
    internal readonly IDictionary<string, ModWall> walls = new Dictionary<string, ModWall>();
    internal readonly IDictionary<string, GlobalWall> globalWalls = new Dictionary<string, GlobalWall>();
    internal readonly IDictionary<string, ModProjectile> projectiles = new Dictionary<string, ModProjectile>();
    internal readonly IDictionary<string, GlobalProjectile> globalProjectiles = new Dictionary<string, GlobalProjectile>();
    internal readonly IDictionary<string, ModNPC> npcs = new Dictionary<string, ModNPC>();
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
            if(type.IsSubclassOf(typeof(ModWall)))
            {
                AutoloadWall(type);
            }
            if(type.IsSubclassOf(typeof(GlobalWall)))
            {
                AutoloadGlobalWall(type);
            }
            if(type.IsSubclassOf(typeof(ModProjectile)))
            {
                AutoloadProjectile(type);
            }
            if(type.IsSubclassOf(typeof(GlobalProjectile)))
            {
                AutoloadGlobalProjectile(type);
            }
            if(type.IsSubclassOf(typeof(ModNPC)))
            {
                AutoloadNPC(type);
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
        ModLoader.GetTexture(texture);
        if(type == EquipType.Body)
        {
            EquipLoader.armTextures[slot] = armTexture;
            EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;
            ModLoader.GetTexture(armTexture);
            ModLoader.GetTexture(femaleTexture);
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
        IList<EquipType> equips = new List<EquipType>();
        if(item.Autoload(ref name, ref texture, equips))
        {
            AddItem(name, item, texture);
            if(equips.Count > 0)
            {
                EquipLoader.idToSlot[item.item.type] = new Dictionary<EquipType, int>();
                foreach(EquipType equip in equips)
                {
                    string equipTexture = texture + "_" + equip.ToString();
                    string armTexture = texture + "_Arms";
                    string femaleTexture = texture + "_FemaleBody";
                    item.AutoloadEquip(equip, ref equipTexture, ref armTexture, ref femaleTexture);
                    int slot = AddEquipTexture(item, equip, equipTexture, armTexture, femaleTexture);
                    EquipLoader.idToSlot[item.item.type][equip] = slot;
                }
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

    public void AddWall(string name, ModWall wall, string texture)
    {
        int id = WallLoader.ReserveWallID();
        wall.Name = name;
        wall.Type = (ushort)id;
        walls[name] = wall;
        WallLoader.walls[id] = wall;
        wall.texture = texture;
        wall.mod = this;
    }

    public ModWall GetWall(string name)
    {
        if (walls.ContainsKey(name))
        {
            return walls[name];
        }
        else
        {
            return null;
        }
    }

    public int WallType(string name)
    {
        ModWall wall = GetWall(name);
        if (wall == null)
        {
            return 0;
        }
        return (int)wall.Type;
    }

    public void AddGlobalWall(string name, GlobalWall globalWall)
    {
        globalWall.mod = this;
        globalWall.Name = name;
        this.globalWalls[name] = globalWall;
        WallLoader.globalWalls.Add(globalWall);
    }

    public GlobalWall GetGlobalWall(string name)
    {
        if (this.globalWalls.ContainsKey(name))
        {
            return globalWalls[name];
        }
        else
        {
            return null;
        }
    }

    private void AutoloadWall(Type type)
    {
        ModWall wall = (ModWall)Activator.CreateInstance(type);
        wall.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        if(wall.Autoload(ref name, ref texture))
        {
            AddWall(name, wall, texture);
        }
    }

    private void AutoloadGlobalWall(Type type)
    {
        GlobalWall globalWall = (GlobalWall)Activator.CreateInstance(type);
        globalWall.mod = this;
        string name = type.Name;
        if(globalWall.Autoload(ref name))
        {
            AddGlobalWall(name, globalWall);
        }
    }

    public void AddProjectile(string name, ModProjectile projectile, string texture)
    {
        int id = ProjectileLoader.ReserveProjectileID();
        projectile.projectile.name = name;
        projectile.Name = name;
        projectile.projectile.type = id;
        projectiles[name] = projectile;
        ProjectileLoader.projectiles[id] = projectile;
        projectile.texture = texture;
        projectile.mod = this;
    }

    public ModProjectile GetProjectile(string name)
    {
        if(projectiles.ContainsKey(name))
        {
            return projectiles[name];
        }
        else
        {
            return null;
        }
    }

    public int ProjectileType(string name)
    {
        ModProjectile projectile = GetProjectile(name);
        if(projectile == null)
        {
            return 0;
        }
        return projectile.projectile.type;
    }

    public void AddGlobalProjectile(string name, GlobalProjectile globalProjectile)
    {
        globalProjectile.mod = this;
        globalProjectile.Name = name;
        this.globalProjectiles[name] = globalProjectile;
        ProjectileLoader.globalProjectiles.Add(globalProjectile);
    }

    public GlobalProjectile GetGlobalProjectile(string name)
    {
        if(this.globalProjectiles.ContainsKey(name))
        {
            return this.globalProjectiles[name];
        }
        else
        {
            return null;
        }
    }

    private void AutoloadProjectile(Type type)
    {
        ModProjectile projectile = (ModProjectile)Activator.CreateInstance(type);
        projectile.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        if(projectile.Autoload(ref name, ref texture))
        {
            AddProjectile(name, projectile, texture);
        }
    }

    private void AutoloadGlobalProjectile(Type type)
    {
        GlobalProjectile globalProjectile = (GlobalProjectile)Activator.CreateInstance(type);
        globalProjectile.mod = this;
        string name = type.Name;
        if (globalProjectile.Autoload(ref name))
        {
            AddGlobalProjectile(name, globalProjectile);
        }
    }

    public void AddNPC(string name, ModNPC npc, string texture)
    {
        int id = NPCLoader.ReserveNPCID();
        npc.npc.name = name;
        npc.npc.type = id;
        npcs[name] = npc;
        NPCLoader.npcs[id] = npc;
        npc.texture = texture;
        npc.mod = this;
    }

    public ModNPC GetNPC(string name)
    {
        if (npcs.ContainsKey(name))
        {
            return npcs[name];
        }
        else
        {
            return null;
        }
    }

    public int NPCType(string name)
    {
        ModNPC npc = GetNPC(name);
        if (npc == null)
        {
            return 0;
        }
        return npc.npc.type;
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

    private void AutoloadNPC(Type type)
    {
        ModNPC npc = (ModNPC)Activator.CreateInstance(type);
        npc.mod = this;
        string name = type.Name;
        string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
        if(npc.Autoload(ref name, ref texture))
        {
            AddNPC(name, npc, texture);
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
        foreach(ModWall wall in walls.Values)
        {
            Main.wallTexture[wall.Type] = ModLoader.GetTexture(wall.texture);
            wall.SetDefaults();
        }
        foreach(GlobalWall globalWall in globalWalls.Values)
        {
            globalWall.SetDefaults();
        }
        foreach(ModProjectile projectile in projectiles.Values)
        {
            Main.projectileTexture[projectile.projectile.type] = ModLoader.GetTexture(projectile.texture);
            projectile.SetDefaults();
        }
        foreach(ModNPC npc in npcs.Values)
        {
            Main.npcTexture[npc.npc.type] = ModLoader.GetTexture(npc.texture);
            Main.npcName[npc.npc.type] = npc.npc.name;
            npc.SetDefaults();
            if(npc.npc.lifeMax > 32767 || npc.npc.boss)
            {
                Main.npcLifeBytes[npc.npc.type] = 4;
            }
            else if(npc.npc.lifeMax > 127)
            {
                Main.npcLifeBytes[npc.npc.type] = 2;
            }
            else
            {
                Main.npcLifeBytes[npc.npc.type] = 1;
            }
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
        walls.Clear();
        globalWalls.Clear();
        projectiles.Clear();
        globalProjectiles.Clear();
        npcs.Clear();
        globalNPCs.Clear();
    }
}}