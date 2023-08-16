using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public abstract class BuilderToggle : ModTexturedType
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
		/// This is the internal ID of this builder toggle.
		/// </summary>
		public int Type { get; internal set; }

		/// <summary>
		/// This dictates whether or not this builder toggle should be active (displayed).
		/// </summary>
		public virtual bool Active() => false;

		/// <summary>
		/// This is the number of different functionalities your builder toggle will have.
		/// For a toggle that has an On and Off state, you'd need 2 states!
		/// </summary>
		/// <value>Default value is 2</value>
		public virtual int NumberOfStates { get; internal set; } = 2;

		/// <summary>
		/// This is the current state of this builder toggle. Every time the toggle is clicked, it will change.
		/// </summary>
		public int CurrentState => Main.player[Main.myPlayer].builderAccStatus[Type];

		/// <summary>
		/// This is the overlay color that is drawn on top of the texture.
		/// </summary>
		/// <value>Default value is <see cref="Color.White"/></value>
		public virtual Color DisplayColorTexture() => Color.White;

		/// <summary>
		/// This is the value that will show up when hovering on the toggle icon.
		/// You can specify different values per each available <see cref="CurrentState"/>
		/// </summary>
		public abstract string DisplayValue();

		public sealed override void SetupContent() {
			ModContent.Request<Texture2D>(Texture);
			SetDefaults();
		}

		/// <summary>
		/// You can assign values to the BuilderToggle here.
		/// </summary>
		public virtual void SetDefaults() { }

		protected override void Register() {
			ModTypeLookup<BuilderToggle>.Register(this);
			Type = BuilderToggleLoader.Add(this);
		}
	}
}