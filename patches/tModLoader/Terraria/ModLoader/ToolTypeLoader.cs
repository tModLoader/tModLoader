using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader
{
	public class ToolTypeLoader
	{
		public static int ToolTypeCount => ToolTypes.Count;

		internal static readonly List<ToolType> ToolTypes = new List<ToolType>() {
			ToolType.Shovel,
			ToolType.Pickaxe,
			ToolType.Axe,
			ToolType.HammerBlock,
			ToolType.HammerSlope,
			ToolType.HammerWall
		};

		private static readonly int DefaultToolCount = ToolTypes.Count;

		static ToolTypeLoader() {
			RegisterDefaultTools();
			ResizeArrays();
		}

		internal static void ResizeArrays() {
			foreach (var tt in ToolTypes)
				tt.RebuildProvidesPowerCache();
		}

		internal static int Add(ToolType toolType) {
			ToolTypes.Add(toolType);
			return ToolTypes.Count - 1;
		}

		internal static void Unload() {
			ToolTypes.RemoveRange(DefaultToolCount, ToolTypes.Count - DefaultToolCount);
		}

		internal static void RegisterDefaultTools() {
			int i = 0;
			foreach (var toolType in ToolTypes) {
				toolType.Type = i++;
				ContentInstance.Register(toolType);
				ModTypeLookup<ToolType>.Register(toolType);
			}
		}

		public static bool IsModToolType(ToolType toolType) => toolType.Type >= DefaultToolCount;

		/// <summary>
		/// Calls CombinedHooks.CanUseTool, then ToolType.CanUseTool if the previous method returns null.
		/// </summary>
		public static bool CanUseTool(ToolType toolType, Player player, Item item, Tile tile, int x, int y) {
			bool? canUse = CombinedHooks.CanUseTool(player, item, toolType, tile, x, y);
			return canUse == true || canUse != false && toolType.CanUseTool(player, item, tile, x, y);
		}

		/// <summary>
		/// Goes through all Modded ToolTypes of a specific ToolPriority that an item has, if any, then calls ToolTypeLoader.CanUseTool and ToolType.UseTool for each of them.
		/// </summary>
		public static bool TryUseTool(Player player, Item item, ToolPriority priority, Tile tile, int x, int y) {
			if (item.IsAir)
				return false;

			foreach (var toolType in item.ToolPower.GetToolTypes(priority)) {
				if (!IsModToolType(toolType))
					continue;

				if (!CanUseTool(toolType, player, item, tile, x, y))
					continue;

				if (toolType.UseTool(player, item, tile, x, y, item.ToolPower[toolType])) {
					player.ApplyItemTime(item);
					return true;
				}
			}
			return false;
		}
	}
}
