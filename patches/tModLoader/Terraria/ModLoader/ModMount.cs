using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a place for you to place all your properties and hooks for each mount.
/// <br/> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// <para/> Only one instance of ModMount will exist for each mount, so storing player specific data on the ModMount is not good.
/// Modders can use <i>player.mount._mountSpecificData</i> or a ModPlayer class to store player specific data relating to a mount. Use SetMount to assign these fields.
/// <para/> Note that texture autoloading is unique for ModMount, see <see cref="Texture"/> for more information.
/// </summary>
public abstract class ModMount : ModType<Mount.MountData, ModMount>
{
	/// <summary>
	/// The vanilla MountData object that is controlled by this ModMount.
	/// </summary>
	public Mount.MountData MountData => Entity;

	/// <summary>
	/// The index of this ModMount in the Mount.mounts array.
	/// </summary>
	public int Type { get; internal set; }

	/// <summary>
	/// The file name of this type's texture file in the mod loader's file space.
	/// <para/> For mounts, this path isn't used directly to autoload a texture, but it is combined with <see cref="MountTextureType"/> values to autoload textures for each of the different layers that mounts support, if that texture exists.
	/// <para/> ModMount typically want at least one of either a "Back" or "Front" texture. For example, a mount named "MyMount" would need to include a "MyMount_Back.png" or "MyMount_Front.png" texture.
	/// </summary>
	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');//GetType().FullName.Replace('.', '/');

	protected override Mount.MountData CreateTemplateEntity() => new() { ModMount = this };

	protected sealed override void Register()
	{
		if (Mount.mounts == null || Mount.mounts.Length == MountID.Count)
			Mount.Initialize();

		Type = MountLoader.ReserveMountID();
		MountID.Search.Add(FullName, Type);

		ModTypeLookup<ModMount>.Register(this);
		MountLoader.mountDatas[Type] = this;

		foreach (MountTextureType textureType in Enum.GetValues(typeof(MountTextureType))) {
			string extraTexture = GetExtraTexture(textureType);

			if (string.IsNullOrEmpty(extraTexture) || !ModContent.RequestIfExists<Texture2D>(extraTexture, out var textureAsset)) {
				continue;
			}

			switch (textureType) {
				case MountTextureType.Back:
					MountData.backTexture = textureAsset;
					break;
				case MountTextureType.BackGlow:
					MountData.backTextureGlow = textureAsset;
					break;
				case MountTextureType.BackExtra:
					MountData.backTextureExtra = textureAsset;
					break;
				case MountTextureType.BackExtraGlow:
					MountData.backTextureExtraGlow = textureAsset;
					break;
				case MountTextureType.Front:
					MountData.frontTexture = textureAsset;
					break;
				case MountTextureType.FrontGlow:
					MountData.frontTextureGlow = textureAsset;
					break;
				case MountTextureType.FrontExtra:
					MountData.frontTextureExtra = textureAsset;
					break;
				case MountTextureType.FrontExtraGlow:
					MountData.frontTextureExtraGlow = textureAsset;
					break;
			}
		}
	}

	public sealed override void SetupContent()
	{
		Mount.mounts[Type] = MountData;
		SetStaticDefaults();
	}

	protected virtual string GetExtraTexture(MountTextureType textureType) => Texture + "_" + textureType;

	public override ModMount NewInstance(Mount.MountData entity)
	{
		ModMount inst = base.NewInstance(entity);
		inst.Type = Type;
		return inst;
	}

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// This is where you would set properties of this type of mount.
	/// </summary>
	public override void SetStaticDefaults() { }

	/// <summary>
	/// Allows you to modify the mount's jump height based on its state.
	/// </summary>
	/// <param name="mountedPlayer"></param>
	/// <param name="jumpHeight"></param>
	/// <param name="xVelocity"></param>
	public virtual void JumpHeight(Player mountedPlayer, ref int jumpHeight, float xVelocity)
	{
	}

	/// <summary>
	/// Allows you to modify the mount's jump speed based on its state.
	/// </summary>
	/// <param name="mountedPlayer"></param>
	/// <param name="jumpSeed"></param>
	/// <param name="xVelocity"></param>
	public virtual void JumpSpeed(Player mountedPlayer, ref float jumpSeed, float xVelocity)
	{
	}

	/// <summary>
	/// Allows you to make things happen when mount is used (creating dust etc.) Can also be used for mount special abilities.
	/// </summary>
	/// <param name="player"></param>
	public virtual void UpdateEffects(Player player)
	{
	}

	/// <summary>
	/// Allows for manual updating of mount frame. Return false to stop the default frame behavior. Returns true by default.
	/// <para/>Possible values for <paramref name="state"/> include:
	/// <br/> 0. Standing still on the ground or sliding
	/// <br/> 1. Moving on the ground
	/// <br/> 2. In the air, not flying. Hovering counts as this as well.
	/// <br/> 3. In the air, flying
	/// <br/> 4. Flying in water
	/// </summary>
	/// <param name="mountedPlayer"></param>
	/// <param name="state"></param>
	/// <param name="velocity"></param>
	/// <returns></returns>
	public virtual bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
	{
		return true;
	}

	//todo: MountLoader is never called for this, why is this in here? Made it internal for now
	internal virtual bool CustomBodyFrame()
	{
		return false;
	}

	/// <summary>
	/// Allows you to make things happen while the mouse is pressed while the mount is active. Called each tick the mouse is pressed.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="mousePosition"></param>
	/// <param name="toggleOn">Does nothing yet</param>
	public virtual void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
	{
	}

	/// <summary>
	/// Allows you to make things happen when the mount ability is aiming (while charging).
	/// </summary>
	/// <param name="player"></param>
	/// <param name="mousePosition"></param>
	public virtual void AimAbility(Player player, Vector2 mousePosition)
	{
	}

	/// <summary>
	/// Allows you to make things happen when this mount is spawned in. Useful for player-specific initialization, utilizing player.mount._mountSpecificData or a ModPlayer class since ModMount is shared between all players.
	/// Custom dust spawning logic is also possible via the skipDust parameter.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="skipDust">Set to true to skip the vanilla dust spawning logic</param>
	public virtual void SetMount(Player player, ref bool skipDust)
	{
	}

	/// <summary>
	/// Allows you to make things happen when this mount is de-spawned. Useful for player-specific cleanup, see SetMount.
	/// Custom dust spawning logic is also possible via the skipDust parameter.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="skipDust">Set to true to skip the vanilla dust spawning logic</param>
	public virtual void Dismount(Player player, ref bool skipDust)
	{
	}

	/// <summary>
	/// Allows for complete customization of mount drawing. This method will be called once for each supported mount texture layer that exists. Use drawType to conditionally apply changes.
	/// drawType corresponds to the following: 0: backTexture, 1: backTextureExtra, 2: frontTexture. 3: frontTextureExtra
	/// Corresponding glow textures, such as backTextureGlow, are paired with their corresponding texture and passed into this method as well.
	/// Return false if you are manually adding DrawData to playerDrawData to replace the vanilla draw behavior, otherwise tweak ref variables to customize the drawing and add additional DrawData to playerDrawData.
	/// </summary>
	/// <param name="playerDrawData"></param>
	/// <param name="drawType">Corresponds to the following: 0: backTexture, 1: backTextureExtra, 2: frontTexture. 3: frontTextureExtra</param>
	/// <param name="drawPlayer"></param>
	/// <param name="texture"></param>
	/// <param name="glowTexture">The corresponding glow texture, if present</param>
	/// <param name="drawPosition"></param>
	/// <param name="frame"></param>
	/// <param name="drawColor"></param>
	/// <param name="glowColor"></param>
	/// <param name="rotation"></param>
	/// <param name="spriteEffects"></param>
	/// <param name="drawOrigin"></param>
	/// <param name="drawScale"></param>
	/// <param name="shadow"></param>
	/// <returns></returns>
	public virtual bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
	{
		return true;
	}
}
