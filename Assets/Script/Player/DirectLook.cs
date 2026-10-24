using UnityEngine;
using Live2D.Cubism.Core;

public class DirectLook : MonoBehaviour
{
    public CubismParameter angleX;
    public CubismParameter angleY;
    public float factor = 30f;

    void LateUpdate()
    {
        if (angleX == null || angleY == null) return;

        // between 1, -1
        float mouseX = (Input.mousePosition.x / Screen.width) * 2f - 1f;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2f - 1f;

        mouseX = Mathf.Clamp(mouseX, -1f, 1f);
        mouseY = Mathf.Clamp(mouseY, -1f, 1f);

        angleX.Value = mouseX * factor;
        angleY.Value = mouseY * factor;
    }
}