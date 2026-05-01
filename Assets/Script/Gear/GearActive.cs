using UnityEngine;

public class GearActive : MonoBehaviour
{
    public AudioSource gearRotMusic;
    public float seekTime = 9.0f;

    // Update is called once per frame
    public void PlaySound()
    {
        if (gearRotMusic!=null)
        {
            gearRotMusic.time = seekTime;
            gearRotMusic.Play();
        }
    }
}
