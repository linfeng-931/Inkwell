using UnityEngine;
using System.Collections.Generic;

public class SnappableItem : MonoBehaviour
{
    public string itemId;
    public bool needRot = false;

    private Vector3 offset;
    private Camera cam;
    public float snapThreshold = 1.0f; // 吸附距離
    public float showGhostThreshold = 1.0f; // 鬼影距離

    private GhostSocket[] allGhostSockets;
    private GhostSocket currentlyActiveGhostSocket = null;

    public AudioClip snapSound;

    private Transform lastValidParent; // 前一位置
    public GearActive gearActiveRef = null;

    public float puzzleZDepth = 0.0f;

    public static bool isPuzzleActive = false;

    public Material mouseOnMat;
    public Material originMat;


    void Start() { 
        cam = Camera.main;
        allGhostSockets = FindObjectsOfType<GhostSocket>();
        lastValidParent = transform.parent;
    }

    bool IsSystemLocked()
    {
        foreach (var rot in FindObjectsOfType<AutoRotate>())
        {
            if (rot.isRotating) return true;
        }
        return false;
    }

    private void OnMouseExit()
    {
        GetComponent<Renderer>().material = originMat;
    }

    private void OnMouseOver()
    {
        if (IsSystemLocked() || gearActiveRef.IsComplete() || !isPuzzleActive) return;
        GetComponent<Renderer>().material = mouseOnMat;
    }

    // 當滑鼠按下
    void OnMouseDown()
    {
        
        if (IsSystemLocked() || gearActiveRef.IsComplete() || !isPuzzleActive) return;
        GetComponent<Renderer>().material = mouseOnMat;

        if (transform.parent != null && transform.parent.TryGetComponent<GhostSocket>(out var oldSocket))
        {
            oldSocket.isOccupied = false;
            oldSocket.NotifyStatusChanged("");
            oldSocket.HideGhost();
            transform.SetParent(null);
        }

        Vector3 mousePos = GetMouseWorldPos();
        offset = gameObject.transform.position - mousePos;
    }

    // 當滑鼠拖曳
    void OnMouseDrag()
    {
        if (IsSystemLocked() || gearActiveRef.IsComplete() || !isPuzzleActive) return;
        GetComponent<Renderer>().material = mouseOnMat;

        Vector3 newPos = GetMouseWorldPos() + offset;
        newPos.z -= 0.1f;
        transform.position = newPos;

        UpdateGhostVisibility();
    }

    // 當滑鼠放開
    void OnMouseUp()
    {
        GetComponent<Renderer>().material = originMat;
        if (IsSystemLocked() || gearActiveRef.IsComplete()||!isPuzzleActive) return;

        if (currentlyActiveGhostSocket != null)
        {
            currentlyActiveGhostSocket.HideGhost();
            currentlyActiveGhostSocket = null;
        }

        // 尋找場景中最近的 Socket
        GameObject[] sockets = GameObject.FindGameObjectsWithTag("Socket");
        float minDistance = float.MaxValue;
        GameObject nearestSocket = null;

        foreach (GameObject s in sockets)
        {
            var gs = s.GetComponent<GhostSocket>();

            if (gs != null && !gs.isOccupied && gs.expectedItemIds.Contains(this.itemId))
            {
                Vector2 itemPos2D = new Vector2(transform.position.x, transform.position.y);
                Vector2 socketPos2D = new Vector2(s.transform.position.x, s.transform.position.y);
                float dist = Vector2.Distance(itemPos2D, socketPos2D);

                // ------------------------------------

                if (dist < snapThreshold && dist < minDistance)
                {
                    minDistance = dist;
                    nearestSocket = s;
                }
            }
        }

        if (nearestSocket != null)
        {
            SnapTo(nearestSocket.transform);
            if(needRot)
                transform.Rotate(90, 0, 0);
            lastValidParent = nearestSocket.transform;
        }
        else
        {
            transform.SetParent(lastValidParent);
            transform.localPosition = Vector3.zero; // 對齊父物件中心

            // 如果這是一個 socket，記得重新設定它為佔用
            var socketComp = lastValidParent.GetComponent<GhostSocket>();
            if (socketComp != null) socketComp.isOccupied = true;
        }
    }

    void UpdateGhostVisibility()
    {
        GhostSocket nearest = null;
        float minDistance = float.MaxValue;

        foreach (GhostSocket gs in allGhostSockets)
        {
            if (gs.isOccupied || !gs.expectedItemIds.Contains(this.itemId)) continue;

            float dist = Vector3.Distance(transform.position, gs.transform.position);

            if (dist < showGhostThreshold && dist < minDistance)
            {
                minDistance = dist;
                nearest = gs;
            }
        }

        // 顯示邏輯
        if (nearest != null)
        {
            if (currentlyActiveGhostSocket != nearest)
            {
                if (currentlyActiveGhostSocket != null) currentlyActiveGhostSocket.HideGhost();
                nearest.ShowGhost(this.itemId);
                currentlyActiveGhostSocket = nearest;
            }
        }
        else
        {
            if (currentlyActiveGhostSocket != null)
            {
                currentlyActiveGhostSocket.HideGhost();
                currentlyActiveGhostSocket = null;
            }
        }
    }


    void SnapTo(Transform socket)
    {
        transform.position = socket.position; // 對齊位置
        transform.SetParent(socket);          // 設定為子物件，從此跟隨旋轉！
        transform.rotation = socket.rotation; // 對齊角度
        transform.localPosition = new Vector3(0, 0, 0);

        if (snapSound != null)
        {
            AudioSource.PlayClipAtPoint(snapSound, transform.position);
        }

        var ghostComp = socket.GetComponent<GhostSocket>();
        if (ghostComp != null)
        {
            ghostComp.isOccupied = true;
            ghostComp.NotifyStatusChanged(this.itemId);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, puzzleZDepth));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return transform.position;
    }
}