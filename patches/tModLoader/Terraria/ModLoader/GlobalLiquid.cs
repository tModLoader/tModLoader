using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using static Terraria.GameContent.Liquid.LiquidRenderer;

namespace Terraria.ModLoader;
/// <summary>
/// This class allows you to modify the behavior of any liquid in the game, both vanilla and modded.
/// <br /> To use it, simply create a new class deriving from this one. Implementations will be registered automatically.
/// </summary>
public abstract class GlobalLiquid : ModType
{
	protected sealed override void Register()
	{
		ModTypeLookup<GlobalLiquid>.Register(this);
		LiquidLoader.globalLiquids.Add(this);
	}

	public sealed override void SetupContent() => SetStaticDefaults();

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="GlobalLiquid.PreRetroDraw" /> for the other Lighting modes pre drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquidDrawCache">The LiquidDrawCache of the rendering</param>
	/// <param name="drawOffset">The amount the liquid rendered is offset by</param>
	/// <param name="isBackgroundDraw">Whether or not the liquid is in the background</param>
	public virtual bool PreDraw(int i, int j, int type, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw, int waterStyle, float waterAlpha)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="GlobalLiquid.PreDraw" /> returns false.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="GlobalLiquid.PostRetroDraw" /> for the other Lighting modes post drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquidDrawCache">The LiquidDrawCache of the rendering</param>
	/// <param name="drawOffset">The amount the liquid rendered is offset by</param>
	/// <param name="isBackgroundDraw">Whether or not the liquid is in the background</param>
	public virtual void PostDraw(int i, int j, int type, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw, int waterStyle, float waterAlpha)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the liquid at the given coordinates is drawn. For example, creating bubble dusts or changing the color the liquid is drawn in. <para />
	/// This hook is seperate from Liquid Rendering, Only use this for things such as spawning particles. <para />
	/// Return false to stop Lava from spawning some of its bubbles. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquidCache">Various information about the liquid that is being drawn, such as opacity, visual liquid, etc.</param>
	public virtual bool EmitEffects(int i, int j, int type, LiquidCache liquidCache)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default.<para />
	/// This method is only called in "Retro" and "Trippy" lighting modes. Use <see cref="GlobalLiquid.PreDraw" /> for the other Lighting modes pre drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="spriteBatch"></param>
	public virtual bool PreRetroDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="GlobalLiquid.PreRetroDraw" /> returns false.<para />
	/// This method is only called in "Retro" and "Trippy" lighting modes. Use <see cref="GlobalLiquid.PostDraw" /> for the other Lighting modes post drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="spriteBatch"></param>
	public virtual void PostRetroDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
	}

	/// <summary>
	/// Allows you to make stuff happen whenever the liquid at the given coordinates is drawn. For example, creating bubble dusts or changing the color the liquid is drawn in. <para />
	/// This hook is not called if <see cref="GlobalLiquid.PreRetroDraw" /> returns false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="spriteBatch"></param>
	/// <param name="drawData">Various information about the liquid that is being drawn, such as color, opacity, waterfall length, etc.</param>
	/// <param name="liquidAmountModified">Gets the amount of liquid in a tile for retro rendering</param>
	/// <param name="liquidGFXQuality">Gets the number of quality the game has for retro rendering</param>
	public virtual void RetroDrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref RetroLiquidDrawInfo drawData, float liquidAmountModified, int liquidGFXQuality)
	{
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default. <para />
	/// Only called for liquid slopes. If you want to predraw the liquid itself, see <see cref="GlobalLiquid.PreDraw" /> or <see cref="GlobalLiquid.PreRetroDraw" />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="behindBlocks">Whether or not the slope is rendered behind tiles</param>
	/// <param name="drawPosition">The rendering position of the slope. Use this rather than x/y as the rendering can be offset sometimes</param>
	/// <param name="liquidSize">The size of the liquid</param>
	/// <param name="colors">The color of the liquid. Usually the light color of the liquid</param>
	public virtual bool PreSlopeDraw(int i, int j, int type, bool behindBlocks, ref Vector2 drawPosition, ref Rectangle liquidSize, ref VertexColors colors)
	{
		return true;
	}

	/// <summary>
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks.<para />
	/// This hook is not called if <see cref="GlobalLiquid.PreSlopeDraw" /> returns false.<para />
	/// Only called for liquid slopes. If you want to postdraw the liquid itself, see <see cref="GlobalLiquid.POstDraw" /> or <see cref="GlobalLiquid.PostRetroDraw" />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="behindBlocks">Whether or not the slope is rendered behind tiles</param>
	/// <param name="drawPosition">The rendering position of the slope. Use this rather than x/y as the rendering can be offset sometimes</param>
	/// <param name="liquidSize">The size of the liquid</param>
	/// <param name="colors">The color of the liquid. Usually the light color of the liquid</param>
	public virtual void PostSlopeDraw(int i, int j, int type, bool behindBlocks, ref Vector2 drawPosition, ref Rectangle liquidSize, ref VertexColors colors)
	{
	}

	/// <summary>
	/// Allows you to determine how much light this liquid emits.<br />
	/// It can also let you light up the block in front of this liquid.<br />
	/// See <see cref="TileLightScanner.ApplyLiquidLight(Tile, int, int, ref Vector3)" /> for vanilla tile light values to use as a reference.<br />
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="r">The red component of light, usually a value between 0 and 1</param>
	/// <param name="g">The green component of light, usually a value between 0 and 1</param>
	/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
	public virtual void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
	{
	}

	/// <summary>
	/// Allows for disabling lava bubbles in "Retro" and "Trippy" lighting modes. Only calls when lava's PreRetroDraw isnt false. Returns false by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool DisableRetroLavaBubbles(int i, int j)
	{
		return false;
	}

	/// <summary>
	/// The ID of the waterfall/liquidfall style the game should use when the liquid is near a half block.
	/// Returns null by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	public virtual int? ChooseWaterfallStyle(int i, int j, int type)
	{
		return null;
	}

	/// <summary>
	/// Allows the changing of liquid movement in-game as well as allowing you to do other stuff with liquids during updates such as custom evaporation. Returns true by default. 
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquid"></param>
	/// <returns></returns>
	public virtual bool UpdateLiquid(int i, int j, int type, Liquid liquid)
	{
		return true;
	}

	/// <summary>
	/// Allows you to make liquids evaporate in the underworld. Also allows for disabling of water evaporation. Returns null by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool? EvaporatesInHell(int i, int j, int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to change how liquids move when loading or creating a world. Returns true by default.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool SettleLiquidMovement(int i, int j, int type)
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
	/// <param name="type"></param>
	/// <param name="otherLiquid">The liquid ID of the liquid colliding with this one.</param>
	/// <returns></returns>
	public virtual int? LiquidMerge(int i, int j, int type, int otherLiquid)
	{
		return null;
	}

	/// <summary>
	/// Used to modify the sound thats played when two liquids merge with each other. <br/>
	/// Called every time liquids collide and only on clients. <br/>
	/// Is not called if PreLiquidMerge returns false.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="otherLiquid">The liquid ID of the liquid colliding with this one.</param>
	/// <param name="collisionSound">The sound to be played, set this to a sound if you don't want the default merge sound to play.</param>
	public virtual void LiquidMergeSound(int i, int j, int type, int otherLiquid, ref SoundStyle? collisionSound)
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
	/// <param name="type"></param>
	/// <param name="otherLiquid">The Liquid ID of the liquid colliding with this liquid</param>
	/// <returns></returns>
	public virtual bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int type, int otherLiquid)
	{
		return true;
	}

	/// <summary>
	/// Allows the editing of how fast liquids fall, return null for the regular liquid falling to apply. <br/>
	/// For Modded Liquids, use ModLiquid.FallDelay property to edit how fast your liquid falls. This method can be used to edit other mod liquids falling as well.<br/>
	/// Returns null by default <br/>
	/// NOTE: liquids can only have a maximum fall delay of 10 (which is the same as the fall of Honey), this is due to the delay being reset to 10 when in quickfall mode.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual int? LiquidFallDelay(int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to determine which liquids the given liquid type can be considered as when looking for avaliable liquids.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual int[] AdjLiquids(int type)
	{
		return Array.Empty<int>();
	}

	/// <summary>
	/// Allows the user to specify whether the liquid prevents the placement or blockswap of a tile over this liquid. Usually used by lava to stop players from placing ontop of it. <br/>
	/// Return true for the liquid to prevent tile placement, return false if the liquid can have tiles placed over it. Return null for the default logic to run.<br/>
	/// Returns null by default.
	/// </summary>
	/// <param name="player">The player instance thats attempting to place a tile over the liquid.</param>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool? BlocksTilePlacement(Player player, int i, int j, int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to decide what happens when the player enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a player enters and leaves. <br/>
	/// Return true to allow the liquids to call their normal splash code. Returns true by default.
	/// </summary>
	/// <param name="player">The player instance thats entering or exiting the liquid.</param>
	/// <param name="type"></param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	public virtual bool OnPlayerSplash(Player player, int type, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a NPC enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a NPC enters and leaves. <br/>
	/// Return true to allow the liquids to call their normal splash code. Returns true by default.
	/// </summary>
	/// <param name="npc">The NPC instance thats entering or exiting the liquid.</param>
	/// <param name="type"></param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	public virtual bool OnNPCSplash(NPC npc, int type, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a projectile enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when a projectile enters and leaves. <br/>
	/// Return true to allow the liquids to call their normal splash code. Returns true by default.
	/// </summary>
	/// <param name="proj">The projectile instance thats entering or exiting the liquid.</param>
	/// <param name="type"></param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	public virtual bool OnProjectileSplash(Projectile proj, int type, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when a fishing bobber catches a fish. Water uses this to create extra water bubbles and to make a splash sound. <br/>
	/// Return false to disable the water splashing fishing bobbers create.
	/// </summary>
	/// <param name="proj">The projectile instance thats fishing in the liquid.</param>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool OnFishingBobberSplash(Projectile proj, int type)
	{
		return true;
	}

	/// <summary>
	/// Allows you to decide what happens when an item enters and exits this liquid. Vanilla liquids use this to spawn dusts and make a splashing noise when an item enters and leaves. <br/>
	/// Return true to allow the liquids to call their normal splash code. Returns true by default.
	/// </summary>
	/// <param name="item">The item instance thats entering or exiting the liquid.</param>
	/// <param name="type"></param>
	/// <param name="isEnter">Whether the currently the liquid is being entered or exited.</param>
	public virtual bool OnItemSplash(Item item, int type, bool isEnter)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with the player (especially player movement). <br/>
	/// Please see <see cref="Terraria.Player.WaterCollision" />, <see cref="Terraria.Player.HoneyCollision" />, or <see cref="Terraria.Player.ShimmerCollision" /> to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to use the normal liquid collision code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="fallThrough">Whether or not the player is falling through the liquid.</param>
	/// <param name="ignorePlats">Whether or not the player ignores platforms when falling.</param>
	/// <returns></returns>
	public virtual bool PlayerLiquidMovement(Player player, int type, bool fallThrough, bool ignorePlats)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with the player (especially player gravity). <br/>
	/// Please see <see cref="Terraria.Player.Update" />, to see how vanilla handles it's liquid gravity.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="gravity">The current player.gravity being changed.</param>
	/// <param name="maxFallSpeed">The current player.maxFallSpeed being changed.</param>
	/// <param name="jumpHeight">The current Player.jumpHeight being changed.</param>
	/// <param name="jumpSpeed">The current Player.jumpSpeed being changed.</param>
	public virtual void PlayerGravityModifier(Player player, int type, ref float gravity, ref float maxFallSpeed, ref int jumpHeight, ref float jumpSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with an item (especially item gravity and movement). <br/>
	/// Please see <see cref="Terraria.Item.UpdateItem" />, to see how vanilla handles it's liquid collision. <br/> <br/>
	/// This method is also used to modify items detected to be in liquids. Use this to do things such as deleting items touching this liquid.
	/// </summary>
	/// <param name="item">The item instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="wetVelocity">The velocity of the item when in the liquid.</param>
	/// <param name="gravity">The gravity of the item.</param>
	/// <param name="maxFallSpeed">The maximum fall speed of the item.</param>
	public virtual void ItemLiquidCollision(Item item, int type, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with a npc (especially npc movement). <br/>
	/// Please see <see cref="Terraria.NPC.UpdateCollision" />, to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to call the method that applies the liquid velocity multiplier. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="npc">The npc instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="dryVelocity">The velocity of the NPC before being modified by the liquid.</param>
	/// <returns></returns>
	public virtual bool NPCLiquidMovement(NPC npc, int type, Vector2 dryVelocity)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with an npc (especially npc gravity). <br/>
	/// Please see <see cref="Terraria.NPC.UpdateNPC_UpdateGravity" />, to see how vanilla handles it's liquid collision.
	/// </summary>
	/// <param name="npc">The npc instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="gravity">The gravity of the npc.</param>
	/// <param name="maxFallSpeed">The maximum fall speed of the npc.</param>
	public virtual void NPCGravityModifier(NPC npc, int type, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how a liquid interacts with a projectile (especially projectile movement). <br/>
	/// Please see <see cref="Terraria.Projectile.HandleMovement" />, to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to use the normal liquid collision code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="projectile">The projectile instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="wetVelocity">The velocity of the item when in the liquid.</param>
	/// <param name="collisionPosition">The position of where the projectile is calculated when colliding with this liquid.</param>
	/// <param name="Width">The modified width of this projectile.</param>
	/// <param name="Height">The modified height of this projectile.</param>
	/// <param name="fallThrough">Whether or not the projectile can fall through.</param>
	/// <returns></returns>
	public virtual bool ProjectileLiquidMovement(Projectile projectile, int type, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough)
	{
		return true;
	}

	/// <summary>
	/// Hook for deciding whether a liquid should allow or disallow, when overtop of the Collision.CheckDrowning position, to be considered drowning or not. <br/>
	/// Not to be confused with CanPlayersDrown, which is a hook called every frame to check if the player is drowning or not. Useful for making items such as a Breething Reed. <br/>
	/// Return null to execute the normal Collision.CheckDrowning behaviour. Returns null by default.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool? ChecksForDrowning(int type)
	{
		return null;
	}

	/// <summary>
	/// Hook for deciding whether a liquid should emit breath dusts when a player or npc is slowly drowning. <br/>
	/// ChecksForDrowning must be set to true for this to run.
	/// Return null to execute the normal breath behaviour. Returns null by default.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public virtual bool? AllowEmitBreathBubbles(int type)
	{
		return null;
	}

	/// <summary>
	/// Allows you to give conditions for when the game attempts to check to see if the player is drowning. <br/>
	/// Set isDrowning to either true or false depending on whether the player should or should not be drowning. <br/>
	/// This is used by items such as the Breething Reed to make the player be submerged deeper underwater. <br/>
	/// Please see <see cref="Terraria.Player.CheckDrowning" /> for how vanilla uses the isDrowning boolean flag. <br/>
	/// ChecksForDrowning must be set to true for this to run. Not to be confused with ChecksForDrowning hook.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="type"></param>
	/// <param name="isDrowning">The boolean flag for whether or not the player is drowning.</param>
	public virtual void CanPlayerDrown(Player player, int type, ref bool isDrowning)
	{
	}

	/// <summary>
	/// The multiper responsable for the poolsize of liquid being fished in. <br/>
	/// Honey uses this to multiply its pool size by 1.5x, this means that honey only needs a minimum of 50 tiles rather than 75 tiles of liquid.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="multiplier">The multiplier used, for most liquids this will be 1, but for honey it's 1.5.</param>
	public virtual void LiquidFishingPoolSizeMulitplier(int type, ref float multiplier)
	{
	}

	/// <summary>
	/// Allows bobbers to ignore the application of the shimmer effect when being wet by shimmer. <br/>
	/// This allows modders to easily enable fishing in shimmer. Do keep in mind, the fishing loot normally is water.
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowFishingInShimmer()
	{
		return false;
	}

	/// <summary>
	/// The opacity liquid slopes render at. Lava uses this to render at 1f opacity, and honey does the following calculation for it's opacity: <br/>
	/// Math.Max(0.5f * 1.7f, 1f) <br/>
	/// The game does additional calulations after this hook to get the final opacity. Use this method to edit liquid slope's default opacity to render at. <br/>
	/// slopeOpacity defaults at 0.5f.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="slopeOpacity">The opacity already given to a liquid. Edit this to change the default liquid slope opacity.</param>
	public virtual void LiquidSlopeOpacity(int type, ref float slopeOpacity)
	{
	}

	/// <summary>
	/// Allows you to change the Light Mask Mode for any liquid. The Light Mask Mode is the mask for lighting to determine how light interacts with a liquid. <br/>
	/// Vanilla has options for Water (slight blue fade) and Honey (dark black) while lava uses the None option. <br/>
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquidMaskMode">Edit this param to change the Light Mask Mode of the liquid.</param>
	public virtual void LiquidLightMaskMode(int i, int j, int type, ref LightMaskMode liquidMaskMode)
	{
	}

	/// <summary>
	/// The multiplier used when the Waves qaulity setting is set to Medium. <br/>
	/// Honey and Lava set this multiplier to be 0.3x. <br/>
	/// This setting is to make waves go the same distance no matter the liquid. For some reason vanilla manually applies a multiplier rather than doing seperate math for the wave speed.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="multiplier">The multiplier used, for most liquids this will be 1f, but for honey and lava it's 0.3.</param>
	public virtual void WaterRippleMultiplier(int type, ref float multiplier)
	{
	}

	/// <summary>
	/// Allows manipulation of tiles when grass burns. This allows modders to make grasses burn with a liquid or do other interactions if a liquid is nearby a tile. <br/>
	/// Only effects tiles in a 9x9 area around a liquid and only executes every few seconds. <br/>
	/// Can be used to allow modders to make sure their grasses burn when near lava.
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="type"></param>
	/// <param name="liquidX">The x position of the liquid in tile coordinates.</param>
	/// <param name="liquidY">The y position of the liquid in tile coordinates.</param>
	public virtual void ModifyNearbyTiles(int i, int j, int type, int liquidX, int liquidY)
	{
	}

	/// <summary>
	/// Allows modders to do extra interactions when a liquid is pumped. <br/>
	/// Works similarly to a Pre hook, executing before any liquids are moved for each pump. <br/>
	/// Return true for pumps to execute their normal liquid moving logic. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="inLiquidType"></param>
	/// <param name="inX">The x position of the in pump in tile coordinates.</param>
	/// <param name="inY">The y position of the in pump in tile coordinates.</param>
	/// <param name="outX">The x position of the out pump in tile coordinates.</param>
	/// <param name="outY">The y position of the out pump in tile coordinates.</param>
	/// <returns></returns>
	public virtual bool OnPump(int inLiquidType, int inX, int inY, int outX, int outY)
	{
		return true;
	}

	/// <summary>
	/// A hook to modify a liquid's stopwatch MPH multiplier whether that be modder or vanilla liquids. <br/>
	/// Water has its multiplier set to 0.5, honey 0.25 and shimmer 0.375 as an example of existing values.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="multiplier">The multiplier for the stopwatch MPH.</param>
	public virtual void StopWatchMPHMultiplier(int type, ref float multiplier)
	{
	}

	/// <summary>
	/// Executed whenever the collision check for players is executed and found true.
	/// This is used for adding de/buffs, damaging the player or incrimenting/deincrementing timers or ModPlayer fields.
	/// Vanilla liquids use this to add buffs or debuffs such as On Fire!, Honey, or Shimmering.
	/// </summary>
	/// <param name="player">The player instance being effected by this liquid.</param>
	/// <param name="type"></param>
	public virtual void OnPlayerCollision(Player player, int type)
	{
	}

	/// <summary>
	/// Executed whenever the collision check for NPCs is executed and found true.
	/// This is used for adding debuffs, damaging the npc or incrimenting/deincrementing timers or GlobalNPC fields.
	/// Vanilla liquids use this to damage the NPC and apply On Fire! when touching lava.
	/// </summary>
	/// <param name="npc">The NPC instance being effected by this liquid.</param>
	/// <param name="type"></param>
	public virtual void OnNPCCollision(NPC npc, int type)
	{
	}

	/// <summary>
	/// Executed whenever the collision check for projectiles is executed and found true.
	/// This is used for changing projectile types, killing projectiles, and doing other misc effects.
	/// Vanilla liquids use this to transforming flaming arrows into arrows and killing flamethrower flames.
	/// </summary>
	/// <param name="proj">The Projectile instance being effected by this liquid.</param>
	/// <param name="type"></param>
	public virtual void OnProjectileCollision(Projectile proj, int type)
	{
	}

	/// <summary>
	/// Allows you to animate a liquid. <br/>
	/// Returning false will cause the liquid/s to nolonger play an animation. <br/>
	/// Use frameState to keep track of how long the current frame has been active, and use frame to change the current frame. <br/>
	/// GameTime has also been provided to check the current seconds that has overlapped ingame. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="type"></param>
	/// <param name="gameTime">The current amount of time that has passed since the game has been opened.</param>
	/// <param name="frame">The current frame the liquid is on.</param>
	/// <param name="frameState">The current frame counter the liquid is on.</param>
	/// <returns></returns>
	public virtual bool AnimateLiquid(int type, GameTime gameTime, ref int frame, ref float frameState)
	{
		return true;
	}

	/// <summary>
	/// Allows you to modify the ripple strength and offset when an NPC enters, exists and moves through a liquid. <br/>
	/// This is executed right before QueueRipple is called in Terraria.GameContent.Shaders.WaterShaderData.DrawWaves.
	/// </summary>
	/// <param name="npc">The NPC instance moving through the liquid.</param>
	/// <param name="type"></param>
	/// <param name="rippleStrength">The strength and size of a ripple.</param>
	/// <param name="rippleOffset">The offset of a ripple.</param>
	public virtual void NPCRippleModifier(NPC npc, int type, ref float rippleStrength, ref float rippleOffset)
	{
	}

	/// <summary>
	/// Allows you to modify the ripple strength and offset when a player enters, exists and moves through a liquid. <br/>
	/// This is executed right before QueueRipple is called in Terraria.GameContent.Shaders.WaterShaderData.DrawWaves.
	/// </summary>
	/// <param name="player">The player moving through the liquid.</param>
	/// <param name="type"></param>
	/// <param name="rippleStrength">The strength and size of a ripple.</param>
	/// <param name="rippleOffset">The offset of a ripple.</param>
	public virtual void PlayerRippleModifier(Player player, int type, ref float rippleStrength, ref float rippleOffset)
	{
	}
}
