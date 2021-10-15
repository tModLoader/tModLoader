using System;

namespace Terraria.ModLoader
{
	public abstract class Component : ModType, IDisposable
	{
		public GameObject GameObject { get; internal set; }
		public int Type { get; private set; }

		protected internal Component() { }

		/// <summary> Create a copy of this component. Called when a game object is cloned. </summary>
		/// <param name="gameObject"> The new game object. </param>
		public virtual Component Clone(GameObject gameObject) {
			Component clone = (Component)MemberwiseClone();

			clone.GameObject = gameObject;

			return clone;
		}

		protected internal virtual void Initialize() { }

		protected internal virtual void Dispose() { }

		protected sealed override void Register() {
			Type = ComponentLoader.RegisterComponent(this);

			ModTypeLookup<Component>.Register(this);
		}

		public T AddComponent<T>() where T : Component
			=> GameObject.AddComponent<T>();

		public bool HasComponent<T>() where T : Component
			=> GameObject.HasComponent<T>();

		public T GetComponent<T>() where T : Component
			=> GameObject.GetComponent<T>();

		public bool TryGetComponent<T>(out T result) where T : Component
			=> GameObject.TryGetComponent<T>(out result);

		void IDisposable.Dispose() {
			Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
