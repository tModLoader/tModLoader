namespace Terraria.ModLoader;

public interface IEntityWithInstances<T>
{
	RefReadOnlyArray<T> Instances { get; }
}
