using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public sealed class GameObject
	{
		private static readonly List<GameObject> gameObjects = new();

		private readonly List<Component> components = new();

		public void Destroy() {
			foreach (var component in components) {
				component.Dispose();
			}

			components.Clear();
		}

		public T AddComponent<T>() where T : Component {
			return AddComponent(Activator.CreateInstance<T>());
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

		internal T AddComponent<T>(T component) where T : Component {
			components.Add(component);

			component.GameObject = this;

			component.Initialize();

			return component;
		}

		public static GameObject Instantiate() {
			var gameObject = new GameObject();

			gameObjects.Add(gameObject);

			return gameObject;
		}
	}
}
