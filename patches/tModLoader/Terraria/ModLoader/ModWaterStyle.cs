using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using static Terraria.WaterfallManager;

namespace Terraria.ModLoader;

/// <summary>
/// Represents a style of water that gets drawn, based on factors such as the background. This is used to determine the color of the water, as well as other things as determined by the hooks below.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModWaterStyle : ModTexturedType
{
	/// <summary>
	/// The ID of the water style.
	/// </summary>
	public int Slot { get; internal set; }

	public virtual string BlockTexture => Texture + "_Block";
	public virtual string SlopeTexture => Texture + "_Slope";

	protected sealed override void Register()
	{
		Slot = LoaderManager.Get<WaterStylesLoader>().Register(this);
	}

	public sealed override void SetupContent()
	{
		LiquidRenderer.Instance._liquidTextures[Slot] = ModContent.Request<Texture2D>(Texture);

		SetStaticDefaults();

		TextureAssets.Liquid[Slot] = ModContent.Request<Texture2D>(BlockTexture);
		if (Mod.TModLoaderVersion < new System.Version(2023, 6, 24)) {
			TextureAssets.LiquidSlope[Slot] = ModContent.Request<Texture2D>(BlockTexture); // TODO: Remove workaround
		}
		else {
			TextureAssets.LiquidSlope[Slot] = ModContent.Request<Texture2D>(SlopeTexture);
		}
	}

	/// <summary>
	/// The ID of the waterfall style the game should use when this water style is in use.
	/// </summary>
	public abstract int ChooseWaterfallStyle();

	/// <summary>
	/// The ID of the dust that is created when anything splashes in water.
	/// </summary>
	public abstract int GetSplashDust();

	/// <summary>
	/// The ID of the gore that represents droplets of water falling down from a block. Return <see cref="ID.GoreID.WaterDrip"/> (or another existing droplet gore) or make a custom ModGore that uses <see cref="ID.GoreID.Sets.LiquidDroplet"/>.
	/// </summary>
	public abstract int GetDropletGore();

	/// <summary>
	/// Allows you to modify the light levels of the tiles behind the water. The light color components will be multiplied by the parameters.
	/// </summary>
	public virtual void LightColorMultiplier(ref float r, ref float g, ref float b)
	{
		// Default values taken from the LightMap constructor
		r = 0.88f;
		g = 0.96f;
		b = 1.015f;
	}

	/// <summary>
	/// Allows you to change the hair color resulting from the biome hair dye when this water style is in use.
	/// </summary>
	public virtual Color BiomeHairColor()
	{
		// Default value taken from DyeInitializer.LoadLegacyHairdyes on 1983 default case
		return new Color(28, 216, 94);
	}

	/// <summary>
	/// Returns the texture to be used when drawing rain of this water type.
	/// <br/>Default uses the vanilla rain texture.
	/// </summary>
	public virtual Asset<Texture2D> GetRainTexture()
	{
		return TextureAssets.Rain;
	}

	/// <summary>
	/// Return the variant of rain used. Equal to the offset in the rain texture divided by four.
	/// <br/>Vanilla rain has three variants per biome, and so vanilla variants range from 0 to 3 * Main.maxLiquidTextures.
	/// <br/>Default is a random number from 0 to 2, which creates normal vanilla forest biome rain.
	/// </summary>
	public virtual byte GetRainVariant()
	{
		return (byte)Main.rand.Next(3);
	}
}

/// <summary>
/// Represents a style of waterfalls that gets drawn. This is mostly used to determine the color of the waterfall.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModWaterfallStyle : ModTexturedType
{
	/// <summary>
	/// The ID of this waterfall style.
	/// </summary>
	public int Slot { get; internal set; }

	protected sealed override void Register()
	{
		Slot = LoaderManager.Get<WaterFallStylesLoader>().Register(this);
	}

	public sealed override void SetupContent()
	{
		Main.instance.waterfallManager.waterfallTexture[Slot] = ModContent.Request<Texture2D>(Texture);

		SetStaticDefaults();
	}

	/// <summary>
	/// Allows you to create light at a tile occupied by a waterfall of this style.
	/// </summary>
	public virtual void AddLight(int i, int j)
	{
	}

	/// <summary>
	/// Allows you to determine the color multiplier acting on waterfalls of this style. Useful for waterfalls whose colors change over time.
	/// </summary>
	public virtual void ColorMultiplier(ref float r, ref float g, ref float b, float a)
	{
	}

	/// <summary>
	/// Allows you to draw things behind the waterfall at the given coordinates. Return false to stop the game from drawing the waterfall normally. Returns true by default.
	/// </summary>
	/// <param name="currentWaterfallData">The current waterfall data.</param>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The Y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <returns></returns>
	public virtual bool PreDraw(WaterfallData currentWaterfallData, int i, int j, SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things overtop of the waterfall at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// </summary>
	/// <param name="currentWaterfallData">The current waterfall data, this is used inside of the waterfalls WaterfallData array.</param>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The Y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <returns></returns>
	public virtual void PostDraw(WaterfallData currentWaterfallData, int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Allows you to animate your waterfall. <br/>
	/// Overriding this method will make your waterfall nolonger animate normally.<br/><br/>
	/// Use frame to specify which frame the waterfall is using currently. <br/><br/>
	/// Use frameBackground to specify which background frame the waterfall is using. (This normally goes unused, but is very useful for modders looking into drawing their own waterfalls manually). <br/>
	/// Rain clouds use this to specify the framing of the rain behind the main rain waterfall.<br/><br/>
	/// Use frameCounter to specify the duration between frames.<br/><br/>
	/// </summary>
	/// <param name="frame">Waterfalls use this to know what frame to use when drawing.</param>
	/// <param name="frameBackground">Unused normally, can be used by modders for extra framing.</param>
	/// <param name="frameCounter">Used to specify a certain amount of time between waterfall frames.</param>
	public virtual void AnimateWaterfall(ref int frame, ref int frameBackground, ref int frameCounter)
	{
	}

	/// <summary>
	/// Edits the opacity of the waterfall. For example: Waterfalls have an opacity of 60% (0.6f) which allows you to see some stuff behind them, while Lavafalls have an opacity of 100% (1f) which prevents you from seeing anything behind. <br />
	/// Returns null be default.
	/// </summary>
	/// <param name="x">The x position in tile coordinates.</param>
	/// <param name="y">The Y position in tile coordinates.</param>
	/// <param name="Alpha">The current waterfall water style alpha</param>
	/// <param name="maxSteps">The maximum length of the waterfall</param>
	/// <param name="s"></param>
	/// <param name="tileCache">Tile at the waterfall position</param>
	/// <returns></returns>
	public virtual float? Alpha(int x, int y, float Alpha, int maxSteps, int s, Tile tileCache)
	{
		return null;
	}

	/// <summary>
	/// Allows you to prevent the waterfall/liquidfall from making any water sounds when on screen. This is useful for waterfalls/liquidfalls that arent made of water. Returns true by default. 
	/// </summary>
	/// <returns></returns>
	public virtual bool PlayWaterfallSounds()
	{
		return true;
	}
}
