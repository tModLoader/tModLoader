using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Light;

namespace Terraria;

partial class Lighting
{
	/// <summary>
	///		Requests the active <see cref="LightMap"/> as a
	///		<see cref="Texture2D"/>, with each pixel corresponding to a tile.
	///		<br />
	///		For more information on how to use this buffer, see
	///		<see cref="LightMapBuffer"/>.
	///		<para />
	///		<b>You should never store the returned buffer</b>; instead, this
	///		method should be directly called every time you need to access the
	///		buffer across multiple frames.
	///		<br />
	///		The buffer may be arbitrarily written to, or even disposed of and
	///		reinitalized between frames in response to lighting updates and
	///		light map resizes.
	///		<para/>
	///		<b>This method is not thread-safe; it many initialize and mutate
	///		graphics resources and must be called exclusively on the main
	///		thread.</b>
	/// </summary>
	/// <returns>
	///		A <see cref="LightMapBuffer"/> with data corresponding to the
	///		active <see cref="LightMap"/>.
	/// </returns>
	/// <seealso cref="LightMapBuffer"/>
	public static LightMapBuffer GetBufferTexture()
	{
		return _activeEngine.GetBufferTexture();
	}
}
