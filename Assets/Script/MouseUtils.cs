using UnityEngine;

namespace MouseInput
{   
    public class MouseUtils : MonoBehaviour
    {
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.z * -1f;
            Vector3 vec = GetMouseWorldPositionWithZ(mousePos, Camera.main);
            vec.z = 0f; //Get mouse position in Z = 0f
            return vec;
        }

        //GetMouseWorldPositionWithZ 多型
        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }

        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }

        //計算單位方向向量（起點輸入）
        public static Vector3 GetDirToMouse(Vector3 fromPosition)
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            return(mouseWorldPosition - fromPosition).normalized;
        }
    }
}