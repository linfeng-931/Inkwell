using UnityEngine;

public class AutoDestoryEffect : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        Destroy(gameObject, animLength);
    }
}
