namespace Terraria.ModLoader;

public interface IEntityWithGlobals<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	RefReadOnlyArray<TGlobal> EntityGlobals { get; }
}
