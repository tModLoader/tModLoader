
namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need multiple versions, all subclassed
	public class UnloadedWall : ModWall
	{
		public override string Texture => "ModLoader/UnloadedWall";
	}
}