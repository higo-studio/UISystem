using System;
using UnityEngine;
using Higo.UI;
using Unity.Collections.LowLevel.Unsafe;

public static class UISystemExtension
{
    public static UIUUID OpenUI<TEnum>(this UISystem @this, TEnum layer, string name, bool isExclusive = true)
        where TEnum : unmanaged, Enum
        => @this.OpenUI(UnsafeUtility.As<TEnum, int>(ref layer), name, isExclusive);
    public static void CloseUI<TEnum>(this UISystem @this, TEnum layer, string name = null)
        where TEnum : unmanaged, Enum
        => @this.CloseUI(UnsafeUtility.As<TEnum, int>(ref layer), name);
}
