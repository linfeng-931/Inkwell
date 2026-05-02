using UnityEngine;
using System.Collections.Generic;

public class GearActive : MonoBehaviour
{
    public AudioSource gearRotMusic;
    public AudioSource completeMusic;
    public float seekTime = 9.0f;

    public int totalRequiredSockets = 7;
    private HashSet<GhostSocket> satisfiedSockets = new HashSet<GhostSocket>();
    public bool completePuzzle = false;

    // Update is called once per frame
    public void PlaySound()
    {
        if (gearRotMusic!=null)
        {
            gearRotMusic.time = seekTime;
            gearRotMusic.Play();
        }
    }

    public void PlayCompleteMusic()
    {
        if (completeMusic != null)
        {
            completeMusic.Play();
        }
    }

    public void UpdateSocketStatus(GhostSocket socket, bool isCorrect)
    {
        if (isCorrect)
            satisfiedSockets.Add(socket);
        else
            satisfiedSockets.Remove(socket);

        Debug.Log($"目前正確數量: {satisfiedSockets.Count} / {totalRequiredSockets}");

        if (satisfiedSockets.Count >= totalRequiredSockets)
        {
            Debug.Log("解謎完成！");
            completePuzzle = true;
        }
    }

    public bool IsComplete()
    {
        return completePuzzle;
    }
}
