using UnityEngine;

public class Effect : MonoBehaviour
{
    public float existTime;
    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= existTime)
        {
            Destroy(gameObject);
        }
    }
}
