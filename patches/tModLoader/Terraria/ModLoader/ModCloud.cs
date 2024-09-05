using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to customize the behavior of a custom cloud.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModCloud : ModTexturedType
{
	internal string nameOverride;
	internal string textureOverride;

	public int Type { get; internal set; }

	public override string Name => nameOverride ?? base.Name;
	public override string Texture => textureOverride ?? base.Texture;

	protected override void Register()
	{
		ModTypeLookup<ModCloud>.Register(this);
		CloudLoader.RegisterModCloud(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Whether or not this cloud can spawn with the given spawning conditions. Return 1 to spawn just as often compared to vanilla.
	/// </summary>
	/// <param name="cloudIndex">The index of the cloud in Main.cloud.</param>
	/// <returns></returns>
	public virtual float SpawnChance(int cloudIndex) => 0f;

	/// <summary>
	/// Return <c>true</c> to draw vanilla, return <c>false</c> to do otherwise. This hook is called before vanilla clouds are drawn.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="position">The center of the sprite.</param>
	/// <param name="color"></param>
	/// <param name="cloudIndex">The index of the cloud in Main.cloud.</param>
	/// <returns></returns>
	public virtual bool Draw(SpriteBatch spriteBatch, Vector2 position, ref Color color, int cloudIndex) => true;
	
}
