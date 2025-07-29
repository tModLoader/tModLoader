using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Represents a builder toggle button shown in the top left corner of the screen while the inventory is shown. These toggles typically control wiring-related visibility or other building-related quality of life features.<para/>
/// The <see cref="Active"/> method determines if the BuilderToggle should be shown to the user and is usually reliant on player-specific values. The <see cref="CurrentState"/> property represents the current state of the toggle. For vanilla toggles a value of 0 is off and a value of 1 is on, but modded toggles can have <see cref="NumberOfStates"/> values.
/// </summary>
public abstract class BuilderToggle : ModTexturedType, ILocalizedModType
{
	public static BuilderToggle RulerLine { get; private set; } = new RulerLineBuilderToggle();
	public static BuilderToggle RulerGrid { get; private set; } = new RulerGridBuilderToggle();
	public static BuilderToggle AutoActuate { get; private set; } = new AutoActuateBuilderToggle();
	public static BuilderToggle AutoPaint { get; private set; } = new AutoPaintBuilderToggle();
	public static BuilderToggle RedWireVisibility { get; private set; } = new RedWireVisibilityBuilderToggle();
	public static BuilderToggle BlueWireVisibility { get; private set; } = new BlueWireVisibilityBuilderToggle();
	public static BuilderToggle GreenWireVisibility { get; private set; } = new GreenWireVisibilityBuilderToggle();
	public static BuilderToggle YellowWireVisibility { get; private set; } = new YellowWireVisibilityBuilderToggle();
	public static BuilderToggle HideAllWires { get; private set; } = new HideAllWiresBuilderToggle();
	public static BuilderToggle ActuatorsVisibility { get; private set; } = new ActuatorsVisibilityBuilderToggle();
	public static BuilderToggle BlockSwap { get; private set; } = new BlockSwapBuilderToggle();
	public static BuilderToggle TorchBiome { get; private set; } = new TorchBiomeBuilderToggle();

	/// <summary>
	/// The path to the texture vanilla info displays use when hovering over an info display.
	/// </summary>
	public static string VanillaHoverTexture => "Terraria/Images/UI/InfoIcon_13";

	/// <summary>
	/// The outline texture drawn when the icon is hovered. By default a circular outline texture is used. Override this method and return <c>Texture + "_Hover"</c> or any other texture path to specify a custom outline texture for use with icons that are not circular.
	/// </summary>
	public virtual string HoverTexture => VanillaHoverTexture;

	/// <summary>
	/// This is the internal ID of this builder toggle.<para/>
	/// Also serves as the index for <see cref="Player.builderAccStatus"/>.
	/// </summary>
	public int Type { get; internal set; }

	public virtual string LocalizationCategory => "BuilderToggles";

	/// <summary>
	/// This dictates whether or not this builder toggle should be active (displayed).<para/>
	/// This is usually determined by player-specific values, typically set in <see cref="ModItem.UpdateInventory"/>.
	/// </summary>
	public virtual bool Active() => false;

	/// <summary>
	/// This is the number of different functionalities your builder toggle will have.<br/>
	/// For a toggle that has an On and Off state, you'd need 2 states!<para/>
	/// </summary>
	/// <value>Default value is 2</value>
	public virtual int NumberOfStates { get; internal set; } = 2;

	/// <summary>
	/// Modify this if you want your builder toggle have custom ordering.
	/// You can specify which BuilderToggle to sort before/after
	/// </summary>
	public virtual Position OrderPosition { get; internal set; } = new Default();

	/// <summary>
	/// This is the current state of this builder toggle. Every time the toggle is clicked, it will change.<para/>
	/// The default state is 0. The state will be saved and loaded for the player to be consistent.
	/// </summary>
	public int CurrentState {
		get => Main.LocalPlayer.builderAccStatus[Type];
		set => Main.LocalPlayer.builderAccStatus[Type] = value;
	}

	/// <summary>
	/// This is the overlay color that is drawn on top of the texture.
	/// </summary>
	/// <value>Default value is <see cref="Color.White"/></value>
	[Obsolete("Use Draw instead", error: true)]
	public virtual Color DisplayColorTexture() => Color.White;
	[Obsolete]
	internal Color DisplayColorTexture_Obsolete() => DisplayColorTexture();

	/// <summary>
	/// This is the value that will show up when hovering on the toggle icon.
	/// You can specify different values per each available <see cref="CurrentState"/>
	/// </summary>
	public abstract string DisplayValue();

	/// <summary>
	/// This allows you to change basic drawing parameters or to override the vanilla drawing completely.<para/>
	/// This is for the icon itself. See <see cref="DrawHover"/> if you want to modify icon hover drawing.<para/>
	/// Return false to stop vanilla drawing code from running. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="drawParams">The draw parameters for the builder toggle icon</param>
	/// <returns>Whether to run vanilla icon drawing code</returns>
	public virtual bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) => true;

	/// <summary>
	/// This allows you to change basic drawing parameters or to override the vanilla drawing completely.<para/>
	/// This is for the icon hover. See <see cref="Draw"/> if you want to modify icon drawing.<para/>
	/// Return false to stop vanilla drawing code from running. Returns true by default.
	/// </summary>
	/// <param name="spriteBatch">The spritebatch to draw on</param>
	/// <param name="drawParams">The draw parameters for the builder toggle hover icon</param>
	/// <returns>Whether to run vanilla icon hover drawing code</returns>
	public virtual bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) => true;

	/// <summary>
	/// Called when the toggle is left clicked and before vanilla operation takes place.<para/>
	/// Return false to stop vanilla left click code (switching between states and playing sound) from running.<br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="sound">The click sound that will be played. Return null to mute.</param>
	/// <returns>Whether to run vanilla click code</returns>
	public virtual bool OnLeftClick(ref SoundStyle? sound) => true;

	/// <summary>
	/// Called when the toggle is right clicked.<br/>
	/// Use this if you want to implement special right click feature (such as cycling through states backwards).
	/// </summary>
	public virtual void OnRightClick() { }

	public sealed override void SetupContent()
	{
		ModContent.Request<Texture2D>(Texture);
		ModContent.Request<Texture2D>(HoverTexture);
		SetStaticDefaults();
	}

	protected override void Register()
	{
		ModTypeLookup<BuilderToggle>.Register(this);
		Type = BuilderToggleLoader.Add(this);
	}

	#region Sort Positions

	public abstract class Position { }

	public sealed class Default : Position { }

	public sealed class Before : Position
	{
		public BuilderToggle Toggle { get; }

		public Before(BuilderToggle toggle)
		{
			Toggle = toggle;
		}
	}

	public sealed class After : Position
	{
		public BuilderToggle Toggle { get; }

		public After(BuilderToggle toggle)
		{
			Toggle = toggle;
		}
	}

	#endregion
}