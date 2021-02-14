namespace Terraria.ModLoader
{
	public abstract class ModEvent : ModType
	{
		public int Type { get; internal set; }

		public bool Active { get; internal set; }

		protected virtual void StartEvent() { }

		protected virtual void EndEvent() { }

		protected internal virtual void Update() { }

		public void Start() {
			Active = true;
			StartEvent();
		}

		public void End() {
			EndEvent();
			Active = false;
		}

		protected override void Register() {
			ModTypeLookup<ModEvent>.Register(this);
			Type = ModEventLoader.Register(this);
		}
	}
}