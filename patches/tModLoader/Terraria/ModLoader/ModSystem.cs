namespace Terraria.ModLoader
{
	/// <summary>
	/// ModSystem is an abstract class that your classes can derive from. It contains general-use hooks, and, unlike Mod, can have unlimited amounts of types deriving from it. 
	/// </summary>
	public abstract partial class ModSystem : ModType
	{
		protected override void Register() {
			Mod.systems[Name] = this;

			SystemHooks.Add(this);
			ContentInstance.Register(this);
		}
	}
}
