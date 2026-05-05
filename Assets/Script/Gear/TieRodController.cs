using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TieRodController : MonoBehaviour
{
    public Animator tieRodAnimator;
    public string animationName = "Cube_036|tie rod";
    private bool isPulling = false;
    public List<AutoRotate> gearScripts;
    public GearActive gearActiveRef = null;
    bool puzzleComplete = false;
    public static bool isPuzzleActive = false;
    public AudioSource tieRodUp;
    public AudioSource tieRodDown;

    void OnMouseDown()
    {
        if (!isPuzzleActive) return;
        PullLever();
    }

    public void PullLever()
    {
        if (isPulling || puzzleComplete) return;
        StartCoroutine(PullSequence());
    }

    private IEnumerator PullSequence()
    {
        isPulling = true;

        // 正播
        tieRodAnimator.SetFloat("PlaySpeed", 1f);
        tieRodAnimator.Play(animationName, 0, 0f);
        tieRodUp.pitch = 2f;
        tieRodUp.Play();

        yield return new WaitForSeconds(1.208f);
        // 啟動齒輪
        if (gearScripts != null)
        {
            foreach (var gear in gearScripts)
            {
                if (gear != null) gear.StartRotation();
            }
        }

        // 倒播
        tieRodDown.pitch = 2f;
        tieRodDown.Play();
        tieRodAnimator.Play(animationName, 0, 1f);
        tieRodAnimator.SetFloat("PlaySpeed", -1f);
        

        yield return new WaitForSeconds(1.208f);

        // 重置
        yield return new WaitUntil(() => AreAllGearsFinished());
        if (gearActiveRef.IsComplete())
            puzzleComplete = true;
        tieRodAnimator.SetFloat("PlaySpeed", 0f);
        isPulling = false;
        print("可再次拉動");
    }

    private bool AreAllGearsFinished()
    {
        foreach (var gear in gearScripts)
        {
            if (gear != null && gear.isRotating)
                return false;
        }
        return true;
    }

    public bool getIsPulling()
    {
        return isPulling;
    }

    public bool getIsComplete()
    {
        return puzzleComplete;
    }
}