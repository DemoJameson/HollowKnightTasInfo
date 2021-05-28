using Assembly_CSharp.TasInfo.mm.Source;
using MonoMod;
using UnityEngine;

// ReSharper disable All

public class patch_PlayMakerUnity2DProxy : PlayMakerUnity2DProxy {
    [MonoModReplace]
    public new void Start() {
        if (PlayMakerUnity2d.isAvailable()) {
            TasInfo.OnColliderCreate(gameObject);
            RefreshImplementation();
        } else {
            Debug.LogError(
                "PlayMakerUnity2DProxy requires the 'PlayMaker Unity 2D' Prefab in the Scene.\nUse the menu 'PlayMaker/Addons/Unity 2D/Components/Add PlayMakerUnity2D to Scene' to correct the situation",
                this);
            enabled = false;
        }
    }
}