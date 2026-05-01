using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour
{
    public float targetXAngle = 180;
    [HideInInspector] public bool isRotating = false;
    public float duration = 2;
    public GearActive gearActiveRef = null;


    void Update()
    {
        if (isRotating) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            print(transform.parent.name);
            if (transform.parent != null && transform.parent.name == "GraySocket")
                return;

            StartCoroutine(RotateAndReverse(targetXAngle, duration));
        }
    }

    public IEnumerator RotateAndReverse(float targetAngle, float duration)
    {
        try
        {
            isRotating = true;
            
            // 正向
            yield return StartCoroutine(UpdateRotation(targetAngle, duration));

            yield return new WaitForSeconds(1f);

            // 反向
            yield return StartCoroutine(UpdateRotation(-targetAngle, duration));
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