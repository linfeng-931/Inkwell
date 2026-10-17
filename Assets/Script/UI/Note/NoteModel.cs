using UnityEngine;

public class NoteModel : MonoBehaviour
{
    public int majorIndex {get; set;} = 0;
    public int[] subPageIndexMemory {get; set;} = new int[4]{0, 0, 0, 0}; // current page
    public int[] subPageAmount {get; set;} // total page per title

    public void Initialize(int[] pageAmounts)
    {
        subPageAmount = pageAmounts;
    }

    public int GetCurrentSubPage()
    {
        return subPageIndexMemory[majorIndex];
    }
}
