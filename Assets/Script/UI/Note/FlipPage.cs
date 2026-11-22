using UnityEngine;
using UnityEngine.UI;

public class FlipPage : MonoBehaviour
{
    private Animator ani;
    private Image image;

    [Header("Main Menu Button SFX")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip flip;

    void Awake()
    {
        ani = GetComponent<Animator>();
        image = GetComponent<Image>();
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void Flip(bool isLeft)
    {
        if (AudioManager.Instance != null && flip != null)
        {
            AudioManager.Instance.PlayUISFX(flip, stopPrevious: true);
        }

        if (isLeft)
        {
            ani.SetTrigger("left");
        }
        else
        {
            ani.SetTrigger("right");
        }
    }

    public void DisableImage()
    {
        image.enabled = false;
    }

    public void EnableImage()
    {
        image.enabled = true;
    }
}
