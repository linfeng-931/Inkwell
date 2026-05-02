using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour
{
    public float targetXAngle = 180;
    [HideInInspector] public bool isRotating = false;
    public float duration = 2;
    public GearActive gearActiveRef = null;
    public GameObject tierController;
    TieRodController tierControllerScript;

    private void Start()
    {
        tierControllerScript = GetComponent<TieRodController>();
    }


    public void StartRotation()
    {
        if (isRotating) return;
        StartCoroutine(RotateAndReverse(targetXAngle, duration));
    }

    public IEnumerator RotateAndReverse(float targetAngle, float duration)
    {
        try
        {
            isRotating = true;
            
            yield return StartCoroutine(UpdateRotation(targetAngle, duration));

            if (gearActiveRef.IsComplete())
            {
                gearActiveRef.PlayCompleteMusic();
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
                yield return StartCoroutine(UpdateRotation(-targetAngle, duration));
            }
        }
        finally
        {
            isRotating = false;
        }
    }

    private IEnumerator UpdateRotation(float angle, float time)
    {
        if (gearActiveRef != null) gearActiveRef.PlaySound();
        float elapsed = 0;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float curvedT = Mathf.SmoothStep(0, 1, t);

            transform.rotation = Quaternion.Slerp(startRot, endRot, curvedT);
            yield return null;
        }
        transform.rotation = endRot;
    }

    void OnDisable()
    {
        isRotating = false;
    }

}