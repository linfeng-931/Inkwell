using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuManager : MonoBehaviour
{
    public void OnClickNewGame(string fileName)
    {
        SaveManager.pendingFileName = fileName;
        SaveManager.isPendingNewGame = true;
        SaveManager.isPendingLoad = false;

        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
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
    
    private IEnumerator LoadGameRoutine()
    {
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("BaseScene", LoadSceneMode.Single);
    }

    public void OnClickSetting()
    {
        
    }

    public void OnClickQuit()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
