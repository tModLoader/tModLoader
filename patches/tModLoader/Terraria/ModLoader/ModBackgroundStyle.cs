using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Core;

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

	[Obsolete]
	private void ModifyFarFades_Obsolete(float[] fades, float transitionSpeed) => ModifyFarFades(fades, transitionSpeed);

	[Obsolete($"Use {nameof(ModifyStyleFade)} instead", error: true)]
	public virtual void ModifyFarFades(float[] fades, float transitionSpeed) => throw new NotImplementedException();

	[Obsolete]
	private int ChooseFarTexture_Obsolete() => ChooseFarTexture();

	[Obsolete("Updated hook's parameters", error: true)]
	public virtual int ChooseFarTexture() => -1;

	[Obsolete]
	private int ChooseMiddleTexture_Obsolete() => ChooseMiddleTexture();

	[Obsolete("Updated hook's parameters", error: true)]
	public virtual int ChooseMiddleTexture() => -1;

	[Obsolete]
	private bool PreDrawCloseBackground_Obsolete(SpriteBatch spriteBatch) => PreDrawCloseBackground(spriteBatch);

	[Obsolete($"Use {nameof(PreDrawBackground)} instead", error: true)]
	public virtual bool PreDrawCloseBackground(SpriteBatch spriteBatch) => true;

	[Obsolete]
	private int ChooseCloseTexture_Obsolete(ref float scale, ref double parallax, ref float a, ref float b) => ChooseCloseTexture(ref scale, ref parallax, ref a, ref b);

	[Obsolete("Updated hook's parameters", error: true)]
	public virtual int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => -1;

	/// <summary>
	/// Allows you to modify the transparency of all background styles that exist. In general, you should move the index equal to this style's slot closer to 1, and all other indexes closer to 0. The transitionSpeed parameter is what you should add/subtract to each element of the fades parameter. See the ExampleMod for an example.
	/// </summary>
	public virtual void ModifyStyleFade(float[] fades, float transitionSpeed)
	{
		ModifyFarFades_Obsolete(fades, transitionSpeed);
	}

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
		return ChooseFarTexture_Obsolete();
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the middle of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseMiddleTexture(in BackgroundLayerParams layerParams)
	{
		return ChooseMiddleTexture_Obsolete();
	}

	/// <summary>
	/// Allows you to determine which texture is drawn in the closest part of the background. This also lets you modify the scale, parallax, Y value and Looping Width.
	/// </summary>
	/// <param name="layerParams">Parameters of background.</param>
	/// <returns></returns>
	public virtual int ChooseCloseTexture(in BackgroundLayerParams layerParams)
	{
		float a = layerParams.TopY;
		float b = layerParams.LoopWidth;
		int textureSlot = ChooseCloseTexture_Obsolete(ref layerParams.Scale, ref layerParams.Parallax, ref a, ref b);
		layerParams.TopY = (int)a;
		layerParams.LoopWidth = (int)b;
		return textureSlot;
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

	private static readonly MethodInfo modifyFadesMethod = typeof(ModSurfaceBackgroundStyle).GetMethod(nameof(ModifyStyleFade));

	protected override void ValidateType()
	{
		base.ValidateType();

		var t = GetType();

		if (!LoaderUtils.HasOverride(t, modifyFadesMethod)) {
			throw new Exception($"{t} must override {nameof(ModifyStyleFade)}.");
		}
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
