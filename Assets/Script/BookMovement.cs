
using UnityEngine;

public class BookMovement : MonoBehaviour
{
    [Header("�W�U�B��")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    [Header("���a����")]
    public float detectionRange = 1f;

    [Header("���ʳ]�w")]
    public Vector3 moveOffset = new Vector3(0, 0, 0);
    public float moveSpeed = 3f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private Transform player;

    private bool isNear = false;

    void Start()
    {
        originalPosition = transform.position;

        // �p��ؼЦ�m = �쥻��m + XYZ �첾
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
            Debug.Log("������ Player�I");
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // �ϥ� Global X �y�ЧP�_�Z��
        float distance = Mathf.Abs(
            player.position.x - originalPosition.x
        );

        isNear = distance <= detectionRange;

        if (isNear)
        {
            // ���a�a��G���ʨ찾���᪺��m
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
        }
        else
        {
            // ���a�����G�^����ë�_�W�U�ư�
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
