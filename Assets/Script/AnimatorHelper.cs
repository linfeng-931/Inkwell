using UnityEngine;

public class AnimatorHelper : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string aniName;
    [SerializeField] private string playedFlagID;
    [SerializeField] private bool autoSave = true;

    private bool isPlayingNow = false;

    public void HandleAnimation()
    {
        if (isPlayingNow) return;

        var flags = SaveManager.Instance.CurrentSave.flags;
        bool hasPlayedBefore = flags.ContainsKey(playedFlagID) && flags[playedFlagID];

        if (hasPlayedBefore)
        {
            animator.Play(aniName, 0, 1f);
        }
        else
        {
            isPlayingNow = true;
            animator.Play(aniName, 0, 0f);

            flags[playedFlagID] = true;
            if (autoSave)
            {
                SaveManager.Instance.SaveGame();
            }
        }
    }

}
