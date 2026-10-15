using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnClickNewGame()
    {
        StartCoroutine(NewGameRoutine());
    }

    private IEnumerator NewGameRoutine()
    {
        GameEvent.OnToggleFade(false);
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("BaseScene", LoadSceneMode.Single);
    }

    public void OnClickLoadGame()
    {
        
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
