using System;

namespace Terraria.ModLoader
{
	public abstract class Component : ModType, IDisposable
	{
		public GameObject GameObject { get; internal set; }

		protected internal Component() { }

		protected internal virtual void Initialize() { }

		protected internal virtual void Dispose() { }

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
