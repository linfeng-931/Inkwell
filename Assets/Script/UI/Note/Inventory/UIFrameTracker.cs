using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFrameTracker : MonoBehaviour
{
    private RectTransform frameRect;
    private Image frameImage;
    public float lerpSpeed = 20f;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        frameRect = GetComponent<RectTransform>();
        frameImage = GetComponent<Image>();
        frameImage.enabled = false;
    }

    private void OnEnable()
    {
        GameEvent.OnUpdateSelectionFrame += MoveToTarget;
    }

    private void OnDisable()
    {
        GameEvent.OnUpdateSelectionFrame -= MoveToTarget;
    }

    private void MoveToTarget(RectTransform targetRect)
    {
        if (targetRect == null) return;
        
        if (!frameImage.enabled) frameImage.enabled = true;

        // change target
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetRect));
    }

    private IEnumerator SmoothMove(RectTransform targetRect)
    {
        Vector3[] corners = new Vector3[4];

        while (targetRect != null)
        {
            // get points in world space
            // corners[0] = left down, corners[1] = left up, corners[2] = right up, corners[3] = right down
            targetRect.GetWorldCorners(corners);

            // get center point
            Vector3 trueCenter = (corners[0] + corners[2]) / 2f;

            // get real size
            float trueWidth = Vector3.Distance(corners[0], corners[3]) / frameRect.lossyScale.x;
            float trueHeight = Vector3.Distance(corners[0], corners[1]) / frameRect.lossyScale.y;
            Vector2 trueTargetSize = new Vector2(trueWidth, trueHeight);

            // check size
            if (Vector3.Distance(frameRect.position, trueCenter) < 0.5f && 
                Vector2.Distance(frameRect.sizeDelta, trueTargetSize) < 0.5f)
            {
                break;
            }

            // move
            frameRect.position = Vector3.Lerp(frameRect.position, trueCenter, Time.unscaledDeltaTime * lerpSpeed);
            frameRect.sizeDelta = Vector2.Lerp(frameRect.sizeDelta, trueTargetSize, Time.unscaledDeltaTime * lerpSpeed);
            
            yield return null; 
        }

        // double check size
        if (targetRect != null)
        {
            targetRect.GetWorldCorners(corners);
            frameRect.position = (corners[0] + corners[2]) / 2f;
            frameRect.sizeDelta = new Vector2(
                Vector3.Distance(corners[0], corners[3]) / frameRect.lossyScale.x,
                Vector3.Distance(corners[0], corners[1]) / frameRect.lossyScale.y
            );
        }
    }
}