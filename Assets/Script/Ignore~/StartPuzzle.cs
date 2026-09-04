//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class StartPuzzle : MonoBehaviour
//{
//    public Transform puzzleCameraTarget;
//    public Interaction interactionScript;
//    public GameObject player;
//    public Camera mainCamera;
//    public Camera puzzleCamera;
//    public GameObject playerUI;
//    public GameObject InteractionUI;
//    public CameraController cameraController;
//    public TieRodController tieRodController;
//    public PlayerController playerController;
//    public Rigidbody playerRig;

//    private Vector3 savedCamPos;
//    private Quaternion savedCamRot;

//    void Start()
//    {
//        puzzleCamera.enabled = false;
//        mainCamera.enabled = true;
//    }

//    void Update()
//    {
//        if (interactionScript != null && interactionScript.GetCanInteract() && Input.GetKeyDown(KeyCode.E))
//        {
//            StartCoroutine(TransitionToPuzzle());
//        }

//        if (Input.GetKeyDown(KeyCode.Escape) && !tieRodController.getIsPulling())
//        {
//            StartCoroutine(LeavePuzzle());
//        }

//        if (tieRodController.getIsComplete())
//        {
//            StartCoroutine(LeavePuzzle());
//            interactionScript.SetPuzzleComplete();
//            InteractionUI.SetActive(false);
//        }
//    }

    IEnumerator TransitionToPuzzle()
    {
        //playerController.isInteract = true;
        playerRig.linearVelocity = new Vector3(0, playerRig.linearVelocity.y, 0);
        player.SetActive(false);
        savedCamPos = mainCamera.transform.position;
        savedCamRot = mainCamera.transform.rotation;

        //print("���e�۾���m: " + savedCamPos);


        //PlayerController.isPuzzleActive = true;
        CameraController.isPuzzleActive = true;

//        InteractionUI.SetActive(false);
//        playerUI.SetActive(false);
//        SnappableItem.isPuzzleActive = true;
//        TieRodController.isPuzzleActive = true;
//        yield return StartCoroutine(CameraTransition(mainCamera, puzzleCamera, puzzleCameraTarget));
//    }

//    IEnumerator LeavePuzzle()
//    {
//        yield return StartCoroutine(CameraTransitionBack(puzzleCamera, mainCamera));

//        puzzleCamera.enabled = false;
//        mainCamera.enabled = true;

//        var camController = mainCamera.GetComponent<CameraController>();
//        if (camController != null)
//        {
//            camController.enabled = true;
//        }

        SnappableItem.isPuzzleActive = false;
        TieRodController.isPuzzleActive = false;
        //PlayerController.isPuzzleActive = false;
        CameraController.isPuzzleActive = false;

        InteractionUI.SetActive(true);
        playerUI.SetActive(true);
        player.SetActive(true);
        //playerController.isInteract = false;

        //print("���e�۾���m: " + mainCamera.transform.position);
    }

//    IEnumerator CameraTransition(Camera currentCam, Camera nextCam, Transform target)
//    {
//        float time = 0;
//        Vector3 startPos = currentCam.transform.position;
//        Quaternion startRot = currentCam.transform.rotation;
//        float duration = 0.8f;

//        while (time < duration)
//        {
//            time += Time.deltaTime;
//            float t = time / duration;

//            currentCam.transform.position = Vector3.Lerp(startPos, target.position, t);
//            currentCam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

//            yield return null;
//        }

//        currentCam.enabled = false;
//        nextCam.transform.position = target.position;
//        nextCam.enabled = true;
//    }

//    IEnumerator CameraTransitionBack(Camera currentCam, Camera nextCam)
//    {
//        float time = 0;
//        float duration = 0.8f;

//        Vector3 startPos = currentCam.transform.position;
//        Quaternion startRot = currentCam.transform.rotation;

        //print("�Q�O�s���۾��y��: " + savedCamPos);

//        while (time < duration)
//        {
//            time += Time.deltaTime;
//            float t = time / duration;

//            currentCam.transform.position =
//                Vector3.Lerp(startPos, savedCamPos, t);

//            currentCam.transform.rotation =
//                Quaternion.Slerp(startRot, savedCamRot, t);

//            yield return null;
//        }

//        nextCam.transform.position = savedCamPos;
//        nextCam.transform.rotation = savedCamRot;
//        currentCam.enabled = false;
//        nextCam.enabled = true;
//    }
//}