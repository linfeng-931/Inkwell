using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartPuzzle : MonoBehaviour
{
    public Transform puzzleCameraTarget;
    public Interaction interactionScript;
    public GameObject player;
    public Camera mainCamera;
    public Camera puzzleCamera;
    public GameObject playerUI;
    public GameObject InteractionUI;
    public CameraController cameraController;

    private Vector3 savedCamPos;
    private Quaternion savedCamRot;

    void Start()
    {
        puzzleCamera.enabled = false;
        mainCamera.enabled = true;
    }

    void Update()
    {
        if (interactionScript != null && interactionScript.GetCanInteract() && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TransitionToPuzzle());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(LeavePuzzle());
        }
    }

    IEnumerator TransitionToPuzzle()
    {
        savedCamPos = mainCamera.transform.position;
        savedCamRot = mainCamera.transform.rotation;

        SnappableItem.isPuzzleActive = true;
        TieRodController.isPuzzleActive = true;
        PlayerController.isPuzzleActive = true;
        CameraController.isPuzzleActive = true; 

        InteractionUI.SetActive(false);
        playerUI.SetActive(false);

        yield return StartCoroutine(CameraTransition(mainCamera, puzzleCamera, puzzleCameraTarget));

        player.SetActive(false);
    }

    IEnumerator LeavePuzzle()
    {
        
        var camController = mainCamera.GetComponent<CameraController>();

        yield return StartCoroutine(CameraTransitionBack(puzzleCamera, mainCamera));
        cameraController.ResetCameraPosition(savedCamPos, savedCamRot);

        SnappableItem.isPuzzleActive = false;
        TieRodController.isPuzzleActive = false;
        PlayerController.isPuzzleActive = false;
        CameraController.isPuzzleActive = false;
        camController.FreezeFollow(0.3f);

        InteractionUI.SetActive(true);
        playerUI.SetActive(true);

        player.SetActive(true);

    }

    IEnumerator CameraTransition(Camera currentCam, Camera nextCam, Transform target)
    {
        float time = 0;
        Vector3 startPos = currentCam.transform.position;
        Quaternion startRot = currentCam.transform.rotation;
        float duration = 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            currentCam.transform.position = Vector3.Lerp(startPos, target.position, t);
            currentCam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        currentCam.enabled = false;
        nextCam.transform.position = target.position;
        nextCam.enabled = true;
    }

    IEnumerator CameraTransitionBack(Camera currentCam, Camera nextCam)
    {
        float time = 0;
        float duration = 0.8f;

        Vector3 startPos = currentCam.transform.position;
        Quaternion startRot = currentCam.transform.rotation;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            currentCam.transform.position =
                Vector3.Lerp(startPos, savedCamPos, t);

            currentCam.transform.rotation =
                Quaternion.Slerp(startRot, savedCamRot, t);

            yield return null;
        }

        currentCam.enabled = false;
        nextCam.enabled = true;
    }
}