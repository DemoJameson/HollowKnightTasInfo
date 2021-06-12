using Assembly_CSharp.TasInfo.mm.Source;

// ReSharper disable All
public class patch_CameraController : CameraController {
    private void OnPreRender() {
        TasInfo.OnPreRender();
    }

    private void OnPostRender() {
        TasInfo.OnPostRender();
    }
}