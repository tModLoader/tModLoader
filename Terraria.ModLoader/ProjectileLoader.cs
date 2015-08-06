using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader {
public static class ProjectileLoader
{
    private static int nextProjectile = ProjectileID.Count;
    internal static readonly IDictionary<int, ModProjectile> projectiles = new Dictionary<int, ModProjectile>();
    internal static readonly IList<GlobalProjectile> globalProjectiles = new List<GlobalProjectile>();

    internal static int ReserveProjectileID()
    {
        int reserveID = nextProjectile;
        nextProjectile++;
        return reserveID;
    }

    public static ModProjectile GetProjectile(int type)
    {
        if(projectiles.ContainsKey(type))
        {
            return projectiles[type];
        }
        else
        {
            return null;
        }
    }

    internal static void ResizeArrays()
    {
        Array.Resize(ref Main.projectileLoaded, nextProjectile);
        Array.Resize(ref Main.projectileTexture, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.TrailingMode, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.TrailCacheLength, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.LightPet, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.Homing, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.MinionSacrificable, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.DontAttachHideToAlpha, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.NeedsUUID, nextProjectile);
        Array.Resize(ref ProjectileID.Sets.StardustDragon, nextProjectile);
        for(int k = ProjectileID.Count; k < nextProjectile; k++)
        {
            Main.projectileLoaded[k] = true;
            ProjectileID.Sets.TrailingMode[k] = -1;
            ProjectileID.Sets.TrailCacheLength[k] = 10;
        }
    }

    internal static void Unload()
    {
        projectiles.Clear();
        nextProjectile = ProjectileID.Count;
        globalProjectiles.Clear();
    }

    internal static bool IsModProjectile(Projectile projectile)
    {
        return projectile.type >= ProjectileID.Count;
    }

    //in Terraria.Projectile.SetDefaults get rid of bad type check
    //in Terraria.Projectile.SetDefaults before scaling size call ProjectileLoader.SetupProjectile(this);
    internal static void SetupProjectile(Projectile projectile)
    {
        if(IsModProjectile(projectile))
        {
            GetProjectile(projectile.type).SetupProjectile(projectile);
        }
        foreach(GlobalProjectile globalProjectile in globalProjectiles)
        {
            globalProjectile.SetDefaults(projectile);
        }
    }
}}
