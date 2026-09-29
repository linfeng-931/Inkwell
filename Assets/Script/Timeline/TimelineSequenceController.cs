using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineSequenceController : MonoBehaviour
{
    public Transform startPosition;
    public bool requireGrounded = true;
    [SerializeField] private bool triggerOnce = true;
    public PlayableDirector director;
    
    private bool hasTriggered = false;

    private void Awake()
    {
        director.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasTriggered && triggerOnce) return;
        hasTriggered = true;

        CutsceneManager.Instance.StartCutscene(this);
    }
}
