using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

public class CombinedEntitySource : IEntitySource
{
    public string Context => "tModLoader_CombinedEntitySource";
    LinkedList<IEntitySource> entitySources = new();

    public CombinedEntitySource AddEntitySource(IEntitySource second)
    {
        entitySources.AddFirst(second);
        return this;
    }

    public CombinedEntitySource(IEntitySource first, IEntitySource second)
    {
        entitySources.AddFirst(first);
        entitySources.AddFirst(second);
    }
    public T AttemptGet<T>() where T : class, IEntitySource
    {
        IEntitySource rv;
        for (LinkedListNode<IEntitySource> node = entitySources.First; node != null; node = node.Next)
        {
            if(node.Value is T)
            return node.Value as T;
        }
        return null;
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
        if (collection is T)
            return (T)collection;
        if (collection is CombinedEntitySource)
            return ((CombinedEntitySource)collection).AttemptGet<T>();
        return null;
    }
}
