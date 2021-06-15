// ReSharper disable All

using System.Collections;
using System.Collections.Generic;
using MonoMod;
using UnityEngine.SceneManagement;

#pragma warning disable CS0649, CS0414

public class patch_GameManager : GameManager {
    private static readonly long TasInfoMark = 1234567890123456789;
    public static string TasInfo;

#if V1028
    [MonoModIgnore]
    private extern void orig_ManualLevelStart();
    private void ManualLevelStart() {
        orig_ManualLevelStart();
        Assembly_CSharp.TasInfo.mm.Source.TasInfo.AfterManualLevelStart();
    }
#endif
}