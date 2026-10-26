using System.Collections;
using UnityEngine;

public class WeaponSwapFade : MonoBehaviour
{
    [SerializeField] private FadeableObject stoneWeapon;

    [SerializeField] private float stoneFadeOutDuration = 0.1f;
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
        if (stoneWeapon != null) stoneWeapon.FadeOut(stoneFadeOutDuration);

        if (playerFadesInAfterStone)
        {
            yield return new WaitForSeconds(stoneFadeOutDuration + gapBetween);
        }

        Debug.Log("[WeaponSwapFade] player weapon fade in");
        PlayerLoadout loadout = MapManager.Instance.player.GetComponent<PlayerLoadout>();
        if (loadout != null) loadout.FadeInWeapon(playerFadeInDuration);

        yield return new WaitForSeconds(playerFadeInDuration);

        
    }

    public bool HasFadePlayed() {
        return hasPlayed;
    }
}