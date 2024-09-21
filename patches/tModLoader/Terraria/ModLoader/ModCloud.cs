using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to customize the behavior of a custom cloud.
/// <para/> Modded clouds can be autoloaded automatically (see <see cref="Mod.CloudAutoloadingEnabled"/>) or manually registered (<see cref="CloudLoader.AddCloudFromTexture"/>), but autoloaded clouds default to being normal clouds with the default spawn chance. Make a ModCloud class if custom behavior is needed or use <see cref="CloudLoader.AddCloudFromTexture"/> if just customizing cloud category and spawn chance is needed.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModCloud : ModTexturedType
{
	internal string nameOverride;
	internal string textureOverride;
	internal float spawnChance = 1f;
	internal bool rareCloud = false;

	public int Type { get; internal set; }

	/// <summary>
	/// If true, this cloud will belong to the "Rare clouds" pool instead of the "Normal clouds" (see <see href="https://terraria.wiki.gg/wiki/Ambient_entities#List_of_clouds">the Terraria wiki for more information</see>). Rare clouds typically can only spawn after certain world conditions have been met. For example <see cref="ID.CloudID.Rare_Skeletron"/> can only spawn if <see cref="NPC.downedBoss3"/> is true. Rare clouds can be used by mods to highlight achievements such as defeating a boss.
	/// <para/> Defaults to false.
	/// </summary>
	public virtual bool RareCloud => rareCloud;

	public override string Name => nameOverride ?? base.Name;
	public override string Texture => textureOverride ?? base.Texture;

	protected override void Register()
	{
		ModTypeLookup<ModCloud>.Register(this);
		CloudLoader.RegisterModCloud(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// The chance that this cloud can spawn. Return 1f to spawn just as often compared as vanilla clouds.
	/// <para/> A cloud can be either a "Normal cloud" or a "Rare cloud" if <see cref="RareCloud"/> is true. The spawn chance is the chance to spawn within each cloud category, so there is no need to return a very small chance value for rare clouds, the logic already takes that into account. If <see cref="RareCloud"/> is true, the spawn chance already takes into account secret seed adjustments so there is no need to implement that logic in this method either.
	/// <para/> See <see href="https://terraria.wiki.gg/wiki/Ambient_entities#List_of_clouds">the Terraria wiki for more information</see>.
	/// <para/> Defaults to 1f.
	/// </summary>
	public virtual float SpawnChance() => spawnChance;

	/// <summary>
	/// Gets called when the Cloud spawns.
	/// </summary>
	public virtual void OnSpawn(Cloud cloud) { }

	/// <summary>
	/// Return <c>true</c> to draw using vanilla drawing logic. Return <c>false</c> to prevent vanilla drawing logic and use this hook to draw the cloud manually.
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="position">The center of the sprite.</param>
	/// <param name="color"></param>
	/// <param name="cloudIndex">The index of the cloud in Main.cloud.</param>
	/// <returns></returns>
	public virtual bool Draw(SpriteBatch spriteBatch, Vector2 position, ref Color color, int cloudIndex) => true;
}
