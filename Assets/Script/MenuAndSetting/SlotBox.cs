using System;
using TMPro;
using UnityEngine;

public class SlotBox : MonoBehaviour
{
    public string slotName;
    [SerializeField] private GameObject newGameBox;
    [SerializeField] private GameObject loadGameBox;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private TextMeshProUGUI timeText;

    public void Initialize(bool hasData)
    {
        if (hasData)
        {
            newGameBox.SetActive(false);
            loadGameBox.SetActive(true);
            SetLoadGameBox();
        }
        else
        {
            newGameBox.SetActive(true);
            loadGameBox.SetActive(false);
        }
    }

    private void SetLoadGameBox()
    {
        var metaData = SaveManager.Instance.GetSaveMetaData(slotName);

        if (metaData != null)
        {
            locationText.text = string.IsNullOrEmpty(metaData.currentLocation) ? "未知地點" : metaData.currentLocation;
            string timeText = TimeSpan.FromSeconds(metaData.playTime).ToString("hh':'mm':'ss");
            this.timeText.text = timeText;
        }
    }
}
