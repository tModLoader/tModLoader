using Microsoft.Xna.Framework.Graphics;

namespace Terraria.ModLoader;

public abstract class ModBackgroundStyle : ModType
{
	/// <summary>
	/// The ID of this underground background style.
	/// </summary>
	public int Slot { get; internal set; }
}

/// <summary>
/// Each background style determines in its own way how exactly the background is drawn. This class serves as a collection of functions for underground backgrounds.
/// </summary>
public abstract class ModUndergroundBackgroundStyle : ModBackgroundStyle
{
	protected override sealed void Register()
	{
		Slot = LoaderManager.Get<UndergroundBackgroundStylesLoader>().Register(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	public virtual bool PreDrawBackground(SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine which textures make up the background by assigning their background slots/IDs to the given array. BackgroundTextureLoader.GetBackgroundSlot may be useful here. Index 0 is the texture on the border of the ground and sky layers. Index 1 is the texture drawn between rock and ground layers. Index 2 is the texture on the border of ground and rock layers. Index 3 is the texture drawn in the rock layer. The border images are 160x16 pixels, and the others are 160x96, but it seems like the right 32 pixels of each is a duplicate of the far left 32 pixels.
	/// </summary>
	public abstract void FillTextureArray(int[] textureSlots);
}

/// <summary>
/// Each background style determines in its own way how exactly the background is drawn. This class serves as a collection of functions for above-ground backgrounds.
/// </summary>
public abstract class ModSurfaceBackgroundStyle : ModBackgroundStyle
{
	public enum BackgroundDistance
	{
		Close,
		Middle,
		Far
	}

	protected override sealed void Register()
	{
		Slot = LoaderManager.Get<SurfaceBackgroundStylesLoader>().Register(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to modify the transparency of all background styles that exist. In general, you should move the index equal to this style's slot closer to 1, and all other indexes closer to 0. The transitionSpeed parameter is what you should add/subtract to each element of the fades parameter. See the ExampleMod for an example.
	/// </summary>
	public abstract void ModifyStyleFade(float[] fades, float transitionSpeed);

	/// <summary>
	/// Gives you complete freedom over how the background is drawn. Return true for ChooseCloseTexture, ChooseCloseMidTexture, ChooseCloseFarTexture, ChooseMiddleTexture and ChooseFarTexture to have an effect; return false to disable tModLoader's own code for drawing the close background.
	/// </summary>
	public virtual bool PreDrawBackground(BackgroundDistance distance, SpriteBatch spriteBatch, double backgroundTopMagicNumber, float bgGlobalScaleMultiplier, int pushBGTopHack)
	{
		return true;
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the very back of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseFarTexture(in BackgroundLayerParams layerParams)
	{
		return -1;
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the middle of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseMiddleTexture(in BackgroundLayerParams layerParams)
	{
		return -1;
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the closest part of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseCloseTexture(in BackgroundLayerParams layerParams)
	{
		return -1;
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the closest middle part of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseCloseMidTexture(in BackgroundLayerParams layerParams)
	{
		return -1;
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the closest furthest part of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseCloseFarTexture(in BackgroundLayerParams layerParams)
	{
		return -1;
	}
}

/// <summary>
/// This class serves to collect functions that operate on any kind of background style, without being specific to one single background style.
/// </summary>
public abstract class GlobalBackgroundStyle : ModType
{
	protected override sealed void Register()
	{
		ModTypeLookup<GlobalBackgroundStyle>.Register(this);
		GlobalBackgroundStyleLoader.globalBackgroundStyles.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to change which underground background style is being used.
	/// </summary>
	public virtual void ChooseUndergroundBackgroundStyle(ref int style)
	{
	}

	/// <summary>
	/// Allows you to change which surface background style is being used.
	/// </summary>
	public virtual void ChooseSurfaceBackgroundStyle(ref int style)
	{
	}

	/// <summary>
	/// Allows you to change which textures make up the underground background by assigning their background slots/IDs to the given array. Index 0 is the texture on the border of the ground and sky layers. Index 1 is the texture drawn between rock and ground layers. Index 2 is the texture on the border of ground and rock layers. Index 3 is the texture drawn in the rock layer. The border images are 160x16 pixels, and the others are 160x96, but it seems like the right 32 pixels of each is a duplicate of the far left 32 pixels.
	/// </summary>
	public virtual void FillUndergroundTextureArray(int style, int[] textureSlots)
	{
	}

	/// <summary>
	/// Allows you to modify the transparency of all background styles that exist. The style parameter is the current style that is being used.
	/// </summary>
	public virtual void ModifyFarSurfaceFades(int style, float[] fades, float transitionSpeed)
	{
	}
}
