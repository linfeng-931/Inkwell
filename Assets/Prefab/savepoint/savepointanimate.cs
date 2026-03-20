using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class savepointanimate : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRenderer;
    Mesh skinnedMesh;
    int blendShapeCount;

    int playIndex = 0;

    float timer = 0f;
    public float changeDelay = 0.2f; // ´«key¶¡¹j®É¶¡¡]¬í¡^

    void Start()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        skinnedMesh = skinnedMeshRenderer.sharedMesh;
        blendShapeCount = skinnedMesh.blendShapeCount;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < changeDelay)
            return;

        timer = 0f;

        if (playIndex > 0)
            skinnedMeshRenderer.SetBlendShapeWeight(playIndex - 1, 0f);

        if (playIndex == 0)
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeCount - 1, 0f);

        skinnedMeshRenderer.SetBlendShapeWeight(playIndex, 100f);

        playIndex++;

        if (playIndex > blendShapeCount - 1)
            playIndex = 0;
    }
}