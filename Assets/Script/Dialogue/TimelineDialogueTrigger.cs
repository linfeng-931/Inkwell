using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineDialogueTrigger : MonoBehaviour
{
    private PlayableDirector director;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        GameEvent.OnDialogueClosed += ResumeTimeline;
    }

    private void OnDisable()
    {
        GameEvent.OnDialogueClosed -= ResumeTimeline;
    }

    /// <summary>
    /// for timeline signal
    /// </summary>
    /// <param name="dialogueId"></param>
    public void PlayDialogueFromTimeline(string dialogueId)
    {
        // stop timeline
        director.Pause();

        // show dialogue
        DialogueManager.Instance.TryPlayDialogue(dialogueId);
    }

    private void ResumeTimeline()
    {
        if (director != null && director.state == PlayState.Paused)
        {
            director.Play();
        }
    }
}