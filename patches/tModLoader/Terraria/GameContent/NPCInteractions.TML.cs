using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Terraria.GameContent;

public static partial class NPCInteractions
{
	public static partial class Actions
	{
		public class OpenShop2 : NPCInteraction
		{
			private string _shopName;
			private int _npcType;
			private string _customTextKey;

			public OpenShop2(int npcType, string shopName, string customTextKey = null)
			{
				_npcType = npcType;
				_shopName = shopName;
				_customTextKey = customTextKey;
			}

			public override bool Condition() => base.TalkNPCType == _npcType;

			public override string GetText()
			{
				if (_customTextKey != null)
					return Language.GetTextValue(_customTextKey);

				return Lang.inter[28].Value;
			}

			public override void Interact()
			{
				Main.instance.OpenShop(_shopName);
			}
		}

		public class ModCloseChat(int type) : CloseChat
		{
			public override bool Condition() => Main.LocalPlayer.TalkNPC.type == type;
		}

		public class ModReportHappiness(int type) : NPCInteraction
		{
			public override bool Condition()
			{
				if (Main.LocalPlayer.TalkNPC.type != type)
					return false;

				if (NPC.CanShowHomelessText(Main.LocalPlayer.talkNPC))
					return false;

				return base.LocalPlayer.currentShoppingSettings.HappinessReport != "";
			}

			public override string GetText() => Language.GetTextValue("UI.NPCCheckHappiness");

			public override void Interact()
			{
				Main.npcChatCornerItem = 0;
				SoundEngine.PlaySound(12);
				Main.npcChatText = base.LocalPlayer.currentShoppingSettings.HappinessReport;
				Main.DoNPCPortraitHop();
			}
		}

		public class ModRequestHome(int type) : RequestHome
		{
			public override bool Condition() => NPC.CanShowHomelessText(Main.LocalPlayer.talkNPC) && Main.LocalPlayer.TalkNPC.type == type;
		}
	}

	public static void InitializeModded()
	{
		for (int i = NPCID.Count; i < NPCLoader.NPCCount; i++) {
			bool skipCloseChat = false;
			bool skipReportHappiness = false;
			bool skipRequestHome = false;
			NPCLoader.SetChatButtons2(i, ref skipCloseChat, ref skipReportHappiness, ref skipRequestHome);

			if (!skipCloseChat)
				Register(new Actions.ModCloseChat(i));
			if (!skipReportHappiness)
				Register(new Actions.ModReportHappiness(i));
			if (!skipRequestHome)
				Register(new Actions.ModRequestHome(i));
		}
	}

	public static void Unload()
	{
		All.Clear();
		Initialize();
	}

	public static void Shop(int npcType, string shopName, string customTextKey = null)
	{
		Register(new Actions.OpenShop2(npcType, shopName, customTextKey));
	}
}