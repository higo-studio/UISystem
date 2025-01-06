using System;
using UnityEngine;
using Higo.UI;
using Unity.Collections.LowLevel.Unsafe;

public static class UISystemExtension
{
    public static Uuid OpenUI<TEnum>(this UISystem @this, TEnum layer, string name, bool isExclusive = true)
        where TEnum : Enum, IConvertible
        => @this.OpenUI(UnsafeUtility.As<TEnum, int>(ref layer), name, isExclusive);
    public static void CloseUI<TEnum>(this UISystem @this, TEnum layer, string name = null)
        where TEnum : Enum
        => @this.CloseUI(UnsafeUtility.As<TEnum, int>(ref layer), name);
}
