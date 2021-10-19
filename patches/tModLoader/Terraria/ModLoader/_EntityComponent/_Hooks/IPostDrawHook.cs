using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <inheritdoc cref="PostDrawEntity"/>
	[ComponentHook]
	public partial interface IPostDrawHook
	{
		/// <summary>
		/// Allows you to draw things in front of an entity. <br/> 
		/// Use the 'Main.EntitySpriteDraw' method for drawing. <br/>
		/// Substract <paramref name="screenPos"/> from the draw position before drawing.
		/// </summary>
		/// <param name="screenPos"> The screen position used to translate world position into screen position. </param>
		/// <param name="drawColor" >The color the NPC is drawn in. </param>
		void PostDrawEntity(Vector2 screenPos, Color drawColor);

		/// <inheritdoc cref="PostDrawEntity"/>
		public static void Invoke(GameObject gameObject, Vector2 screenPos, Color drawColor) {
			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				function(obj, screenPos, drawColor);
			}
		}
	}
}
