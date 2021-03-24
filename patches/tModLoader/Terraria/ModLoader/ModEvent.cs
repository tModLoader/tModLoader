using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public abstract class ModEvent : ModType
	{
		public int Type { get; internal set; }

		public bool Active { get; internal set; }

		public abstract Texture2D Icon { get; }

		protected virtual void StartEvent() { }

		protected virtual void EndEvent() { }

		protected internal virtual void Update() { }

		public virtual TagCompound Save() => null;

		public virtual void Load(TagCompound tag) { }

		public void Start() {
			Active = true;
			Main.invasionDisplays[Type] = new ModInvasion.ModInvasionProgressDisplay(160, 0f);
			StartEvent();
		}

		public void End() {
			EndEvent();
			Active = false;
		}

		protected override void Register() {
			ModTypeLookup<ModEvent>.Register(this);

			if (this is ModInvasion modInvasion)
				ModTypeLookup<ModInvasion>.Register(modInvasion);

			Type = ModEventLoader.Register(this);
		}
	}
}