using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria;

public partial class Item : TagSerializable, IEntityWithGlobals<GlobalItem>
{
	/// <summary>
	/// A list of all content tags that apply to this item.<br/>
	/// Content tags are a wide variety of strings stored here, designed to help denote what an item is.<br/>
	/// For a list of all available content tags please refer to [REDACTED].<br/>
	/// </summary>
	public HashSet<string> Tags { get; set; }

	/// <summary>
	/// Automagically partially populates an item's taglist BEFORE the rest of <see cref="SetDefaults(int)"/> is run.<br/>
	/// Uses ID sets to help identify applicable tags for an item early.<br/>
	/// </summary>
	public void PopulateTaglistEarly()
	{
		Tags = [];

		if (ItemID.Sets.IsFood[type])
			Tags.Add("food");

		if (ItemID.Sets.IsAKite[type])
			Tags.Add("kite");

		if (ItemID.Sets.IsFishingCrate[type]) {
			Tags.Add("fishing_crate");
			if (ItemID.Sets.IsFishingCrateHardmode[type])
				Tags.Add("fishing_crate/hardmode");
		}

		if (ItemID.Sets.BossBag[type]) {
			Tags.Add("boss_bag");
			if (ItemID.Sets.PreHardmodeLikeBossBag[type])
				Tags.Add("boss_bag/prehardmode");
		}

		if (ItemID.Sets.IsChainsaw[type] || ItemID.Sets.IsDrill[type]) {
			Tags.Add("motorized_tool");
			if (ItemID.Sets.IsChainsaw[type])
				Tags.Add("motorized_tool/chainsaw");
			if (ItemID.Sets.IsDrill[type])
				Tags.Add("motorized_tool/drill");
		}
	}

	/// <summary>
	/// Automagically finishes population of an item's taglist AFTER <see cref="SetDefaults(int)"/> is run in full, including:<br/>
	/// - <see cref="ModItem.SetDefaults"/>, when applicable<br/>
	/// - All applicable <see cref="GlobalType{TEntity, TGlobal}.SetDefaults(TEntity)"/> hooks, if any<br/>
	/// Used for tags that cannot ask ID sets for help in the early stage or at all, such as tool or weapon type tags.<br/>
	/// </summary>
	public void PopulateTaglistLate()
	{
		if (pick > 0 || axe > 0 || hammer > 0) {
			Tags.Add("tool");
			if (pick > 0)
				Tags.Add("tool/pickaxe");
			if (axe > 0)
				Tags.Add("tool/axe");
			if (hammer > 0)
				Tags.Add("tool/hammer");
		}

		if (damage > 0) {
			if (DamageType == DamageClass.Melee) {
				if (useStyle == ItemUseStyleID.Swing && !noMelee && !noUseGraphic && !Tags.Contains("tool"))
					Tags.Add("broadsword");
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.ShortSword)
					Tags.Add("shortsword");
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.Flail)
					Tags.Add("flail");
				if (shoot > 0 && ItemID.Sets.Yoyo[type] && ContentSamples.ProjectilesByType[shoot].aiStyle == ProjAIStyleID.Yoyo)
					Tags.Add("yoyo");
			}
			if (DamageType == DamageClass.Ranged) {
				
			}
			if (DamageType == DamageClass.Magic) {
				
			}
			if (DamageType == DamageClass.Summon) {
				if (shoot > 0 && ContentSamples.ProjectilesByType[shoot].minionSlots > 0)
					Tags.Add("summon/minion");
				if (shoot > 0 && ProjectileID.Sets.IsAWhip[shoot])
					Tags.Add("summon/sentry");
				if (shoot > 0 && ProjectileID.Sets.IsAWhip[shoot])
					Tags.Add("whip");
			}
		}

		if (createTile > -1 || createWall > -1)
		{
			Tags.Add("placeable");
			if (createTile > -1) {
				Tags.Add("placeable/foreground");
				if (ItemID.Sets.Torches[type] && TileID.Sets.Torch[createTile]) {
					Tags.Add("placeable/foreground/torch");
					if (ItemID.Sets.WaterTorches[type])
						Tags.Add("placeable/foreground/torch/valid_in_water");
				}
			}
			if (createWall > -1) {
				Tags.Add("placeable/background");
			}
		}
	}
}