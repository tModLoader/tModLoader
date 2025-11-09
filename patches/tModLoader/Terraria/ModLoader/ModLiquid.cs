using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.Localization;
using static Terraria.GameContent.Liquid.LiquidRenderer;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of liquid that can be added by a mod. Only one instance of this class will ever exist for each type of liquid that is added. Any hooks that are called will be called by the instance corresponding to the liquid type. This is to prevent the game from using a massive amount of memory storing liquid instances.<br />
/// </summary>
// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile">Basic Liquid Guide</see> teaches the basics of making a modded tile. //Add documentation page
public abstract class ModLiquid : ModTexturedType, ILocalizedModType, IModType
{
	public string LocalizationCategory => "Liquids";

	/// <summary> The Liquid ID of this Modded Liquid </summary>
	public ushort Type { get; internal set; }

	/// <summary> The block texture of this liquid. Used for some slope framing and when liquids are rendered in retro lighting modes. </summary>
	public virtual string BlockTexture => Texture + "_Block";

	/// <summary> The slope texture of this liquid. Used for rendering the liquids next to tile slopes. </summary>
	public virtual string SlopeTexture => Texture + "_Slope";

	/// <summary> The default sound that plays when an entity enters and exits this liquid. Overriding any OnSplash hooks/methods will prevent this from being used. </summary>
	public SoundStyle? SplashSound { get; set; }

	/// <summary> The default dust ID thats used when an entity enters and exists this liquid. Overriding any OnSplash hooks/methods will prevent this from being used. </summary>
	public int SplashDustType { get; set; } = -1;

	/// <summary> Liquids can only have a maximum fall delay of 10 (which is the same as the fall of Honey), this is due to the delay being reset to 10 when in quickfall mode. </summary>
	public int FallDelay { get; set; } = 0;

	/// <summary> The vanilla ID of what should replace the instance when a user unloads and subsequently deletes data from your mod in their save file. Defaults to 0. </summary>
	public ushort VanillaFallbackOnModDeletion { get; set; }

	/// <summary> An array of the IDs of liquids that this tile can be considered as when looking for available liquids. </summary>
	public int[] AdjLiquids { get; set; } = Array.Empty<int>();

	/// <summary> The check in Collision that checks if a coord will be drowning in water.</summary>
	public bool ChecksForDrowning { get; set; } = true;

	/// <summary> Whether or not, when drowning, a player or npc will emit dusts from their mouth. </summary>
	public bool AllowEmitBreathBubbles { get; set; } = true;

	/// <summary> The multiplier used to change how many tiles are needed to fish in a pool of liquid. Honey uses a 1.5x multiplier for its pool size. </summary>
	public float FishingPoolSizeMultiplier { get; set; } = 1f;

	/// <summary> The multiplier used only when Waves quality setting is set to Medium. Honey and Lava use this to set the modifier to 0.3x. </summary>
	public float WaterRippleMultiplier { get; set; } = 1f;

	/// <summary> The opacity that this liquid slope renders at. Lava and Honey use this to look thicker. Defaults to 0.5f. </summary>
	public float SlopeOpacity { get; set; } = 0.5f;

	/// <summary> The multiplier thats used to specify how slow (or fast) the player moves while in this liquid. </summary>
	public float PlayerMovementMultiplier { get; set; } = 0.5f;

	/// <summary> The multiplier used for stopwatches to know how to offset the MPH reading. </summary>
	public float StopWatchMPHMultiplier { get; set; } = 0.5f;

	/// <summary> The default value that ModLiquidNPC.moddedLiquidMovementSpeed sets to before ModNPC.SetDefaults </summary>
	public float NPCMovementMultiplierDefault { get; set; } = 0.5f;

	/// <summary> The multiplier thats used to specify how slow (or fast) projectiles move while in this liquid. </summary>
	public float ProjectileMovementMultiplier { get; set; } = 0.5f;

	/// <summary> Uses lava based collision check for when applying this liquid's wet boolean. <br/>
	/// This means collision is more sensitive, and the wet is active even when touching the smallest amount of this liquid. </summary>
	public bool UsesLavaCollisionForWet { get; set; } = false;

	/// <summary> Whether or not this liquid removes the On Fire! debuffs when entering. </summary>
	public bool ExtinguishesOnFireDebuffs { get; set; } = true;

	/// <summary> The multiplier repsonsible for multiplying how much opacity each step in this liquid's fall should be drawn at.  
	/// <br/> Only effects slopes when underground (similar to other liquids).</summary>
	public float LiquidfallOpacityMultiplier { get; set; } = 1f;

	/// <summary>
	/// Adds an entry to the minimap for this liquid with the given color and display name. This should be called in SetDefaults.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name = null)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name);
			if (!MapLoader.liquidEntries.Keys.Contains(Type)) {
				MapLoader.liquidEntries[Type] = new List<MapEntry>();
			}
			MapLoader.liquidEntries[Type].Add(entry);
		}
	}

	/// <summary>
	/// <inheritdoc cref="M:ModLiquidLib.ModLoader.ModLiquid.AddMapEntry(Microsoft.Xna.Framework.Color,Terraria.Localization.LocalizedText)" />
	/// <br /><br /> <b>Overload specific:</b> This overload has an additional <paramref name="nameFunc" /> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name, nameFunc);
			if (!MapLoader.liquidEntries.Keys.Contains(Type)) {
				MapLoader.liquidEntries[Type] = new List<MapEntry>();
			}
			MapLoader.liquidEntries[Type].Add(entry);
		}
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModLiquid>.Register(this);
		Type = (ushort)LiquidLoader.ReserveLiquidID();
		LiquidLoader.liquids.Add(this);
	}

	public sealed override void SetupContent()
	{
		LiquidLoader.LiquidAssets[Type] = ModContent.Request<Texture2D>(Texture, (AssetRequestMode)2);
		LiquidLoader.LiquidBlockAssets[Type] = ModContent.Request<Texture2D>(BlockTexture, (AssetRequestMode)2);
		LiquidLoader.LiquidSlopeAssets[Type] = ModContent.Request<Texture2D>(SlopeTexture, (AssetRequestMode)2);
		LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
		LiquidRenderer.DEFAULT_OPACITY[Type] = 0.6f;
		LiquidRenderer.VISCOSITY_MASK[Type] = 0;
		SetStaticDefaults();
	}

	/// <summary>
	/// Legacy helper method for creating a localization sub-key MapEntry
	/// </summary>
	/// <returns></returns>
	public LocalizedText CreateMapEntryName()
	{
		return this.GetLocalization("MapEntry", PrettyPrintName);
	}

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br /> This is where you would set the properties of this liquid. Many properties are stored as arrays throughout Terraria's code.
	/// <br /> For example:
	/// <list type="bullet">
	/// <item> WATERFALL_LENGTH = 10 </item>
	/// <item> DefaultOpacity = 0.6f; </item>
	/// <item> VisualViscosity = 0; </item>
	/// </list>
	/// </summary>
	public override void SetStaticDefaults()
	{
	}

	/// <summary>
	/// Allows you to choose which minimap entry the liquid at the given coordinates will use. 0 is the first entry added by AddMapEntry, 1 is the second entry, etc. Returns 0 by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual ushort GetMapOption(int i, int j)
	{
		return 0;
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreRetroDraw" /> for the other Lighting modes pre drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquidDrawCache">The LiquidDrawCache of the rendering</param>
	/// <param name="drawOffset">The amount the liquid rendered is offset by</param>
	/// <param name="isBackgroundDraw">Whether or not the liquid is in the background</param>
	public virtual bool PreDraw(int i, int j, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreDraw" /> returns false.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PostRetroDraw" /> for the other Lighting modes post drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquidDrawCache">The LiquidDrawCache of the rendering</param>
	/// <param name="drawOffset">The amount the liquid rendered is offset by</param>
	/// <param name="isBackgroundDraw">Whether or not the liquid is in the background</param>
	public virtual void PostDraw(int i, int j, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the liquid at the given coordinates is drawn. For example, creating bubble dusts or changing the color the liquid is drawn in. <para />
	/// This hook is seperate from Liquid Rendering, Only use this for things such as spawning particles.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquidCache">Various information about the liquid that is being drawn, such as opacity, visual liquid, etc.</param>
	public virtual void EmitEffects(int i, int j, LiquidCache liquidCache)
	{
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default.<para />
	/// This method is only called in "Retro" and "Trippy" lighting modes. Use <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreDraw" /> for the other Lighting modes pre drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual bool PreRetroDraw(int i, int j, SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreRetroDraw" /> returns false.<para />
	/// This method is only called in "Retro" and "Trippy" lighting modes. Use <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PostDraw" /> for the other Lighting modes post drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	public virtual void PostRetroDraw(int i, int j, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the liquid at the given coordinates is drawn. For example, creating bubble dusts or changing the color the liquid is drawn in. <para />
	/// This hook is not called if <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreRetroDraw" /> returns false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="spriteBatch"></param>
	/// <param name="drawData">Various information about the liquid that is being drawn, such as color, opacity, waterfall length, etc.</param>
	/// <param name="liquidAmountModified">Gets the amount of liquid in a tile for retro rendering</param>
	/// <param name="liquidGFXQuality">Gets the number of quality the game has for retro rendering</param>
	public virtual void RetroDrawEffects(int i, int j, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality)
	{
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default. <para />
	/// Only called for liquid slopes. If you want to predraw the liquid itself, see <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreDraw" /> or <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreRetroDraw" />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="behindBlocks">Whether or not the slope is rendered behind tiles</param>
	/// <param name="drawPosition">The rendering position of the slope. Use this rather than x/y as the rendering can be offset sometimes</param>
	/// <param name="liquidSize">The size of the liquid</param>
	/// <param name="colors">The color of the liquid. Usually the light color of the liquid</param>
	public virtual bool PreSlopeDraw(int i, int j, bool behindBlocks, ref Vector2 drawPosition, ref Rectangle liquidSize, ref VertexColors colors)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PreSlopeDraw" /> returns false.<para />
	/// Only called for liquid slopes. If you want to postdraw the liquid itself, see <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PostDraw" /> or <see cref="P:ModLiquidLib.ModLoader.ModLiquid.PostRetroDraw" />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="behindBlocks">Whether or not the slope is rendered behind tiles</param>
	/// <param name="drawPosition">The rendering position of the slope. Use this rather than x/y as the rendering can be offset sometimes</param>
	/// <param name="liquidSize">The size of the liquid</param>
	/// <param name="colors">The color of the liquid. Usually the light color of the liquid</param>
	public virtual void PostSlopeDraw(int i, int j, bool behindBlocks, ref Vector2 drawPosition, ref Rectangle liquidSize, ref VertexColors colors)
	{
	}

	/// <summary>
	/// Allows you to determine how much light this liquid emits.<br />
	/// It can also let you light up the block in front of this liquid.<br />
	/// See <see cref="M:Terraria.Graphics.Light.TileLightScanner.ApplyTileLight(Terraria.Tile,System.Int32,System.Int32,Terraria.Utilities.FastRandom@,Microsoft.Xna.Framework.Vector3@)" /> for vanilla tile light values to use as a reference.<br />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="r">The red component of light, usually a value between 0 and 1</param>
	/// <param name="g">The green component of light, usually a value between 0 and 1</param>
	/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
	public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
	}

	/// <summary>
	/// Modifies the LightMaskMode used for this liquid. <br/>
	/// Due to limitations in the lighting engine, tile coordinates and editing of vanilla lightmasks are unavaliable. <br/>
	/// NOTE: set LiquidLightMaskMode to None for this hook/method to be called. Otherwise this liquid will use the other mask mode instead.
	/// </summary>
	/// <param name="index">The current LightMap index. Water uses this to randomise its colors slightly.</param>
	/// <param name="r">The red component of light, usually a value between 0 and 1</param>
	/// <param name="g">The green component of light, usually a value between 0 and 1</param>
	/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
	public virtual void ModifyLightMaskMode(int index, ref float r, ref float g, ref float b)
	{
	}

	/// <summary>
	/// Allows you to change the Light Mask Mode for this liquid. The Light Mask Mode is the mask for lighting to determine how light interacts with this liquid. <br/>
	/// Vanilla has options for Water (slight blue fade) and Honey (dark black) while lava uses the None option. <br/>
	/// Defaults to LightMaskMode.None.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual LightMaskMode LiquidLightMaskMode(int i, int j)
	{
		return LightMaskMode.None;
	}

	/// <summary>
	/// The ID of the waterfall/liquidfall style the game should use when this liquid is near a half block
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual int ChooseWaterfallStyle(int i, int j)
	{
		return 0;
	}

	/// <summary>
	/// Allows the changing of liquid movement in-game as well as allowing you to do other stuff with liquids during updates such as custom evaporation. Returns true by default. 
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquid"></param>
	/// <returns></returns>
	public virtual bool UpdateLiquid(int i, int j, Liquid liquid)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make your liquid evaporate like water when in the underworld. Returns false by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool EvaporatesInHell(int i, int j)
	{
		return false;
	}

	/// <summary>
	/// Allows you to change how your liquid moves when loading or creating a world. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool SettleLiquidMovement(int i, int j)
	{
		return true;
	}

	/// <summary>
	/// Used to change what tile the liquid generates upon coming in contact with another liquid. Use the otherLiquid param to check what liquid this liquid is interacting with.<br/>
	/// This method is called even if a tile isn't generated. It is recommended that modders use a non-frame important tile as the result, otherwise the tile may fail to generate.<br/>
	/// It is recomended that developers use PreLiquidMerge for handling custom collisions and effects as that method is only called when a tile is requested to generate.<br/>
	/// This hook is still called, even if PreLiquidMerge is returned false. <br/>
	/// NOTE: it is encouraged to set a default tile that this liquid results in, otherwise it will default to stone. <br/>
	/// Returns 1 by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="otherLiquid">The liquid ID of the liquid colliding with this one.</param>
	/// <returns></returns>
	public virtual int LiquidMerge(int i, int j, int otherLiquid)
	{
		return TileID.Stone;
	}

	/// <summary>
	/// Used to modify the sound thats played when two liquids merge with each other. <br/>
	/// Called every time liquids collide and only on clients. <br/>
	/// Is not called if PreLiquidMerge returns false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="otherLiquid">The liquid ID of the liquid colliding with this one.</param>
	/// <param name="collisionSound">The sound to be played, set this to a sound if you don't want the default merge sound to play.</param>
	public virtual void LiquidMergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound)
	{

	}

	/// <summary>
	/// Used to do do extra effects and special merges when two liquids collide with each other. Use the otherLiquid param to check which liquid is colliding with this liquid. <br/>
	/// NOTE: this method is called ONLY on servers when in multiplayer. Make sure you send a packet if you want to do extra visual effects. To learn more about handling custom packets, please see <see cref="Mod.HandlePacket" />. <br/>
	/// Return false to prevent the normal liquid merging logic. <br/>
	/// Retruns true by default.
	/// </summary>
	/// <param name="liquidX">The x position of the target liquid in tile coordinates<br/>
	/// NOTE: This may not always mean the x position of THIS liquid, as it may mean the x position of the other liquid.</param>
	/// <param name="liquidY">The y position of the target liquid in tile coordinates<br/>
	/// NOTE: This may not always mean the y position of THIS liquid, as it may mean the y position of the other liquid.</param>
	/// <param name="tileX">The x position of where the tile is sechedualed to generate at in tile coordninates.<br/>
	/// NOTE: most of the time this x position is the same as liquid's x position, this means that the tile is being generated over the liquid.<br/>
	/// Useful for when trying to do something else other than generating a tile.</param>
	/// <param name="tileY">The y position of where the tile is sechedualed to generate at in tile coordninates.<br/>
	/// NOTE: most of the time this y position is the same as liquid's y position, this means that the tile is being generated over the liquid.<br/>
	/// Useful for when trying to do something else other than generating a tile.</param>
	/// <param name="otherLiquid">The Liquid ID of the liquid colliding with this liquid</param>
	/// <returns></returns>
	public virtual bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify whether the liquid prevents the placement or blockswap of a tile over this liquid. Usually used by lava to stop players from placing ontop of it. <br/>
	/// Return true for the liquid to prevent tile placement, return false if the liquid can have tiles placed over it. <br/>
	/// Returns false by default.
	/// </summary>
	/// <param name="player">The player instance thats attempting to place a tile over the liquid.</param>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool BlocksTilePlacement(Player player, int i, int j)
	{
		return false;
	}

	/// <summary>
	/// Allows you to decide what happens when the player enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a player enters and leaves. <br/>
	/// Players now also have a moddedWet array to show which modded liquids are being entered in at a time. Please see <see cref="P:ModLiquidLib.Utils.ModLiquidPlayer.moddedWet" /> array on how to use it. <br/>
	/// Return false for the liquid to not execute the default modded liquid splash code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="player">The player instance thats entering or exiting the liquid.</param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	/// <returns></returns>
	public virtual bool OnPlayerSplash(Player player, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a NPC enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a NPC enters and leaves. <br/>
	/// NPCs now also have a moddedWet array to show which modded liquids are being entered in at a time. Please see <see cref="P:ModLiquidLib.Utils.ModLiquidPlayer.moddedWet" /> array on how to use it. <br/>
	/// Return false for the liquid to not execute the default modded liquid splash code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="npc">The NPC instance thats entering or exiting the liquid.</param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	/// <returns></returns>
	public virtual bool OnNPCSplash(NPC npc, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a projectile enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a projectile enters and leaves. <br/>
	/// Projectiles now also have a moddedWet array to show which modded liquids are being entered in at a time. Please see <see cref="P:ModLiquidLib.Utils.ModLiquidPlayer.moddedWet" /> array on how to use it. <br/>
	/// Return false for the liquid to not execute the default modded liquid splash code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="proj">The projectile instance thats entering or exiting the liquid.</param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	/// <returns></returns>
	public virtual bool OnProjectileSplash(Projectile proj, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when an item enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when an item enters and leaves. <br/>
	/// Items now also have a moddedWet array to show which modded liquids are being entered in at a time. Please see <see cref="P:ModLiquidLib.Utils.ModLiquidPlayer.moddedWet" /> array on how to use it. <br/>
	/// Return false for the liquid to not execute the default modded liquid splash code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="item">The item instance thats entering or exiting the liquid.</param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	/// <returns></returns>
	public virtual bool OnItemSplash(Item item, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a fishing bobber catches a fish. Water uses this to create extra water bubbles and to make a splash sound.		/// </summary>
	/// <param name="proj">The projectile instance thats fishing in the liquid.</param>
	/// <returns></returns>
	public virtual void OnFishingBobberSplash(Projectile proj)
	{
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with the player (especially player movement). <br/>
	/// Please see <see cref="P:Terraria.Player.WaterCollision" />, <see cref="P:Terraria.Player.HoneyCollision" />, or <see cref="P:Terraria.Player.ShimmerCollision" /> to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to use the normal liquid collision code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="fallThrough">Whether or not the player is falling through the liquid.</param>
	/// <param name="ignorePlats">Whether or not the player ignores platforms when falling.</param>
	/// <returns></returns>
	public virtual bool PlayerLiquidMovement(Player player, bool fallThrough, bool ignorePlats)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with the player (especially player gravity). <br/>
	/// Please see <see cref="P:Terraria.Player.Update" />, to see how vanilla handles it's liquid gravity.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="gravity">The current player.gravity being changed.</param>
	/// <param name="maxFallSpeed">The current player.maxFallSpeed being changed.</param>
	/// <param name="jumpHeight">The current Player.jumpHeight being changed.</param>
	/// <param name="jumpSpeed">The current Player.jumpSpeed being changed.</param>
	public virtual void PlayerGravityModifier(Player player, ref float gravity, ref float maxFallSpeed, ref int jumpHeight, ref float jumpSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with an item (especially item gravity and movement). <br/>
	/// Please see <see cref="P:Terraria.Item.UpdateItem" />, to see how vanilla handles it's liquid collision. <br/> <br/>
	/// This method is also used to modify items detected to be in liquids. Use this to do things such as deleting items touching this liquid.
	/// </summary>
	/// <param name="item">The item instance thats being effected by the liquid.</param>
	/// <param name="wetVelocity">The velocity of the item when in the liquid.</param>
	/// <param name="gravity">The gravity of the item.</param>
	/// <param name="maxFallSpeed">The maximum fall speed of the item.</param>
	public virtual void ItemLiquidCollision(Item item, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with an npc (especially npc movement). <br/>
	/// Please see <see cref="P:Terraria.NPC.UpdateCollision" />, to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to call the method that applies the liquid velocity multiplier. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="npc">The npc instance thats being effected by the liquid.</param>
	/// <param name="dryVelocity">The velocity of the NPC before being modified by the liquid.</param>
	/// <returns></returns>
	public virtual bool NPCLiquidMovement(NPC npc, Vector2 dryVelocity)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with an npc (especially npc gravity). <br/>
	/// Please see <see cref="P:Terraria.NPC.UpdateNPC_UpdateGravity" />, to see how vanilla handles it's liquid gravity.
	/// </summary>
	/// <param name="npc">The npc instance thats being effected by the liquid.</param>
	/// <param name="gravity">The gravity of the npc.</param>
	/// <param name="maxFallSpeed">The maximum fall speed of the npc.</param>
	public virtual void NPCGravityModifier(NPC npc, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with a projectile (especially projectile movement). <br/>
	/// Please see <see cref="P:Terraria.Projectile.HandleMovement" />, to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to use the normal liquid collision code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="projectile">The projectile instance thats being effected by the liquid.</param>
	/// <param name="wetVelocity">The velocity of the item when in the liquid.</param>
	/// <param name="collisionPosition">The position of where the projectile is calculated when colliding with this liquid.</param>
	/// <param name="Width">The modified width of this projectile.</param>
	/// <param name="Height">The modified height of this projectile.</param>
	/// <param name="fallThrough">Whether or not the projectile can fall through.</param>
	/// <returns></returns>
	public virtual bool ProjectileLiquidMovement(Projectile proj, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough)
	{
		return true;
	}

	/// <summary>
	/// Allows you to give conditions for when the game attempts to check to see if the player is drowning. <br/>
	/// Set isDrowning to either true or false depending on whether the player should or should not be drowning. <br/>
	/// This is used by items such as the Breething Reed to make the player be submerged deeper underwater. <br/>
	/// Please see <see cref="P:Terraria.Player.CheckDrowning" /> for how vanilla uses the isDrowning boolean flag. <br/>
	/// ChecksForDrowning must be set to true for this to run. Not to be confused with ChecksForDrowning.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="isDrowning">The boolean flag for whether or not the player is drowning.</param>
	public virtual void CanPlayerDrown(Player player, ref bool isDrowning)
	{
	}

	/// <summary>
	/// Allows manipulation of tiles when grass burns. This allows modders to make grasses burn with their liquid or do other interactions if a liquid is nearby a tile. <br/>
	/// Only effects tiles in a 9x9 area around a liquid and only executes every few seconds.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquidX">The x position of the liquid in tile coordinates.</param>
	/// <param name="liquidY">The y position of the liquid in tile coordinates.</param>
	public virtual void ModifyNearbyTiles(int i, int j, int liquidX, int liquidY)
	{
	}

	/// <summary>
	/// Allows modders to do extra interactions when their liquid is pumped. <br/>
	/// Works similarly to a Pre hook, executing before any liquids are moved for each pump. <br/>
	/// Return true for pumps to execute their normal liquid moving logic. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="inX">The x position of the in pump in tile coordinates.</param>
	/// <param name="inY">The y position of the in pump in tile coordinates.</param>
	/// <param name="outX">The x position of the out pump in tile coordinates.</param>
	/// <param name="outY">The y position of the out pump in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool OnPump(int inX, int inY, int outX, int outY)
	{
		return true;
	}

	/// <summary>
	/// Executed whenever the collision check for players is executed and found true.
	/// This is used for adding de/buffs, damaging the player or incrimenting/deincrementing timers or ModPlayer fields.
	/// Vanilla liquids use this to add buffs or debuffs such as On Fire!, Honey, or Shimmering.
	/// </summary>
	/// <param name="player">The player instance being effected by this liquid.</param>
	public virtual void OnPlayerCollision(Player player)
	{
	}

	/// <summary>
	/// Executed whenever the collision check for NPCs is executed and found true.
	/// This is used for adding debuffs, damaging the npc or incrimenting/deincrementing timers or GlobalNPC fields.
	/// Vanilla liquids use this to damage the NPC and apply On Fire! when touching lava.
	/// </summary>
	/// <param name="npc">The NPC instance being effected by this liquid.</param>
	public virtual void OnNPCCollision(NPC npc)
	{
	}

	/// <summary>
	/// Executed whenever the collision check for projectiles is executed and found true.
	/// This is used for changing projectile types, killing projectiles, and doing other misc effects.
	/// Vanilla liquids use this to transforming flaming arrows into arrows and killing flamethrower flames.
	/// </summary>
	/// <param name="proj">The Projectile instance being effected by this liquid.</param>
	public virtual void OnProjectileCollision(Projectile proj)
	{
	}

	/// <summary>
	/// Allows you to animate your liquid. <br/>
	/// Overriding this method will prevent normal vanilla liquid animations from playing, allowing you to animate without interference. <br/>
	/// Use frameState to keep track of how long the current frame has been active, and use frame to change the current frame. <br/>
	/// GameTime has also been provided to check the current seconds that has overlapped ingame.
	/// </summary>
	/// <param name="gameTime">The current amount of time that has passed since the game has been opened.</param>
	/// <param name="frame">The current frame the liquid is on.</param>
	/// <param name="frameState">The current frame counter the liquid is on.</param>
	public virtual void AnimateLiquid(GameTime gameTime, ref int frame, ref float frameState)
	{
	}

	/// <summary>
	/// Allows you to modify the ripple strength and offset when an NPC enters, exists and moves through this liquid. <br/>
	/// This is executed right before QueueRipple is called in Terraria.GameContent.Shaders.WaterShaderData.DrawWaves.
	/// </summary>
	/// <param name="npc">The NPC instance moving through the liquid.</param>
	/// <param name="rippleStrength">The strength and size of a ripple.</param>
	/// <param name="rippleOffset">The offset of a ripple.</param>
	public virtual void NPCRippleModifier(NPC npc, ref float rippleStrength, ref float rippleOffset)
	{
	}

	/// <summary>
	/// Allows you to modify the ripple strength and offset when a player enters, exists and moves through this liquid. <br/>
	/// This is executed right before QueueRipple is called in Terraria.GameContent.Shaders.WaterShaderData.DrawWaves.
	/// </summary>
	/// <param name="player">The player moving through the liquid.</param>
	/// <param name="rippleStrength">The strength and size of a ripple.</param>
	/// <param name="rippleOffset">The offset of a ripple.</param>
	public virtual void PlayerRippleModifier(Player player, ref float rippleStrength, ref float rippleOffset)
	{
	}
}
