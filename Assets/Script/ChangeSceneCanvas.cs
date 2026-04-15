using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneCanvas : MonoBehaviour
{
    public SceneManagement[] sceneManagement;
    public AudioSource audioSource;

    private bool musicFade;
    private float fadeSpeed;
    private string _SceneName;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicFade = false;
        fadeSpeed = 1f;
        _SceneName = sceneManagement[0].TargetScene;
    }

    // Update is called once per frame
    void Update()
    {
        if (musicFade && audioSource != null)
        {
            float volume = audioSource.volume;
            volume -= Time.deltaTime * fadeSpeed;
            if(volume <= 0f) volume = 0;
            audioSource.volume -= volume;
            musicFade = false;
        }
    }

    public void SceneSwitch()
    {
        foreach(SceneManagement i in sceneManagement)
        {
            if(i.canChange) _SceneName = i.TargetScene;
        }
        SceneManager.LoadScene(_SceneName, LoadSceneMode.Single);
    }

    public void MusicFade()
    {
        musicFade = true;
    }
}
