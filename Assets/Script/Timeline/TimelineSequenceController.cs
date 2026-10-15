using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineSequenceController : MonoBehaviour, INotificationReceiver
{
    public Transform startPosition;
    public bool requireGrounded = true;
    [SerializeField] private bool triggerOnce = true;
    public PlayableDirector director;
    public UnityEvent onTimelineFinished;

    private bool hasTriggered = false;

    private void Awake()
    {
        director.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            CutsceneManager.Instance.StartCutscene(this);
        }
    }

    public void TriggerTimeline()
    {
        if(hasTriggered && triggerOnce) return;
        hasTriggered = true;
        CutsceneManager.Instance.StartCutscene(this);
    }

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is DialogueMarker dialogueMarker)
        {
            CutsceneManager.Instance.PauseForDialogue(dialogueMarker.dialogueId);
        }
    }

    public void FinishSequence()
    {
        onTimelineFinished?.Invoke();
    }
}
