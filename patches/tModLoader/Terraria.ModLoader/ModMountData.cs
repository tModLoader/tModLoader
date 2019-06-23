using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to place all your properties and hooks for each mount. Create instances of ModMoundData (preferably overriding this class) to pass as parameters to Mod.AddMount.
	/// </summary>
	public class ModMountData
	{
		internal string texture;

		/// <summary>
		/// The vanilla MountData object that is controlled by this ModMountData.
		/// </summary>
		public Mount.MountData mountData {
			get;
			internal set;
		}

		/// <summary>
		/// The mod which has added this ModMountData.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The index of this ModMountData in the Mount.mounts array.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		/// <summary>
		/// The name of this type of mount.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ModMountData() {
			mountData = new Mount.MountData();
		}

		/// <summary>
		/// Allows you to automatically load a mount instead of using Mod.AddMount. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name, texture is initialized to the namespace and overriding class name with periods replaced with slashes, and extraTextures is initialized to a dictionary containing all MountTextureTypes as keys, with texture + "_" + the texture type name as values. Use this method to either force or stop an autoload, change the default display name and texture path, and to modify the extra mount textures.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="texture"></param>
		/// <param name="extraTextures"></param>
		/// <returns></returns>
		public virtual bool Autoload(ref string name, ref string texture, IDictionary<MountTextureType, string> extraTextures) {
			return mod.Properties.Autoload;
		}

		internal void SetupMount(Mount.MountData mountData) {
			ModMountData newMountData = (ModMountData)MemberwiseClone();
			newMountData.mountData = mountData;
			mountData.modMountData = newMountData;
			newMountData.mod = mod;
			newMountData.SetDefaults();
		}

		/// <summary>
		/// Allows you to set the properties of this type of mount.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to modify the mount's jump height based on its state.
		/// </summary>
		/// <param name="jumpHeight"></param>
		/// <param name="xVelocity"></param>
		public virtual void JumpHeight(ref int jumpHeight, float xVelocity) {
		}

		/// <summary>
		/// Allows you to modify the mount's jump speed based on its state.
		/// </summary>
		/// <param name="jumpSeed"></param>
		/// <param name="xVelocity"></param>
		public virtual void JumpSpeed(ref float jumpSeed, float xVelocity) {
		}

		/// <summary>
		/// Allows you to make things happen when mount is used (creating dust etc.) Can also be used for mount special abilities.
		/// </summary>
		/// <param name="player"></param>
		public virtual void UpdateEffects(Player player) {
		}

		/// <summary>
		/// Allows for manual updating of mount frame. Return false to stop the default frame behavior. Returns true by default.
		/// </summary>
		/// <param name="mountedPlayer"></param>
		/// <param name="state"></param>
		/// <param name="velocity"></param>
		/// <returns></returns>
		public virtual bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			return true;
		}

		//todo: MountLoader is never called for this, why is this in here? Made it internal for now
		internal virtual bool CustomBodyFrame() {
			return false;
		}

		/// <summary>
		/// Allows you to make things happen while the mouse is pressed while the mount is active. Called each tick the mouse is pressed.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="mousePosition"></param>
		/// <param name="toggleOn">Does nothing yet</param>
		public virtual void UseAbility(Player player, Vector2 mousePosition, bool toggleOn) {
		}

		/// <summary>
		/// Allows you to make things happen when the mount ability is aiming (while charging).
		/// </summary>
		/// <param name="player"></param>
		/// <param name="mousePosition"></param>
		public virtual void AimAbility(Player player, Vector2 mousePosition) {
		}
	}
}
