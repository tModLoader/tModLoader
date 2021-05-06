namespace Terraria.ModLoader
{
	public interface IEntityWithGlobals<T> where T : GlobalType
	{
		RefReadOnlyArray<Instanced<T>> Globals { get; }
	}
}
