using UnityEngine;

public class PlayOnceFrameAnimator : MonoBehaviour
{
    [Header("每幀的 GameObject 列表")]
    public GameObject[] frames;

    [Header("每幀停留時間 (秒)")]
    public float frameTime = 0.1f;

    private int currentFrame = 0;
    private float timer = 0f;
    private bool isPlaying = true;

    void Start()
    {
        // 一開始先全部關閉
        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
                frames[i].SetActive(false);
        }

        // 啟動第一幀
        if (frames.Length > 0 && frames[0] != null)
            frames[0].SetActive(true);
    }

    void Update()
    {
        if (!isPlaying || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;

            // 關掉上一幀
            if (frames[currentFrame] != null)
                frames[currentFrame].SetActive(false);

            // 換下一幀
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                // 播放完畢，停在最後一幀
                currentFrame = frames.Length - 1;
                if (frames[currentFrame] != null)
                    frames[currentFrame].SetActive(true);

                isPlaying = false; // 停止播放
                return;
            }

            // 啟動新的幀
            if (frames[currentFrame] != null)
                frames[currentFrame].SetActive(true);
        }
    }
}