using System;
using System.Collections;
using System.Collections.Generic;

namespace Terraria.ModLoader.Shops
{
	public class CacheList : IEnumerable<Item>
	{
		private Item[] data;
		public int Count { get; private set; }
		public int Capacity { get; private set; }

		public readonly int InitialSize;
		public readonly int GrowBy;
		
		public CacheList(int size = 40, int grow = 10) {
			InitialSize = size;
			GrowBy = grow;
			
			Capacity = size;

			data = new Item[size];
			for (int i = 0; i < size; i++)
			{
				data[i] = new Item();
			}
		}

		public void Clear() {
			data = new Item[InitialSize];
			Count = 0;
			Capacity = InitialSize;

			for (int i = 0; i < InitialSize; i++)
			{
				data[i] = new Item();
			}
		}

		public int Add(Item item) {
			Count++;

			int nextEmpty = -1;
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i].IsAir)
				{
					nextEmpty = i;
					break;
				}
			}

			if (nextEmpty == -1)
			{
				Capacity += GrowBy;
				nextEmpty = data.Length;

				Array.Resize(ref data, data.Length + GrowBy);

				for (int i = data.Length - GrowBy; i < data.Length; i++)
					data[i] = new Item();
			}

			data[nextEmpty] = item;
			return nextEmpty;
		}

		public void Remove(int index) {
			data[index] = new Item();
			Count--;
		}
		
		public void RemoveEmptyRows() {
			for (int i = data.Length / GrowBy - 1; i >= 0; i--)
			{
				for (int j = i * GrowBy; j < i * GrowBy + GrowBy; j++)
				{
					if (!data[j].IsAir) return;
				}
				
				Array.Resize(ref data, data.Length - GrowBy);
				Capacity -= GrowBy;
			}
		}

		public IEnumerator<Item> GetEnumerator() => (IEnumerator<Item>)data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public Item this[int index] {
			get => data[index];
			set => data[index] = value;
		}

		public void AddRange(IEnumerable<Item> items) {
			foreach (Item item in items)
			{
				Add(item);
			}
		}
	}
}