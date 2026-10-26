using UnityEngine;
using UnityEngine.Events;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Game Object Visual")]
    [SerializeField] private GameObject weaponVisual;
    [SerializeField] private GamePlayHUD gamePlayHUD; // Player UI
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerHealth playerHealth;
    // [SerializeField] private GameObject[] weaponParticle;

    [Header("Player State")]
    [SerializeField] private bool isCombatEnabled = false;
    [SerializeField] private bool isWeaponVisible = false;

    [SerializeField] private PlayerController playerController;

    public bool CombatEnabled { get; private set; }

    public void Initialize()
    {
        CombatEnabled = isCombatEnabled;
        SetWeaponVisible(isWeaponVisible);

        // Hide Player UI
        gamePlayHUD.enabled = false;
        playerEnergy.enabled = false;
        playerHealth.enabled = false;
        gamePlayHUD.gameObject.SetActive(false);

        // Hide Weapon Particle
        //foreach (GameObject particle in weaponParticle) {
        //    particle.SetActive(false);
        //}
    }

    // Show Weapon and Start Combat System
    public void EnterArmedMode()
    {
        SetWeaponVisible(true);
        CombatEnabled = true;

        // Show Player UI
        gamePlayHUD.gameObject.SetActive(true);
        gamePlayHUD.enabled = true;
        playerEnergy.enabled = true;
        playerHealth.enabled = true;

        // Show Weapon Particle
        //foreach (GameObject particle in weaponParticle)
        //{
        //    particle.SetActive(true);
        //}

        playerController.canJump = true;
    }

    private void SetWeaponVisible(bool visible)
    {
        if (weaponVisual != null)
        {
            weaponVisual.SetActive(visible);
        }
    }

    public void FadeInWeapon(float duration)
    {
        if (weaponVisual == null) return;

        // Show Weapon and open combat system
        FadeableObject fader = weaponVisual.GetComponent<FadeableObject>();
        if (fader == null) fader = weaponVisual.AddComponent<FadeableObject>();
        fader.FadeIn(duration);
        EnterArmedMode();
    }
}