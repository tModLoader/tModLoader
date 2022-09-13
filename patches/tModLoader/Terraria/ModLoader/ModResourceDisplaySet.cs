﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Text.RegularExpressions;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to define your own logic for drawing the player's life and mana resources.<br/>
	/// For modifying parts of the vanilla display sets, use <see cref="ModResourceOverlay"/>.
	/// </summary>
	public abstract class ModResourceDisplaySet : ModType, IPlayerResourcesDisplaySet, IConfigKeyHolder
	{
		public int Type { get; internal set; }

		public bool Selected => Main.ResourceSetsManager.ActiveSet == this;

		/// <summary>
		/// Gets the name for this resource display set based on its DisplayName and the current culture
		/// </summary>
		public string DisplayedName => DisplayName.GetTranslation(Language.ActiveCulture);

		/// <summary>
		/// Included only for completion's sake.  Returns DisplayName.Key
		/// </summary>
		public string NameKey => DisplayName.Key;

		/// <summary>
		/// The name used to get this resource display set.  Returns <see cref="ModType.FullName"/>
		/// </summary>
		public string ConfigKey => FullName;

		/// <summary>
		/// The translations for the display name of this item.
		/// </summary>
		public ModTranslation DisplayName { get; internal set; }

		/// <summary>
		/// The current snapshot of the life and mana stats for Main.LocalPlayer
		/// </summary>
		public static PlayerStatsSnapshot PlayerStats => new PlayerStatsSnapshot(Main.LocalPlayer);

		protected sealed override void Register() {
			ModTypeLookup<ModResourceDisplaySet>.Register(this);

			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"ResourceDisplaySet.{Name}");

			Type = ResourceDisplaySetLoader.Add(this);
		}

		public sealed override void SetupContent() {
			AutoStaticDefaults();
			SetStaticDefaults();
		}

		/// <summary>
		/// Automatically sets certain static defaults. Override this if you do not want the properties to be set for you.
		/// </summary>
		public virtual void AutoStaticDefaults() {
			if (DisplayName.IsDefault())
				DisplayName.SetDefault(Regex.Replace(DisplayedName, "([A-Z])", " $1").Trim());
		}

		public void Draw() {
			var stats = PlayerStats;
			PreDrawResources(stats);

			Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			if (ResourceOverlayLoader.PreDrawResourceDisplay(stats, this, true, ref color, out bool drawText))
				DrawLife(Main.spriteBatch);
			ResourceOverlayLoader.PostDrawResourceDisplay(stats, this, true, color, drawText);

			color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
			if (ResourceOverlayLoader.PreDrawResourceDisplay(stats, this, false, ref color, out drawText))
				DrawMana(Main.spriteBatch);
			ResourceOverlayLoader.PostDrawResourceDisplay(stats, this, false, color, drawText);
		}

		/// <summary>
		/// Allows you to initialize fields, textures, etc. before drawing occurs
		/// </summary>
		/// <param name="snapshot">A copy of <see cref="PlayerStats"/></param>
		public virtual void PreDrawResources(PlayerStatsSnapshot snapshot) { }

		/// <summary>
		/// Draw the life resources for your display set here
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawLife(SpriteBatch spriteBatch) { }

		/// <summary>
		/// Draw the mana resources for your display set here
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawMana(SpriteBatch spriteBatch) { }

		public void TryToHover() {
			if (PreHover(out bool hoveringLife)) {
				if (hoveringLife)
					CommonResourceBarMethods.DrawLifeMouseOver();
				else
					CommonResourceBarMethods.DrawManaMouseOver();
			}
		}

		/// <summary>
		/// Allows you to specify if the vanilla life/mana hover text should display
		/// </summary>
		/// <param name="hoveringLife">Whether the hover text should be for life (<see langword="true"/>) or mana (<see langword="false"/>)</param>
		public virtual bool PreHover(out bool hoveringLife) {
			hoveringLife = false;
			return false;
		}
	}
}
