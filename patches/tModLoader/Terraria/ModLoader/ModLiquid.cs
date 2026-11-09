using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.Localization;
using LiquidCache = Terraria.GameContent.Liquid.LiquidRenderer.LiquidCache;
using LiquidDrawCache = Terraria.GameContent.Liquid.LiquidRenderer.LiquidDrawCache;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of liquid that can be added by a mod. Only one instance of this class will ever exist for each type of liquid that is added. Any hooks that are called will be called by the instance corresponding to the liquid type. This is to prevent the game from using a massive amount of memory storing liquid instances.<br />
/// </summary>
public abstract class ModLiquid : ModBlockType
{
	/// <summary> The block texture of this liquid. Used for some slope framing and when liquids are rendered in retro lighting modes. </summary>
	public virtual string BlockTexture => Texture + "_Block";

	/// <summary> The slope texture of this liquid. Used for rendering the liquids next to tile slopes. </summary>
	public virtual string SlopeTexture => Texture + "_Slope";

	/// <summary> The default sound that plays when an entity enters and exits this liquid. Overriding any OnSplash hooks/methods will prevent this from being used. </summary>
	public SoundStyle? SplashSound { get; set; }

	/// <summary> The default dust ID thats used when an entity enters and exists this liquid. Overriding any OnSplash hooks/methods will prevent this from being used. </summary>
	public int SplashDustType { get; set; } = -1;

	protected sealed override void Register()
	{
		ModTypeLookup<ModLiquid>.Register(this);
		Type = (ushort)LiquidLoader.ReserveLiquidID();
		LiquidLoader.liquids.Add(this);
	}

	public sealed override void SetupContent()
	{
		LiquidLoader.LiquidAssets[Type] = ModContent.Request<Texture2D>(Texture);
		LiquidLoader.LiquidBlockAssets[Type] = ModContent.Request<Texture2D>(BlockTexture);
		LiquidLoader.LiquidSlopeAssets[Type] = ModContent.Request<Texture2D>(SlopeTexture);
		LiquidRenderer.WATERFALL_LENGTH[Type] = 10;
		LiquidRenderer.DEFAULT_OPACITY[Type] = 0.6f;
		LiquidRenderer.VISCOSITY_MASK[Type] = 0;
		SetStaticDefaults();
	}

	/// <summary>
	/// Allows you to modify the properties after initial loading has completed.
	/// <br /> This is where you would set the properties of this liquid. Many properties are stored as arrays throughout Terraria's code.
	/// <br /> For example:
	/// <list type="bullet">
	/// <item> LiquidRenderer.WATERFALL_LENGTH[Type] = 10; </item>
	/// <item> LiquidRenderer.DEFAULT_OPACITY[Type] = 0.6f; </item>
	/// <item> LiquidRenderer.VISCOSITY_MASK[Type] = 0; </item>
	/// </list>
	/// </summary>
	public override void SetStaticDefaults()
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
	/// The ID of the waterfall/liquidfall style the game should use when this liquid is near a half block
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	public virtual int ChooseWaterfallStyle(int i, int j)
	{
		return 0;
	}

	/// <summary>
	/// Allow or not fishing in this liquid. Return true by default; 
	/// </summary>
	/// <param name="player">The player attempting to fish in it</param>
	/// <param name="bobber">The bobber that is trying to fish in the liquid</param>
	/// <returns></returns>
	public virtual bool AllowFishing(Player player, Projectile bobber)
	{
		return true;
	}

	/// <summary>
	/// Allows the changing of liquid movement in-game as well as allowing you to do other stuff with liquids they update, return false if you don't want your liquid to you vanilla update behavior
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquid"></param>
	/// <returns></returns>
	public virtual bool PreUpdate(int i, int j, Liquid liquid)
	{
		return true;
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
	/// <param name="adjacentLiquid">The liquid ID of the liquid colliding with this one.</param>
	/// <returns></returns>
	public virtual int Merge(int adjacentLiquid)
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
	public virtual void MergeSound(int i, int j, int otherLiquid, ref SoundStyle? collisionSound)
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
	public virtual bool PreMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid)
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
	/// Allows the user to specify how the liquid interacts with the player (especially player movement). <br/>
	/// Please see <see cref="Player.WaterCollision" />, <see cref="Player.HoneyCollision" />, or <see cref="Player.ShimmerCollision" /> to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to use the normal liquid collision code. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="player">The player instance thats being effected by the liquid.</param>
	/// <param name="fallThrough">Whether or not the player is falling through the liquid.</param>
	/// <param name="ignorePlats">Whether or not the player ignores platforms when falling.</param>
	/// <returns></returns>
	public virtual bool PlayerMovement(Player player, bool fallThrough, bool ignorePlats)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with an item (especially item gravity and movement). <br/>
	/// Please see <see cref="Item.UpdateItem" />, to see how vanilla handles it's liquid collision. <br/> <br/>
	/// This method is also used to modify items detected to be in liquids. Use this to do things such as deleting items touching this liquid.
	/// </summary>
	/// <param name="item">The item instance thats being effected by the liquid.</param>
	/// <param name="wetVelocity">The velocity of the item when in the liquid.</param>
	/// <param name="gravity">The gravity of the item.</param>
	/// <param name="maxFallSpeed">The maximum fall speed of the item.</param>
	public virtual void ItemCollision(WorldItem item, ref Vector2 wetVelocity, ref float gravity, ref float maxFallSpeed)
	{
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with an npc (especially npc movement). <br/>
	/// Please see <see cref="NPC.UpdateCollision" />, to see how vanilla handles it's liquid collision. <br/>
	/// Return true for the liquid to call the method that applies the liquid velocity multiplier. <br/>
	/// Returns true by default.
	/// </summary>
	/// <param name="npc">The npc instance thats being effected by the liquid.</param>
	/// <param name="dryVelocity">The velocity of the NPC before being modified by the liquid.</param>
	/// <returns></returns>
	public virtual bool NPCMovement(NPC npc, Vector2 dryVelocity)
	{
		return true;
	}

	/// <summary>
	/// Allows the user to specify how the liquid interacts with a projectile (especially projectile movement). <br/>
	/// Please see <see cref="Projectile.HandleMovement" />, to see how vanilla handles it's liquid collision. <br/>
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
	public virtual bool ProjectileMovement(Projectile proj, ref Vector2 wetVelocity, Vector2 collisionPosition, int Width, int Height, bool fallThrough)
	{
		return true;
	}

	/// <summary>
	/// Allows you to give conditions for when the game attempts to check to see if the player is drowning. <br/>
	/// Return true if the player should be drowning in the current liquid. <br/>
	/// Return false if the player should not be drowning in the current liquid <br/>
	/// This is used by items such as the Breathing Reed to make the player be submerged deeper underwater. <br/>
	/// </summary>
	/// <param name="player">The player instance that being effected by the liquid.</param>
	public virtual bool CanPlayerDrown(Player player)
	{
		return true;
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

	public override string LocalizationCategory => "Liquids";

	/// <summary>
	/// A display name for the liquid. May be used as a default impl for recipe conditions, map names and other mod features.
	/// </summary>
	public virtual LocalizedText DisplayName => this.GetLocalization(nameof(DisplayName), PrettyPrintName);

	/// <inheritdoc/>
	public override void AddMapEntry(Color color, LocalizedText name = null, Func<string, int, int, string> nameFunc = null)
	{
		AddMapEntryToMapLoader(MapLoader.liquidEntries, color, name, nameFunc);
	}

	/// <summary>
	/// Allows you to draw things behind the liquid at the given coordinates. Return false to stop the game from drawing the liquid normally. Returns true by default. It is important to use Main.tileBatch for drawing operation here.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="PreRetroDraw" /> for the other Lighting modes pre drawing
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
	/// Allows you to draw things in front of the liquid at the given coordinates. This can also be used to do things such as rendering glowmasks. It is important to use Main.tileBatch for drawing operation here.<para />
	/// This hook is called regardless if <see cref="PreDraw" /> returns true or false.<para />
	/// This method is only called in "Color" and "White" lighting modes. Use <see cref="PostRetroDraw" /> for the other Lighting modes post drawing
	/// </summary>
	/// <param name="i">The x position in tile coordinates.</param>
	/// <param name="j">The y position in tile coordinates.</param>
	/// <param name="liquidDrawCache">The LiquidDrawCache of the rendering</param>
	/// <param name="drawOffset">The amount the liquid rendered is offset by</param>
	/// <param name="isBackgroundDraw">Whether or not the liquid is in the background</param>
	public virtual void PostDraw(int i, int j, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw)
	{
	}
}
