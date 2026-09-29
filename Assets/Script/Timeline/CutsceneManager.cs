using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    //[SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Animator playerAnimator;

    [Header("Track Name In Timeline")]
    [SerializeField] private string playerAniTrackName = "@Player_Ani";
    [SerializeField] private string dialogueSignalTrackName = "@Dialogue_Signal";

    private PlayableDirector currentDirector;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// get request from TimelineSequenceController
    /// </summary>
    /// <param name="sequence"></param>
    public void StartCutscene(TimelineSequenceController sequence)
    {
        if (currentDirector != null) return; // is playing

        currentDirector = sequence.director;
        StartCoroutine(ExecuteCutsceneStepsRoutine(sequence));
    }

    private IEnumerator ExecuteCutsceneStepsRoutine(TimelineSequenceController sequence)
    {
        GameEvent.OnCutsceneStateChanged.Invoke(true);
        playerController.EnterCutscene(sequence.startPosition, sequence.requireGrounded);

        yield return new WaitUntil(() => playerController.isCutsceneReady);

        BindTimelineTracks(currentDirector);
        currentDirector.stopped += OnDirectorStopped;
        currentDirector.Play();
    }

    private void BindTimelineTracks(PlayableDirector director)
    {
        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
        foreach(TrackAsset track in timelineAsset.GetOutputTracks())
        {
            // auto bind cinemachine
            if(track is CinemachineTrack && cinemachineBrain != null)
            {
                director.SetGenericBinding(track, cinemachineBrain);
            }
            // auto bind player ani track
            else if (track is AnimationTrack && track.name == playerAniTrackName)
            {
                director.SetGenericBinding(track, playerAnimator);
            }
        }
        director.RebuildGraph();
    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        director.stopped -= OnDirectorStopped;
        currentDirector = null;

        GameEvent.OnCutsceneStateChanged.Invoke(false);
        playerController.TransitionToState<IdleState>();
    }
}
