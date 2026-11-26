using UnityEngine;
using UnityEngine.Events;
using Live2D.Cubism.Core;

public class PlayerLoadout : MonoBehaviour
{
    [Header("Game Object Visual")]
    public CubismModel _model;
    public CubismParameter _weaponOpacity;
    [SerializeField] private GameObject weaponVisual;
    [SerializeField] private GamePlayHUD gamePlayHUD; // Player UI
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private PlayerHealth playerHealth;
    // [SerializeField] private GameObject[] weaponParticle;

    [Header("Player State")]
    [SerializeField] private bool isCombatEnabled = false;
    [SerializeField] private bool isWeaponVisible = false;
    [SerializeField] private bool isRunEnabled = false;
    [SerializeField] private bool isShootEnabled = false;
    [SerializeField] private bool canJump = false;

    [SerializeField] private PlayerController playerController;
    public NotePresenter notePresenter;
    private float _targetWeaponOpacity = 0f;

    public bool CombatEnabled { get; private set; }
    public bool RunEnabled { get; private set; }

    public bool ShootEnabled { get; private set; }

    private void Start() // for test
    {
        EnterArmedMode();
        RunEnabled = true;
        notePresenter.SetCanOpenNote(true);

        _weaponOpacity = _model.Parameters.FindById("ParamWeaponOpacity");

    }

    public void Initialize()
    {
        RunEnabled = isRunEnabled;
        CombatEnabled = isCombatEnabled;
        ShootEnabled = isShootEnabled;
        SetWeaponVisible(isWeaponVisible);

        // Hide Player UI
        gamePlayHUD.enabled = false;
        playerEnergy.enabled = false;
        playerHealth.enabled = false;
        gamePlayHUD.gameObject.SetActive(false);

        // Lock note event
        notePresenter.SetCanOpenNote(false);

        // Hide Weapon Particle
        //foreach (GameObject particle in weaponParticle) {
        //    particle.SetActive(false);
        //}
        playerController.canJump = canJump;
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

    public void EnterRunMode()
    {
        RunEnabled = true;
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

        FadeableObject fader = weaponVisual.GetComponent<FadeableObject>();
        if (fader == null) fader = weaponVisual.AddComponent<FadeableObject>();
        fader.FadeIn(duration);

        _targetWeaponOpacity = 1f;
    }

    private void LateUpdate()
    {
        if (_weaponOpacity != null)
        {
            _weaponOpacity.Value = _targetWeaponOpacity;
        }
    }

    private void OnEnable()
    {
        GameEvent.OnGameLoaded += ApplySaveState;
    }

    private void OnDisable()
    {
        GameEvent.OnGameLoaded -= ApplySaveState;
    }

    private void ApplySaveState()
    {
        var playerData = SaveManager.Instance?.CurrentSave?.player;
        if (playerData != null && playerData.isArmed)
        {
            EnterArmedMode();
        }
    }
}