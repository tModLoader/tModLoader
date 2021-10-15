using Terraria.ModLoader;

namespace Terraria
{
	partial class Entity : Component
	{
		private bool _active;

		public bool Active {
			get => _active;
			set {
				if (value == _active) {
					return;
				}

				if (value) {
					GameObject = GameObject.Instantiate();

					GameObject.AddComponentInternal(this);
				}
				else {
					GameObject.Destroy();

					GameObject = null;
				}

				_active = value;
			}
		}
	}
}
