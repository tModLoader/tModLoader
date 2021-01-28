
namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // Need two named versions
	public class UnloadedWall : ModWall
	{
		public override string Name { get; }

		public override string Texture => "ModLoader/UnloadedWall";

		public UnloadedWall(string name = null) {
			Name = name ?? base.Name;
		}
	}
}