using Assembly_CSharp.TasInfo.mm.Source;

public class patch_CameraController : CameraController {
    private void OnPreRender() {
        TasInfo.OnPreRender();
    }

    private void OnPostRender() {
        TasInfo.OnPreRender();
    }
}