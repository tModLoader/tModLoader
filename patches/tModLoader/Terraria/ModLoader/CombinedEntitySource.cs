using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public class CombinedEntitySource : IEntitySource
{
    public string Context => "tModLoader_CombinedEntitySource";
    List<IEntitySource> entitySources = new();

    public EntitySourceTest AddEntitySource(IEntitySource second)
    {
        entitySources.Add(second);
        return this;
    }

    public EntitySourceTest(IEntitySource first, IEntitySource second)
    {
        entitySources.Add(first);
        entitySources.Add(second);
    }
    public T AttemptGet<T>() where T : class, IEntitySource
    {
        IEntitySource rv = entitySources.Find(i => i is T);
        return rv as T;
    }
}

public static class EntitySourceTestExtension
{
    public static EntitySourceTest AddEntitySource(this IEntitySource first, IEntitySource second)
    {
        return new EntitySourceTest(first, second);
    }

    public static T AttemptGet<T>(this IEntitySource collection) where T : class, IEntitySource
    {
        if(collection is EntitySourceTest)
            return ((EntitySourceTest)collection).AttemptGet<T>();
        return null;
    }
}
