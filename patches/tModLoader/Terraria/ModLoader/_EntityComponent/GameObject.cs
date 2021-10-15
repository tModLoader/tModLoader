using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public sealed class GameObject
	{
		private static readonly List<GameObject> gameObjects = new();

		private readonly List<Component> components = new();

		public readonly IReadOnlyList<Component> Components;

		internal GameObject() {
			Components = (components = new()).AsReadOnly();
		}

		public void Destroy() {
			foreach (var component in components) {
				component.Dispose();
			}

			components.Clear();
		}

		public T AddComponent<T>() where T : Component {
			return AddComponentFromBase(ModContent.GetInstance<T>());
		}

		public bool HasComponent<T>() where T : Component {
			for (int i = 0; i < components.Count; i++) {
				if (components[i] is T) {
					return true;
				}
			}

			return false;
		}

		public T GetComponent<T>() where T : Component {
			for (int i = 0; i < components.Count; i++) {
				if (components[i] is T tComponent) {
					return tComponent;
				}
			}

			throw new KeyNotFoundException($"Component of type '{typeof(T).Name}' was not present.");
		}

		public bool TryGetComponent<T>(out T result) where T : Component {
			for (int i = 0; i < components.Count; i++) {
				if (components[i] is T tComponent) {
					result = tComponent;

					return true;
				}
			}

			result = default;

			return false;
		}

		internal T AddComponentFromBase<T>(T componentBase) where T : Component {
			var newComponent = (T)componentBase.Clone(this);

			return AddComponentInternal(newComponent);
		}

		internal T AddComponentInternal<T>(T component) where T : Component {
			components.Add(component);

			component.GameObject = this;

			component.Initialize();

			return component;
		}

		public static GameObject Instantiate() {
			var gameObject = new GameObject();

			gameObjects.Add(gameObject);

			ComponentLoader.AddGlobalComponents(gameObject);

			return gameObject;
		}
	}
}
