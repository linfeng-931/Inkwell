
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    [Header("上下浮動")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    [Header("玩家偵測")]
    public float detectionRange = 1f;

    [Header("移動設定")]
    public Vector3 moveOffset = new Vector3(0, 0, 0);
    public float moveSpeed = 3f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private Transform player;

    private bool isNear = false;

    void Start()
    {
        originalPosition = transform.position;

        // 計算目標位置 = 原本位置 + XYZ 位移
        targetPosition = originalPosition + moveOffset;

        FindPlayer();
    }

    // 持續尋找 Player
    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log("成功找到 Player！");
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // 使用 Global X 座標判斷距離
        float distance = Mathf.Abs(
            player.position.x - originalPosition.x
        );

        isNear = distance <= detectionRange;

        if (isNear)
        {
            // 玩家靠近：移動到偏移後的位置
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
        }
        else
        {
            // 玩家遠離：回到原位並恢復上下飄動
            Vector3 floatPosition = originalPosition;

            floatPosition.y += Mathf.Sin(
                Time.time * floatSpeed
            ) * floatHeight;

            transform.position = Vector3.Lerp(
                transform.position,
                floatPosition,
                Time.deltaTime * moveSpeed
            );
        }
    }
}
