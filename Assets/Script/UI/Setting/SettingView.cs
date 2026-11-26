using UnityEngine;

public class SettingView : MonoBehaviour
{
    public GameObject settingContent;

    public void Reset()
    {
        settingContent.SetActive(false);
    }
}
