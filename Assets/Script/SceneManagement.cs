using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{    
    public string TargetScene;
    public Animator changeSceneAnimator;
    public bool canChange;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            changeSceneAnimator.SetTrigger("changeScene");
            canChange = true;
        }
    }
}
