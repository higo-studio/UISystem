using System;
using UnityEngine;
using Higo.UI;
using Unity.Collections.LowLevel.Unsafe;

public static class UISystemExtension
{
    public static Uuid Show<TEnum>(this UISystem @this, TEnum layer, string name, bool isExclusive = true)
        where TEnum : unmanaged, Enum
        => @this.Show(UnsafeUtility.As<TEnum, int>(ref layer), name, isExclusive);
    public static void Hide<TEnum>(this UISystem @this, TEnum layer, string name = null)
        where TEnum : unmanaged, Enum
        => @this.Hide(UnsafeUtility.As<TEnum, int>(ref layer), name);

    public static bool Contains<TEnum>(this UISystem @this, TEnum layer, string name)
        where TEnum : unmanaged, Enum
    {
        return @this.TryGetTop(UnsafeUtility.As<TEnum, int>(ref layer), name, out _);
    }

    public static Uuid CheckShow<TEnum>(this UISystem @this, TEnum layer, string name, bool isExclusive = true)
        where TEnum : unmanaged, Enum
    {
        return @this.TryGetTop(UnsafeUtility.As<TEnum, int>(ref layer), name, out var uuid)
            ? uuid
            : Show(@this, layer, name, isExclusive);
    }
}
