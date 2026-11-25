
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    public float detectionRange = 1f;

    public Vector3 moveOffset = new Vector3(0, 0, 0);
    public float moveSpeed = 3f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private Transform player;

    private bool isNear = false;

    void Start()
    {
        originalPosition = transform.position;

        targetPosition = originalPosition + moveOffset;

        FindPlayer();
    }

    // ����M�� Player
    void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance = Mathf.Abs(
            player.position.x - originalPosition.x
        );

        isNear = distance <= detectionRange;

        if (isNear)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
        }
        else
        {
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
