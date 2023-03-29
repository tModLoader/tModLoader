namespace Terraria.ModLoader;

public interface IEntityWithInstances<T> where T : IIndexed
{
	RefReadOnlyArray<T> Instances { get; }
}
