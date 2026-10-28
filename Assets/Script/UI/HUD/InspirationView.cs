using System.Collections;
using TMPro;
using UnityEngine;

public class InspirationView : BasePanel
{
    public TextMeshProUGUI amountText;
    [SerializeField] private float countSpeed = 10f;
    [SerializeField] private float closeDelay = 2f;

    private float displayValue;
    private int targetValue;
    private Coroutine countCoroutine;
    private float closeTimer;

    public bool IsRunning => countCoroutine != null;

    public void Initialize(int value)
    {
        targetValue = value;
        displayValue = value;
        amountText.text = value.ToString();
    }

    public void UpdateAmountText(int newTargetValue)
    {
        targetValue = newTargetValue;
        closeTimer = closeDelay;

        if(countCoroutine == null)
        {
            countCoroutine = StartCoroutine(CountUpAndCloseRoutine());
        }
    }

    private IEnumerator CountUpAndCloseRoutine()
    {
        while (true)
        {
            if(Mathf.Abs(targetValue - displayValue) > 0.5f)
            {
                // smooth ani
                displayValue = Mathf.Lerp(displayValue, targetValue, Time.deltaTime * countSpeed);
                amountText.text = Mathf.RoundToInt(displayValue).ToString();

                closeTimer = closeDelay;
            }
            else
            {
                if(displayValue != targetValue)
                {
                    displayValue = targetValue;
                    amountText.text = targetValue.ToString();
                }

                closeTimer -= Time.deltaTime;

                if(closeTimer <= 0)
                {
                    break;
                }
            }

            yield return null;
        }
        countCoroutine = null;
        OnExit();
    }
}
