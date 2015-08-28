using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class ModSound
	{
		private static int nextSound = 126;
		internal static readonly IDictionary<int, ModSound> sounds = new Dictionary<int, ModSound>();

		internal static int ReserveSoundID()
		{
			int reserveID = nextSound;
			nextSound++;
			return reserveID;
		}

		public static ModSound GetSound(int type)
		{
			if (sounds.ContainsKey(type))
			{
				return sounds[type];
			}
			else
			{
				return null;
			}
		}

		internal static void ResizeArrays(bool unloading = false)
		{
			Array.Resize(ref Main.soundItem, nextSound);
			Array.Resize(ref Main.soundInstanceItem, nextSound);
		}

		internal static void Unload()
		{
			sounds.Clear();
			nextSound = 126;
		}

		public string Name
		{
			get;
			internal set;
		}

		public int Type
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}

		internal string audioFilename;

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}
	}
}
