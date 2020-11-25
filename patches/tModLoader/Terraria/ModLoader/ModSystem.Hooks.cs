using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Graphics;
using Terraria.UI;

namespace Terraria.ModLoader
{
	partial class ModSystem
	{
		/// <summary>
		/// Override this method to initialize your system. This is guaranteed to be called after all content has been autoloaded.
		/// </summary>
		public new virtual void Load() {
			
		}

		/// <summary>
		/// Allows you to load things in your system after the mod's content has been setup (arrays have been resized to fit the content, etc).
		/// </summary>
		public virtual void PostSetupContent() {
		}

		/// <summary>
		/// Allows you to determine what music should currently play.
		/// </summary>
		/// <param name="music">The music.</param>
		/// <param name="priority">The music priority.</param>
		public virtual void UpdateMusic(ref int music, ref MusicPriority priority) {
		}

		/// <summary>
		/// Allows you to set the transformation of the screen that is drawn. (Translations, rotations, scales, etc.)
		/// </summary>
		public virtual void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
		}

		/// <summary>
		/// Ran every update and suitable for calling Update for UserInterface classes
		/// </summary>
		public virtual void UpdateUI(GameTime gameTime) {
		}

		/// <summary>
		/// Use this if you want to do something before anything in the World gets updated.
		/// Called after UI updates, but before anything in the World (Players, NPCs, Projectiles, Tiles) gets updated.
		/// <para />
		/// When <see cref="Main.autoPause" /> is true or <see cref="Main.FrameSkipMode" /> is 0 or 2, the game may do a partial update. This means that it only updates menus and some animations, but not the World or Entities. This hook - and every hook after it - only gets called on frames with a full update.
		/// <para />
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// </summary>
		public virtual void PreUpdateEntities() {
		}

		/// <summary>
		/// Called after Players got updated, but before any NPCs get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="PreUpdateEntities" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// </summary>
		public virtual void MidUpdatePlayerNPC() {
		}

		/// <summary>
		/// Called after NPCs got updated, but before any Gores get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdatePlayerNPC" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// </summary>
		public virtual void MidUpdateNPCGore() {
		}

		/// <summary>
		/// Called after Gores got updated, but before any Projectiles get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateNPCGore" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// </summary>
		public virtual void MidUpdateGoreProjectile() {
		}

		/// <summary>
		/// Gets called immediately after all Projectiles are updated, but before any Items get updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateGoreProjectile" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateItemDust" />.
		/// </summary>
		public virtual void MidUpdateProjectileItem() {
		}

		/// <summary>
		/// Called after Items got updated, but before any Dust gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateProjectileItem" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateDustTime" />.
		/// </summary>
		public virtual void MidUpdateItemDust() {
		}

		/// <summary>
		/// Called after Dust got updated, but before Time (day/night, events, etc.) gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateItemDust" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// </summary>
		public virtual void MidUpdateDustTime() {
		}

		/// <summary>
		/// Called after Time got updated, but before the World gets updated.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateDustTime" />.
		/// If you are looking to hook a later part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void MidUpdateTimeWorld() {
		}

		/// <summary>
		/// Called after Invasions got updated. The only thing that is updated after this is the Network.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateTimeWorld" />.
		/// If you are looking to hook even after the Network is updated, see <see cref="PostUpdateEverything" />.
		/// </summary>
		public virtual void MidUpdateInvasionNet() {
		}

		/// <summary>
		/// Called after the Network got updated, this is the last hook that happens in an update.
		/// <para />
		/// If you are looking to hook an earlier part of the update process, see <see cref="MidUpdateInvasionNet" />.
		/// </summary>
		public virtual void PostUpdateEverything() {
		}

		/// <summary>
		/// Allows you to modify the elements of the in-game interface that get drawn. GameInterfaceLayer can be found in the Terraria.UI namespace. Check https://github.com/tModLoader/tModLoader/wiki/Vanilla-Interface-layers-values for vanilla interface layer names
		/// </summary>
		/// <param name="layers">The layers.</param>
		public virtual void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
		}

		/// <summary>
		/// Allows you to modify color of light the sun emits.
		/// </summary>
		/// <param name="tileColor">Tile lighting color</param>
		/// <param name="backgroundColor">Background lighting color</param>
		public virtual void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
		}

		/// <summary>
		/// Allows you to modify overall brightness of lights. Can be used to create effects similiar to what night vision and darkness (de)buffs give you. Values too high or too low might result in glitches. For night vision effect use scale 1.03
		/// </summary>
		/// <param name="scale">Brightness scale</param>
		public virtual void ModifyLightingBrightness(ref float scale) {
		}

		/// <summary>
		/// Called after interface is drawn but right before mouse and mouse hover text is drawn. Allows for drawing interface.
		/// 
		/// Note: This hook should no longer be used. It is better to use the ModifyInterfaceLayers hook.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch.</param>
		public virtual void PostDrawInterface(SpriteBatch spriteBatch) {
		}

		/// <summary>
		/// Called while the fullscreen map is active. Allows custom drawing to the map.
		/// </summary>
		/// <param name="mouseText">The mouse text.</param>
		public virtual void PostDrawFullscreenMap(ref string mouseText) {
		}

		/// <summary>
		/// Called after the input keys are polled. Allows for modifying things like scroll wheel if your custom drawing should capture that.
		/// </summary>
		public virtual void PostUpdateInput() {
		}

		/// <summary>
		/// Called in SP or Client when the Save and Quit button is pressed. One use for this hook is clearing out custom UI slots to return items to the player.  
		/// </summary>
		public virtual void PreSaveAndQuit() {
		}
	}
}
