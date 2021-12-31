using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.NetModules;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Net;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.GameContent.Creative
{
	public partial class CreativeUI
	{

		/// <summary>
		/// Conveniance method that allows you to fully research an item without needing to have sufficent of that item
		/// </summary>
		/// <param name="toSacrifice"></param>
		/// <param name="alreadyResearched"> true if it was already researched, false otherwise</param>
		/// <returns></returns>
		public static ItemSacrificeResult ResearchItem(Item toSacrifice, out bool alreadyResearched) {
			return ResearchItem(toSacrifice.type, out alreadyResearched);
		}
		/// <summary>
		/// Conveniance method that allows you to fully research an item without needing to have an item, just a type.
		/// </summary>
		/// <param name="type">Type of the item to reseach</param>
		/// <param name="alreadyResearched"> true if it was already researched, false otherwise</param>
		/// <returns></returns>
		public static ItemSacrificeResult ResearchItem(int type, out bool alreadyResearched) {
			int amountNeeded = 0;
			alreadyResearched = false;
			Item item = new Item();
			item.SetDefaults(type, false);
			bool? canSacrifice = ItemLoader.CanResearch(item);
			if (canSacrifice != null && canSacrifice.Value == false)
				return ItemSacrificeResult.CannotSacrifice;

			int num = 0;
			if (canSacrifice == null && !CreativeItemSacrificesCatalog.Instance.TryGetSacrificeCountCapToUnlockInfiniteItems(type, out amountNeeded))
				return ItemSacrificeResult.CannotSacrifice;
		
			int value = 0;
			Main.LocalPlayerCreativeTracker.ItemSacrifices.SacrificesCountByItemIdCache.TryGetValue(type, out value);
			num = Utils.Clamp(amountNeeded - value, 0, amountNeeded);
			alreadyResearched = num == 0;
			if (canSacrifice == null && alreadyResearched)
					return ItemSacrificeResult.CannotSacrifice;
			
			if (!Main.ServerSideCharacter) {
				Main.LocalPlayerCreativeTracker.ItemSacrifices.RegisterItemSacrifice(type, num);
			}
			else {
				NetPacket packet = NetCreativeUnlocksPlayerReportModule.SerializeSacrificeRequest(type, num);
				NetManager.Instance.SendToServerOrLoopback(packet);
			}

			ItemLoader.OnResearched(item, true);
			return ItemSacrificeResult.SacrificedAndDone;
		}
	}
}
