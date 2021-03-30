using System;
using System.Linq;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public class ToolBehavior
	{
		private Dictionary<int, int> _toolTypes;
		private Dictionary<ToolPriority, List<int>> _toolPriorityCache;

		public ToolBehavior() {
		}

		private ToolBehavior(Dictionary<int, int>? toolTypes, Dictionary<ToolPriority, List<int>>? toolPriorityCache) {
			if (toolTypes != null)
				_toolTypes = new Dictionary<int, int>(toolTypes);
			if (toolPriorityCache != null)
				_toolPriorityCache = toolPriorityCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
		}

		public int this[ToolType toolType] {
			get => GetToolPower(toolType);
			set => SetToolPower(toolType, value);
		}

		/// <sumary> The amount of ToolTypes that are part of this ToolBehavior. </sumary>
		public int ToolCount => _toolTypes?.Count ?? 0;

		/// <sumary> Whether this ToolBehavior has a given ToolType as part of itself. </sumary>
		/// <param name="toolType"> The ToolType to check for. </param>
		public bool HasToolType(ToolType toolType) => _toolTypes?.ContainsKey(toolType.Type) == true;

		/// <sumary> Gets the power of a given ToolType for this ToolBehavior. </sumary>
		/// <param name="toolType"> The ToolType whose power is being retrieved. </param>
		public int GetToolPower(ToolType toolType) {
			if (toolType == null)
				throw new ArgumentException("ToolType cannot be null");

			if (_toolTypes == null)
				return 0;
			if (_toolTypes.TryGetValue(toolType.Type, out int power) && power > 0)
				return power;
			if (toolType.PowerProvider == null)
				return 0;
			return GetToolPower(toolType.PowerProvider);
		}

		/// <sumary> Sets the power of a given ToolType for this ToolBehavior. </sumary>
		/// <param name="toolType"> The ToolType whose power is being set. If the ToolType wasn't part of this tool's behavior, adds it. </param>
		/// <param name="power"> The power to be set to the ToolType. If the power is 0 or less, the ToolType will be removed from this ToolBehavior. </param>
		public void SetToolPower(ToolType toolType, int power) {
			if (toolType == null)
				throw new ArgumentException("ToolType cannot be null");
			
			if (power <= 0) 
				RemoveToolType(toolType);
			else
				AddToolType(toolType, power);
		}

		/// <sumary> Adds a given ToolType to this ToolBehavior. </sumary>
		/// <param name="toolType"> The ToolType being added. </param>
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

		/// <sumary> Removes a given ToolType from this ToolBehavior. </sumary>
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
				if (!_toolTypes.ContainsKey(index) ^ power > 0)
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
				ToolPowerSharingCheck(tool, power);
			}
		}

		public ToolBehavior Clone() {
			return new ToolBehavior(_toolTypes, _toolPriorityCache);
		}
	}
}