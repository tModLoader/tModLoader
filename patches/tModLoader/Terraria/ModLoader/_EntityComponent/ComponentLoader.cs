using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader
{
	public sealed class ComponentLoader : Loader
	{
		private struct ComponentTypeData
		{
			public bool IsGlobal;
			public int[] DependencyIds;
		}

		private static readonly List<Component> components = new();
		private static readonly List<Component> globalComponents = new();
		private static readonly List<Component> globalComponentsWithoutDependencies = new();
		private static readonly List<ComponentHook> componentHooks = new();
		private static readonly Dictionary<Type, int> componentIdsByType = new();

		private static ComponentTypeData[] componentypeDataById = Array.Empty<ComponentTypeData>();

		public static int ComponentCount => components.Count;

		internal override void ResizeArrays() {
			Array.Resize(ref componentypeDataById, components.Count);

			for (int i = 0; i < components.Count; i++) {
				var component = components[i];
				var type = component.GetType();
				ref var typeData = ref componentypeDataById[i];

				typeData = default;

				typeData.DependencyIds = type
					.GetCustomAttributes<ComponentDependencyAttribute>(true)
					.Select(a => componentIdsByType[a.ComponentType])
					.OrderBy(id => id)
					.ToArray();

				if (type.GetCustomAttribute<GlobalComponentAttribute>() != null) {
					typeData.IsGlobal = true;

					globalComponents.Add(component);

					if (typeData.DependencyIds.Length == 0) {
						globalComponentsWithoutDependencies.Add(component);
					}
				}
			}

			foreach (var hook in componentHooks) {
				hook.Update(components);
			}
		}

		internal override void Unload() {
			components.Clear();
			globalComponents.Clear();
			globalComponentsWithoutDependencies.Clear();
			componentIdsByType.Clear();

			componentypeDataById = Array.Empty<ComponentTypeData>();
		}

		internal static ushort RegisterComponent(Component component) {
			ushort id = (ushort)components.Count;

			componentIdsByType[component.GetType()] = id;

			components.Add(component);

			return id;
		}

		internal static void RegisterComponentHook(ComponentHook hook) {
			componentHooks.Add(hook);
		}

		internal static void AddGlobalComponents(GameObject gameObject) {
			foreach (var component in globalComponentsWithoutDependencies) {
				gameObject.AddComponentFromBase(component);
			}
		}
	}
}
