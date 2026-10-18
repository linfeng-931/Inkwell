using UnityEngine;

public class SlotBox : MonoBehaviour
{
    public string slotName;
    [SerializeField] private GameObject newGameBox;
    [SerializeField] private GameObject loadGameBox;

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
        
    }
}
