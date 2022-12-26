using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace ReLogic.Utilities;

partial class SlotVector<T>
{
	public bool TryGetValue(SlotId id, [NotNullWhen(true)] out T? result) {
		uint index = id.Index;

		if (index >= _array.Length) {
			result = default;
			return false;
		}
		
		ref readonly var arrayEntry = ref _array[index];

		if (!arrayEntry.Id.IsActive || id != arrayEntry.Id) {
			result = default;
			return false;
		}

		result = arrayEntry.Value;

		return true;
	}

	public bool TryGetValue(int index, [NotNullWhen(true)] out T? result) {
		if (index < 0 || index >= _array.Length) {
			result = default;
			return false;
		}

		ref readonly var arrayEntry = ref _array[index];
		
		if (!arrayEntry.Id.IsActive) {
			result = default;
			return false;
		}

		result = arrayEntry.Value;

		return true;
	}
}
