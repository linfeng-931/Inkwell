using UnityEngine;

[ExecuteInEditMode]
public class GlobalHatchSender : MonoBehaviour {
    public Texture2D hatch0;
    public Texture2D hatch1;
    public float tiling = 8f;

    void Update() {
        if (hatch0 != null) Shader.SetGlobalTexture("_GlobalHatch0", hatch0);
        if (hatch1 != null) Shader.SetGlobalTexture("_GlobalHatch1", hatch1);
        Shader.SetGlobalFloat("_GlobalHatchTile", tiling);
    }
}