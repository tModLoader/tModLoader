using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable
	{
		public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

		public ModItem ModItem { get; internal set; }

		internal Instanced<GlobalItem>[] globalItems = new Instanced<GlobalItem>[0];

		private DamageClass _damageClass = DamageClass.Generic;
		/// <summary>
		/// The damage type of this Item. Assign to DamageClass.Melee/Ranged/Magic/Summon/Throwing for vanilla classes, or ModContent.GetInstance<T>() for custom damage types.
		/// </summary>
		public DamageClass DamageType {
			get => _damageClass;
			set => _damageClass = value ?? throw new ArgumentException("DamageType cannot be null");
		}

		private Dictionary<int, int> _toolTypes;
		private Dictionary<ToolPriority, List<int>> _toolPriorityCache;
		/// <summary>
		/// Whether this item has any ToolTypes as part of its behavior, and is therefore a tool.
		/// </summary>
		public bool IsTool => _toolTypes?.Count > 0;

		/// <summary> Gets the instance of the specified GlobalItem type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalItem<T>(bool exactType = true) where T : GlobalItem
			=> GlobalType.GetGlobal<Item, GlobalItem, T>(globalItems, exactType);

		/// <summary> Gets the local instance of the type of the specified GlobalItem instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalItem<T>(T baseInstance) where T : GlobalItem
			=> GlobalType.GetGlobal<Item, GlobalItem, T>(globalItems, baseInstance);

		/// <summary> Gets the instance of the specified GlobalItem type. </summary>
		public bool TryGetGlobalItem<T>(out T result, bool exactType = true) where T : GlobalItem
			=> GlobalType.TryGetGlobal<GlobalItem, T>(globalItems, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalItem instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalItem<T>(T baseInstance, out T result) where T : GlobalItem
			=> GlobalType.TryGetGlobal<GlobalItem, T>(globalItems, baseInstance, out result);

		public TagCompound SerializeData() => ItemIO.Save(this);

		internal static void PopulateMaterialCache() {
			for (int i = 0; i < Recipe.numRecipes; i++) {
				foreach (Item item in Main.recipe[i].requiredItem) {
					ItemID.Sets.IsAMaterial[item.type] = true;
				}
			}

			foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values) {
				foreach (var item in recipeGroup.ValidItems) {
					ItemID.Sets.IsAMaterial[item] = true;
				}
			}

			ItemID.Sets.IsAMaterial[71] = false;
			ItemID.Sets.IsAMaterial[72] = false;
			ItemID.Sets.IsAMaterial[73] = false;
			ItemID.Sets.IsAMaterial[74] = false;
		}

		public static int NewItem(Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false) => 
			Item.NewItem(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
		public static int NewItem(Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false) => 
			NewItem((int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

		public bool CountsAsClass(DamageClass damageClass) => DamageClassLoader.countsAs[DamageType.Type, damageClass.Type];

		/// <sumary> Whether this tool has a given ToolType as part of its behavior. </sumary>
		/// <param name="toolType"> The ToolType to check for. </param>
		public bool IsToolType(ToolType toolType) => _toolTypes?.ContainsKey(toolType.Type) == true;

		/// <sumary> Gets the power of a given ToolType for this tool's behavior. </sumary>
		/// <param name="toolType"> The ToolType whose power is being retrieved. </param>
		public int GetToolPower(ToolType toolType) {
			if (_toolTypes == null)
				return 0;
			if (_toolTypes.TryGetValue(toolType.Type, out int power) && power > 0)
				return power;
			if (toolType.PowerProvider == null)
				return 0;
			return GetToolPower(toolType.PowerProvider);
		}

		/// <sumary> Sets the power of a given ToolType for this tool's behavior. </sumary>
		/// <param name="toolType"> The ToolType whose power is being set. If the ToolType wasn't part of this tool's behavior, adds it. </param>
		/// <param name="power"> The power to be set to the ToolType. If the power is 0 or less, the ToolType will be removed from this tool's behavior. </param>
		public void SetToolPower(ToolType toolType, int power) {
			if (power <= 0) 
				RemoveToolType(toolType);
			else
				AddToolType(toolType, power);
		}

		/// <sumary> Adds a given ToolType to this tool's behavior. </sumary>
		/// <param name="toolType"> The ToolType being added to this tool's behavior. </param>
		/// <param name="power"> The power to be set to the ToolType. If it's 0 or less, the ToolType won't be added. </param>
		public void AddToolType(ToolType toolType, int power) {
			if (power <= 0)
				return;

			if (_toolTypes == null) {
				_toolTypes = new Dictionary<int, int>() { [toolType.Type] = power };
				_toolPriorityCache = new Dictionary<ToolPriority, List<int>>() { [toolType.Priority] = new List<int>() { toolType.Type } };
				ToolPowerSharingCheck(toolType, power);
				return;
			}

			if (!_toolTypes.ContainsKey(toolType.Type)) {
				if (_toolPriorityCache.TryGetValue(toolType.Priority, out var tools)) {
					tools.Add(toolType.Type);
					tools.Sort();
				}
				else {
					_toolPriorityCache[toolType.Priority] = new List<int>() { toolType.Type };
				}
			}

			_toolTypes[toolType.Type] = power;
			ToolPowerSharingCheck(toolType, power);
		}

		/// <sumary> Removes a given ToolType from this tool's behavior. ToolTypes that are receiving power from another one don't get removed if their source is still present, but if they were using their own power, they'll now receive it from their source.</sumary>
		/// <param name="toolType"> The ToolType being removed from this tool's behavior. </param>
		/// <returns> Whether the ToolType was successfully removed. Returns false if the ToolType wasn't part of this tool's behavior. </returns>
		public bool RemoveToolType(ToolType toolType) {
			if (_toolTypes?.Remove(toolType.Type) != true)
				return false;
			
			if (_toolTypes.Count == 0) {
				_toolTypes = null;
				_toolPriorityCache = null;
				return true;
			}

			var tools = _toolPriorityCache[toolType.Priority];
			if (tools.Count > 1)
				tools.Remove(toolType.Type);
			else
				_toolPriorityCache.Remove(toolType.Priority);

			ToolPowerSharingCheck(toolType);
			return true;
		}

		/// <sumary> Gets the ToolTypes used by this tool. </sumary>
		public IEnumerable<ToolType> GetToolTypes() {
			return _toolTypes?.Keys.OrderBy(i => i).Select(i => ToolTypeLoader.ToolTypes[i]) ?? Enumerable.Empty<ToolType>();
		}

		/// <sumary> Gets the ToolTypes with a specific priority used by this tool. </sumary>
		public IEnumerable<ToolType> GetToolTypes(ToolPriority priority) {
			if (_toolPriorityCache == null || !_toolPriorityCache.TryGetValue(priority, out var tools))
				return Enumerable.Empty<ToolType>();
			return tools.Select(i => ToolTypeLoader.ToolTypes[i]);
		}

		private void ToolPowerSharingCheck(ToolType toolType, int power = 0) {
			if (toolType.ProvidesPowerTo.Count == 0)
				return;

			foreach (var index in toolType.ProvidesPowerTo) {
				if (_toolTypes.TryGetValue(index, out int toolPower) && toolPower >= 0)
					continue;

				var tool = ToolTypeLoader.ToolTypes[index];
				if (power == 0) {
					RemoveToolType(tool);
					continue;
				}

				_toolTypes[index] = 0;
				if (_toolPriorityCache.TryGetValue(tool.Priority, out var tools)) {
					tools.Add(index);
					tools.Sort();
				}
				else {
					_toolPriorityCache[tool.Priority] = new List<int>() { index };
				}
			}
		}
	}
}