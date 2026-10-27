using System;
using UnityEngine;

public class InspirationSpawn : MonoBehaviour
{
    [SerializeField] private int amount = 27;
    [SerializeField] private float speed = 10;
    [SerializeField] private float startPower = 3;
    [SerializeField] private GameObject inspirationPrefab;

    public void Spawn()
    {
        int count5 = amount / 5;
        int remainder = amount % 5;    
        int count3 = remainder / 3;    
        int count1 = remainder % 3;

        SpawnGroup(count5, 5, 0.15f);
        SpawnGroup(count3, 3, 0.1f);
        SpawnGroup(count1, 1, 0.05f);
    }

    private void SpawnGroup(int count, int numValue, float scaleMultiplier)
    {
        for (int i = 0; i < count; i++)
        {
            // spawn obj
            GameObject obj = Instantiate(inspirationPrefab, transform.position, Quaternion.identity);
            
            // change scale
            obj.transform.localScale = Vector3.one * scaleMultiplier;

            // set speed
            if (obj.TryGetComponent<InspirationEssence>(out InspirationEssence essence))
            {
                Vector3 randomDir = UnityEngine.Random.onUnitSphere;

                float randomPower = UnityEngine.Random.Range(startPower * 0.4f, startPower);
                Vector3 burstVelocity = randomDir * startPower;
                
                essence.Initialize(numValue, speed, burstVelocity);
            }
        }
    }
}
