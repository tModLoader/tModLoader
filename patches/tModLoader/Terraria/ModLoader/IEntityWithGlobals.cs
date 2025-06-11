namespace Terraria.ModLoader;

public interface IEntityWithGlobals<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	int Type { get; }
	RefReadOnlyArray<TGlobal> EntityGlobals { get; }
}
