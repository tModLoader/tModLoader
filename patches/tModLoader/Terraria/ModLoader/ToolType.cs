using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as a place for you to program behaviors for tools, specifically those affecting tiles in any way. This is useful for making custom tools that mine any block or wall, mine blocks in an area, or change blocks into other blocks.
	/// </summary>
	public abstract class ToolType : ModType
	{
		/// <summary>
		/// Mines soft blocks, specifically those set in the `TileID.Sets.CanBeDugByShovel` array. Mines in a 3x3 area, and bypasses the extra hit needed to mine grass.
		/// It is the first Vanilla ToolType to run its code. 
		/// Has no usage condition, so if it isn't stopped by mods, it will stop all other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType Shovel { get; private set; } = new ShovelToolType();

		/// <summary>
		/// Mines almost any block, only excluding those only axes and hammers can mine.
		/// It is the fourth Vanilla ToolType to run its code, and the last of the block mining ones.
		/// Has an usage condition, and if it's met and isn't stopped by mods, it will stop all other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType Pickaxe { get; private set; } = new PickaxeToolType();

		/// <summary>
		/// Mines trees, cacti, and any other tile type that has the `Main.tileAxe` flag.
		/// It is the third Vanilla ToolType to run its code.
		/// Has an usage condition, and if it's met and isn't stopped by mods, it will stop all other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType Axe { get; private set; } = new AxeToolType();

		/// <summary>
		/// Mines Demon/Crimson Altars, Shadow Orbs/Crimson Hearts, and any other tile type that has the `Main.tileHammer` flag.
		/// It is the second Vanilla ToolType to run its code.
		/// Has an usage condition, and if it's met and isn't stopped by mods, it will stop all other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType HammerBlock { get; private set; } = new HammerBlockToolType();

		/// <summary>
		/// Changes the shape of solid blocks into slopes or half-blocks. Has special behavior for specific tiles.
		/// It is the fifth Vanilla ToolType to run its code. 
		/// Has an usage condition, and if it's met and isn't stopped by mods, it will stop all other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType HammerSlope { get; private set; } = new HammerSlopeToolType();

		/// <summary>
		/// Mines any type of wall, assuming it's a player-placed one, or there's at least 1 missing or player-placed wall in a 1 tile radius surrounding it.
		/// It is the sixth and last Vanilla ToolType to run its code. 
		/// Has an usage condition, but even if it's met, it won't stop other ToolTypes after it from executing their code.
		/// </summary>
		public static ToolType HammerWall { get; private set; } = new HammerWallToolType();

		/// <summary>
		/// This is the internal ID of this ToolType.
		/// </summary>
		public int Type { get; internal set; }

		/// <summary>
		/// This is the translation that is used behind <see cref="DisplayName"/>. The translation will show up when an item tooltip displays 'X% [ToolName]'. This should include the 'power' part.
		/// </summary>
		public ModTranslation ToolName { get; internal set; }

		/// <summary>
		/// This is the name that will show up when an item tooltip displays 'X% [ToolName]'. This should include the 'power' part. 
		/// </summary>
		public string DisplayName => DisplayNameInternal;

		internal protected virtual string DisplayNameInternal => ToolName.GetTranslation(Language.ActiveCulture);

		/// A collection of all ToolTypeIDs this ToolType provides power to when present.
		public ReadOnlyCollection<int> ProvidesPowerTo { get; private set; } = new ReadOnlyCollection<int>(new List<int>());

		/// <summary>
		/// Whether or not to display 'X% [ToolName]' in the item's tooltip. Defaults to true.
		/// </summary>
		public virtual bool ShowsOnTooltip => true;

		/// <summary>
		/// The identifier of this ToolType's tooltip. Defaults to "[ModName].[ToolTypeClassName]Power".
		/// </summary>
		public virtual string TooltipName => Mod.Name + "." + Name + "Power";
		
		/// <summary>
		/// The amount to multiply this ToolType's power by on the item's tooltip. Defaults to 1.
		/// </summary>
		public virtual float TooltipToolPowerMultiplier => 1f;

		/// <summary>
		/// The priority of this ToolType for using it in during Vanilla's mining code execution. Defaults to ToolPriority.Last.
		/// </summary>
		public virtual ToolPriority Priority => ToolPriority.Last;

		/// <summary>
		/// The ToolType this ToolType can use the tool power from. Defaults to null (None).
		/// </summary>
		public virtual ToolType? PowerProvider => null;

		/// <summary>
		/// Whether this ToolType can affect blocks. Defaults to false.
		/// </summary>
		public virtual bool AffectsBlocks => false;
		
		/// <summary>
		/// Whether this ToolType can affect walls. Defaults to false.
		/// </summary>
		public virtual bool AffectsWalls => false;

		/// <summary>
		/// Allows deciding in which conditions this ToolType can be used.
		/// </summary>
		/// <param name="player"> The player using the item. </param>
		/// <param name="item"> The item being used. </param>
		/// <param name="tile"> The tile being targeted. </param>
		/// <param name="x"> The x coordinate of the target tile. </param>
		/// <param name="y"> The y coordinate of the target tile. </param>
		/// <returns> Whether this ToolType can be used at given tile coordinates. Returns true by default. </returns>
		public virtual bool CanUseTool(Player player, Item item, Tile tile, int x, int y) => true;

		/// <summary>
		/// The effect this ToolType has, whether it is mining or something else, on the tile at the given coordinates.
		/// </summary>
		/// <param name="player"> The player using the item. </param>
		/// <param name="item"> The item being used. </param>
		/// <param name="tile"> The tile being targeted. </param>
		/// <param name="x"> The x coordinate of the target tile. </param>
		/// <param name="y"> The y coordinate of the target tile. </param>
		/// <param name="power">The power this ToolType will use. </param>
		/// <returns> Whether this ToolType was used successfully, and should prevent other ToolTypes after it from executing their code. Returns false by default. </returns>
		public virtual bool UseTool(Player player, Item item, Tile tile, int x, int y, int power) => false;

		internal void RebuildProvidesPowerCache() {
			var receivers = new List<int>();
			foreach (var tool in ToolTypeLoader.ToolTypes) {
				if (tool != this && tool.PowerProvider == this)
					receivers.Add(tool.Type);
			}

			ProvidesPowerTo = receivers.AsReadOnly();
		}

		protected override void Register() {
			ToolName = Mod.GetOrCreateTranslation($"Mods.{Mod.Name}.ToolTypeName.{Name}");

			ModTypeLookup<ToolType>.Register(this);

			Type = ToolTypeLoader.Add(this);
		}
	}
}
