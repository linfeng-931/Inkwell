using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Button SFX")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip clickSFX;

    private void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
    }
    public string currentDeleteGame { get; set; } = "";

    public void OnClickNewGame(string fileName)
    {
        SaveManager.pendingFileName = fileName;
        SaveManager.isPendingNewGame = true;
        SaveManager.isPendingLoad = false;

        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        PlaySFX();
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("BaseScene", LoadSceneMode.Single);
    }

    public void OnClickLoadGame(string fileName)
    {
        SaveManager.pendingFileName = fileName;
        SaveManager.isPendingLoad = true;
        SaveManager.isPendingNewGame = false;

        StartCoroutine(LoadGameRoutine());
    }

    public void OnDeleteGame()
    {
        SaveManager.Instance.DeleteSave(currentDeleteGame);
        currentDeleteGame = "";
    }

    private IEnumerator LoadGameRoutine()
    {
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("BaseScene", LoadSceneMode.Single);
    }

    public void OnClickSetting()
    {
        PlaySFX();
    }

    public void OnClickQuit()
    {
        PlaySFX();
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PlaySFX()
    {
        audioManager.PlayUISFX(clickSFX);
    }
}
