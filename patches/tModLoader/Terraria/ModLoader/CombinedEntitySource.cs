using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public class CombinedEntitySource : IEntitySource
{
    public string Context => "tModLoader_CombinedEntitySource";
    List<IEntitySource> entitySources = new();

    public CombinedEntitySource AddEntitySource(IEntitySource second)
    {
        entitySources.Add(second);
        return this;
    }

    public CombinedEntitySource(IEntitySource first, IEntitySource second)
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

public static class CombinedEntitySourceExtension
{
    public static CombinedEntitySource AddEntitySource(this IEntitySource first, IEntitySource second)
    {
        return new CombinedEntitySource(first, second);
    }

    public static T AttemptGet<T>(this IEntitySource collection) where T : class, IEntitySource
    {
        if(collection is CombinedEntitySource)
            return ((CombinedEntitySource)collection).AttemptGet<T>();
        return null;
    }
}
