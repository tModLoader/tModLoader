using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

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
		private static readonly List<ComponentHookList> componentHooks = new();
		private static readonly Dictionary<Type, int> componentIdsByType = new();

		private static ComponentTypeData[] componentypeDataById = Array.Empty<ComponentTypeData>();

		public static int ComponentCount => components.Count;

		internal override void ResizeArrays() {
			Array.Resize(ref componentypeDataById, components.Count);

			// Run static constructors of hook interfaces
			foreach (var mod in ModLoader.Mods) {
				var assembly = mod.Code;

				foreach (var type in assembly.GetTypes()) {
					if (type.IsInterface && type.GetCustomAttribute<ComponentHookAttribute>() != null) {
						// Won't run more than once in .NET Core+
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}
				}
			}

			// Setup static component information
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

			// Prepare hooklists
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

		internal static void RegisterComponentHookList(ComponentHookList hook) {
			componentHooks.Add(hook);
		}

		internal static void AddGlobalComponents(GameObject gameObject) {
			foreach (var component in globalComponentsWithoutDependencies) {
				gameObject.AddComponentFromBase(component);
			}
		}
	}
}
