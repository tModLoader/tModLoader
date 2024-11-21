using System;

namespace Terraria.ModLoader;

/// <summary>
/// <see cref="ILoadable"/> classes annotated with this attribute will have their static constructor called again during the ResizeArrays stage of mod loading. This is intended for classes containing data sets created through <see cref="Terraria.ID.SetFactory"/>, similar to the design of vanilla classes such as <see cref="ID.ItemID.Sets"/>. This attribute removes the need to manually initialize these data sets in <see cref="ModSystem.ResizeArrays"/> and helps avoid mod ordering issues that would complicate the implementation logic.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ReinitializeDuringResizeArraysAttribute : Attribute
{
}
