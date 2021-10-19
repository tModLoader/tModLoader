using Microsoft.Xna.Framework;

namespace Terraria.ModLoader
{
	/// <inheritdoc cref="PreDrawEntity"/>
	[ComponentHook]
	public partial interface IPreDrawHook
	{
		/// <summary>
		/// Allows you to draw things behind an entity, or to modify the way it's drawn. <br/> 
		/// Use the 'Main.EntitySpriteDraw' method for drawing. <br/>
		/// Substract <paramref name="screenPos"/> from the draw position before drawing. <para/> 
		/// Return false to stop the game from drawing the entity (useful if you're manually drawing the entity). <br/> 
		/// Returns true by default.
		/// </summary>
		/// <param name="screenPos"> The screen position used to translate world position into screen position. </param>
		/// <param name="drawColor" >The color the NPC is drawn in. </param>
		bool PreDrawEntity(Vector2 screenPos, ref Color drawColor);

		/// <inheritdoc cref="PreDrawEntity"/>
		public static bool Invoke(GameObject gameObject, Vector2 screenPos, ref Color drawColor) {
			bool result = true;

			foreach (var (obj, function) in Hook.Enumerate(gameObject)) {
				result &= function(obj, screenPos, ref drawColor);
			}

			return result;
		}
	}
}
