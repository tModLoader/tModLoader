using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Terraria.ModLoader.Core
{
	/// <summary>
	///		Handles communicating between the parent <see cref="AssemblyLoadContext"/> and child, coremodded <see cref="AssemblyLoadContext"/>.
	/// </summary>
	internal static class CoremodCommunications
	{
		internal static bool Initialized;

		/// <summary>
		///		Retrieve the <see cref="AssemblyLoadContext"/> instance being used. This is the child instance when coremods are enabled, and the disabled instance when not.
		/// </summary>
		public static Func<AssemblyLoadContext> GetLoadContext;

		/// <summary>
		///		Determines whether the <see cref="CoremodLauncher"/> permits the loading of coremods.
		/// </summary>
		public static Func<bool> CoremodsEnabled;

		/// <summary>
		///		Initializes communications when coremods are allowed to be loaded by the <see cref="CoremodLauncher"/> (when -coremods-loaded is passed).
		/// </summary>
		internal static void InitializeEnabled() {
			GetLoadContext = () => CoremodLauncher.ChildLoadContext;
			CoremodsEnabled = () => true;

			Initialized = true;
		}

		/// <summary>
		///		Initializes communications when coremodes are not allowed to be loaded by the <see cref="CoremodLauncher"/> (when -skip-coremods is passed).
		/// </summary>
		internal static void InitializeDisabled() {
			GetLoadContext = () => AssemblyLoadContext.Default;
			CoremodsEnabled = () => false;

			Initialized = true;
		}

		/// <summary>
		///		Sets the fields of this class to the values of the fields from the parent <see cref="AssemblyLoadContext"/>.
		/// </summary>
		/// <param name="parentAlcType">This class, but from the parent <see cref="AssemblyLoadContext"/>.</param>
		internal static void InitializeFromParent(Type parentAlcType) {
			foreach (FieldInfo parentField in parentAlcType.GetFields(BindingFlags.Public | BindingFlags.Static)) {
				FieldInfo childField = typeof(CoremodCommunications).GetField(parentField.Name, BindingFlags.Public | BindingFlags.Static);

				// Set the child ALC's type's value to the parent ALC's type's value. This allows us to share instances.
				childField?.SetValue(null, parentField.GetValue(null));
			}

			Initialized = true;
		}
	}
}