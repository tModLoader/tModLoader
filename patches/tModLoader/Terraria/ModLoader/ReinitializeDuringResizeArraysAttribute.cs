using System;

namespace Terraria.ModLoader;

/// <summary>
/// Classes annotated with this attribute will have their static constructor called again during the ResizeArrays stage of mod loading.
/// <para/> This will happen before <see cref="ModSystem.ResizeArrays"/> is called for any mod.
/// <para/> This is intended for classes containing ID sets created through <see cref="Terraria.ID.SetFactory"/>, similar to the design of vanilla classes such as <see cref="ID.ItemID.Sets"/>. This attribute removes the need to manually initialize these ID sets in <see cref="ModSystem.ResizeArrays"/> and helps avoid mod ordering issues that would complicate the implementation logic.
/// <para/> This will not work on generic classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ReinitializeDuringResizeArraysAttribute : Attribute
{
}
