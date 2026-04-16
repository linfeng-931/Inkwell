using UnityEngine;

public class BrokenPillar : MonoBehaviour
{
    public GameObject brokenPillar;
    public GameObject groundPillar;
    public float pushForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        brokenPillar.SetActive(false);
        groundPillar.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        GetComponent<Collider>().enabled = false;
        Vector3 basePushDir = (transform.position - other.transform.position).normalized;
        basePushDir.y = 0;

        brokenPillar.SetActive(true);
        groundPillar.SetActive(false);

        for (int i = 0; i < brokenPillar.transform.childCount; i++)
        {
            Transform child = brokenPillar.transform.GetChild(i);
            Rigidbody rig = child.GetComponent<Rigidbody>();

            if (rig != null)
            {
                float randomX = Random.Range(-0.5f, 0.5f);
                float randomZ = Random.Range(-0.5f, 0.5f);
                
                Vector3 finalDirection = (basePushDir + new Vector3(randomX, 2f, randomZ)).normalized;

                rig.AddForce(finalDirection * pushForce, ForceMode.Impulse);
            }
        }
    }
}
