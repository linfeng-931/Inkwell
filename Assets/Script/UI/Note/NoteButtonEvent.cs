using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class NoteButtonEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("Main Menu Button UI")]
    [SerializeField] private Sprite menuBtnGray;
    [SerializeField] private Sprite menuBtnYellow;
    private Image buttonImage;
    private TextMeshProUGUI buttonText;

    [Header("Button SFX")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip writing;
    [SerializeField] private AudioClip clickSFX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        buttonImage.sprite = menuBtnYellow;
        buttonText.color = new Color32(254, 189, 3, 255);

        if (AudioManager.Instance != null && writing != null)
        {
            AudioManager.Instance.PlayUISFX(writing, stopPrevious: true);
        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        gameObject.GetComponent<Image>().sprite = menuBtnGray;
        buttonText.color = new Color32(50, 50, 50, 255);
    }

    public void SetDefaultColor()
    {
        gameObject.GetComponent<Image>().sprite = menuBtnGray;
        buttonText.color = new Color32(50, 50, 50, 255);
    }

    public void PlaySFX()
    {
        audioManager.PlayUISFX(clickSFX);
    }
}
