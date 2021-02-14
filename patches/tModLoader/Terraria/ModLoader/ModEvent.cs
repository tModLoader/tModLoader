namespace Terraria.ModLoader
{
	public class ModEvent : ModType
	{
		public int Type { get; internal set; }

		public bool Active { get; internal set; }

		protected virtual void StartEvent() { }

		protected virtual void StopEvent() { }

		protected internal virtual void Update() { }

		public void Start() {
			Active = true;
		}

		protected override void Register() {
			ModTypeLookup<ModEvent>.Register(this);
			Type = ModEventLoader.Register(this);
		}
	}
}