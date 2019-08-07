using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.Social;
using Terraria.UI;





namespace ExampleMod
{
    public class ExampleEasyRecompile
	{

        Keys recompileHotkey = Keys.P;

        bool doRecompileMods = false;
        bool doEnterGame = true;

        public void load() {
			Main.DoUpdateEvent += OnDoUpdate;	
		}
		
		public void unload() {
			Main.DoUpdateEvent -= OnDoUpdate;	
		}
		

        public void OnDoUpdate(object sender, EventArgs e)
        {

            //should we host a game automatically
            if (this.doEnterGame)
            {
                if (Main.menuMode == 0)
                {
                    Main.SetSelectedMenu(1);



                    return;
                }

                if (Main.menuMode == 12)
                {
                    Main.SetSelectedMenu(2);



                    return;
                }

                if (Main.menuMode == 888)
                {
                    if (Main.MenuUI.CurrentState.GetType() == typeof(UICharacterSelect))
                    {
                        Main.SelectPlayer(Main.PlayerList[0]);
                    }
                    if (Main.MenuUI.CurrentState.GetType() == typeof(UIWorldSelect))
                    {
                        if (Main.WorldList.Count > 0)
                        {
                            Main.WorldList[0].SetAsActive();
                            if (Main.menuMultiplayer && SocialAPI.Network != null)
                            {
                                Main.menuMode = 889;
                            }
                            else if (Main.menuMultiplayer)
                            {
                                Main.menuMode = 30;
                            }
                            else
                            {
                                Main.menuMode = 10;
                            }
                        }
                    }
                }
                if (Main.menuMode == 889)
                {
                    Main.SetSelectedMenu(4);
                }
                if (Main.menuMode == 30)
                {
                    Main.SetSelectedMenu(2);
                    this.doEnterGame = false;

                }



            }


            //check if recompile hotkey is pressed
            var pressedKeys = Main.keyState.GetPressedKeys();
            foreach (var pressed in pressedKeys)
            {
                if (pressed == recompileHotkey)
                {
                    this.doRecompileMods = true;
                }
            }

            //should we exit game and recompile mods?
            if (this.doRecompileMods)
            {
                if (Main.menuMode == 14)
                {
                    Main.menuMode = 10;
                    WorldGen.SaveAndQuit(null);
                }

                if (Main.menuMode == 0)
                {
                    Main.DoRecompileMods();
                    Main.SetSelectedMenu(4);
                    this.doEnterGame = true;
                    this.doRecompileMods = false;
                    Thread.Sleep(1000);
                }
            }
        }

    }
}