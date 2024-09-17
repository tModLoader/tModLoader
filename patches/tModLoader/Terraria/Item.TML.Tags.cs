using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria;

public partial class Item : TagSerializable, IEntityWithGlobals<GlobalItem>
{
	/// <summary>
	/// A list of all dynamic content tags that apply to this item.<br/>
	/// Content tags are a wide variety of strings stored here, designed to help denote important details about an item.<br/>
	/// The tModLoader wiki provides a convenient list of content tags provided by tML, what they mean, and what effects they have, if any.<br/>
	/// </summary>
	public HashSet<string> DynamicTags { get; set; }

	/// <summary>
	/// Fetches tags for this item from both <see cref="ItemLoader.StaticItemTags"/> and <see cref="DynamicTags"/>.<br/>
	/// Anything which checks against an item having a specific tag should check this instead of its constituent parts.
	/// </summary>
	public HashSet<string> Tags => ItemLoader.StaticItemTags[type].Union(DynamicTags).ToHashSet();

	/// <summary>
	/// Automagically finishes population of an item's taglist AFTER <see cref="SetDefaults(int)"/> is run in full, including:<br/>
	/// - <see cref="ModItem.SetDefaults"/>, when applicable<br/>
	/// - All applicable <see cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/> hooks, if any<br/>
	/// Used for tags that cannot ask ID sets for help in the early stage or at all, such as tool or weapon type tags.<br/>
	/// </summary>
	public void DetermineAdditionalTags()
	{
		if (pick > 0 || axe > 0 || hammer > 0) {
			DynamicTags.Add("tool");
			if (pick > 0)
				DynamicTags.Add("tool/pickaxe");
			if (axe > 0)
				DynamicTags.Add("tool/axe");
			if (hammer > 0)
				DynamicTags.Add("tool/hammer");
		}

		if (damage > 0) {
			if (DamageType == DamageClass.Melee) {
				if (useStyle == ItemUseStyleID.Swing && !noMelee && !noUseGraphic && !DynamicTags.Contains("tool"))
					DynamicTags.Add("broadsword");
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.ShortSword)
					DynamicTags.Add("shortsword");
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.Flail)
					DynamicTags.Add("flail");
				if (shoot > 0 && ItemID.Sets.Yoyo[type] && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.Yoyo)
					DynamicTags.Add("yoyo");
			}
			if (DamageType == DamageClass.Ranged) {
				
			}
			if (DamageType == DamageClass.Magic) {
				
			}
			if (DamageType == DamageClass.Summon) {
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].minionSlots > 0)
					DynamicTags.Add("summon/minion");
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].sentry)
					DynamicTags.Add("summon/sentry");
				if (shoot > 0 && ProjectileID.Sets.IsAWhip[shoot])
					DynamicTags.Add("whip");
			}
		}

		if (createTile > -1 || createWall > -1)
		{
			DynamicTags.Add("placeable");
			if (createTile > -1) {
				DynamicTags.Add("placeable/foreground");
				if (ItemID.Sets.Torches[type] && TileID.Sets.Torch[createTile]) {
					DynamicTags.Add("placeable/foreground/torch");
					if (ItemID.Sets.WaterTorches[type])
						DynamicTags.Add("placeable/foreground/torch/valid_in_water");
				}
			}
			if (createWall > -1) {
				DynamicTags.Add("placeable/background");
			}
		}

		if (rare != ItemRarityID.White || ItemID.Sets.IsLavaImmuneRegardlessOfRarity[type])
			DynamicTags.Add("lava_immune");
	}
}