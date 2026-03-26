using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to customize the behavior of a custom gore.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModGore : ModTexturedType
{
	internal string nameOverride;
	internal string textureOverride;

	/// <summary> Allows you to copy the Update behavior of a different type of gore. This defaults to 0, which means no behavior is copied. </summary>
	public int UpdateType { get; set; } = -1;

	public int Type { get; internal set; }

	public override string Name => nameOverride ?? base.Name;
	public override string Texture => textureOverride ?? base.Texture;

	protected override void Register()
	{
		ModTypeLookup<ModGore>.Register(this);
		GoreLoader.RegisterModGore(this);
		GoreID.Search.Add(FullName, Type);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to modify a gore's fields when it is created.
	/// </summary>
	public virtual void OnSpawn(Gore gore, IEntitySource source) { }

	//in Terraria.Gore.Update at beginning of if block checking for active add
	//  if(this.modGore != null && !this.modGore.Update(this)) { return; }
	/// <summary>
	/// Allows you to customize how you want this type of gore to behave. Return true to allow for vanilla gore updating to also take place; returns true by default.
	/// </summary>
	public virtual bool Update(Gore gore) => true;

	//at beginning of Terraria.Gore.Update add
	//  if(this.modGore != null) { Color? modColor = this.modGore.GetAlpha(this, newColor);
	//    if(modColor.HasValue) { return modColor.Value; } }
	/// <summary>
	/// Allows you to override the color this gore will draw in. Return null to draw it in the normal light color; returns null by default.
	/// </summary>
	/// <returns></returns>
	public virtual Color? GetAlpha(Gore gore, Color lightColor) => null;
}
