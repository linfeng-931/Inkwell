using System.Collections;
using UnityEngine;
using Live2D.Cubism.Rendering;

public class WeaponSwapFade : MonoBehaviour
{
    [SerializeField] private GameObject stoneWeapon;

    [SerializeField] private float stoneFadeOutDuration = 0.5f;
    [SerializeField] private float playerFadeInDuration = 0.1f;

    [SerializeField] private bool playerFadesInAfterStone = true;
    [SerializeField] private float gapBetween = 0.1f;
    [SerializeField] private TextEffect textEffect;

    private bool hasPlayed;

    public void Play()
    {
        if (hasPlayed) return;
        hasPlayed = true;
        StartCoroutine(SwapRoutine());
    }

    private IEnumerator SwapRoutine()
    {
        Debug.Log("[WeaponSwapFade] stone weapon fade out");
        if (textEffect != null) textEffect.StopBlinking();
        if (stoneWeapon != null)
        {
            yield return StartCoroutine(FadeOutLive2D(stoneWeapon, stoneFadeOutDuration));
        }

        if (playerFadesInAfterStone)
        {
            yield return new WaitForSeconds(stoneFadeOutDuration + gapBetween);
        }

        Debug.Log("[WeaponSwapFade] player weapon fade in");
        PlayerLoadout loadout = MapManager.Instance.player.GetComponent<PlayerLoadout>();
        if (loadout != null) loadout.FadeInWeapon(playerFadeInDuration);

        yield return new WaitForSeconds(playerFadeInDuration);

        
    }

    private IEnumerator FadeOutLive2D(GameObject target, float duration)
    {
        CubismRenderController renderController = target.GetComponent<CubismRenderController>();
        if (renderController == null)
        {
            yield break;
        }

        float startOpacity = renderController.Opacity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            renderController.Opacity = Mathf.Lerp(startOpacity, 0f, t);
            yield return null;
        }

        renderController.Opacity = 0f;
        target.SetActive(false);
    }

    public bool HasFadePlayed() {
        return hasPlayed;
    }
}