using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    [Header("Player Type")]
    [SerializeField] private PlayerType m_playerType;
    public PlayerType PlayerType => m_playerType;

    // Alt bileþenler
    public PlayerMovementController MovementController { get; private set; }
    public PlayerCombatController CombatController { get; private set; }
    public PlayerHealthManager HealthManager { get; private set; }
    public PlayerAnimControl AnimControl { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerDetectionControl DetectionControl { get; private set; }
    public PlayerRespawn RespawnController { get; private set; }

    private GameManager m_gameManager;

    public delegate void PlayerSpawnHandler();
    public static event PlayerSpawnHandler OnPlayerSpawn;

    private void Awake()
    {
        OnPlayerSpawn?.Invoke();

        MovementController = GetComponent<PlayerMovementController>();
        CombatController = GetComponent<PlayerCombatController>();
        HealthManager = GetComponent<PlayerHealthManager>();
        AnimControl = GetComponent<PlayerAnimControl>();
        InputHandler = GetComponent<PlayerInputHandler>();
        DetectionControl = GetComponent<PlayerDetectionControl>();
        RespawnController = FindObjectOfType<PlayerRespawn>();

        // Initialize
        MovementController.Initialize(this);
        CombatController.Initialize(this);
        HealthManager.Initialize(this);
    }

    private void Start()
    {
        m_gameManager = GameManager.Instance;
    }

    public void OnPlayerDeath()
    {
        // GameManager var mý kontrol
        if (m_gameManager == null)
        {
            Debug.LogWarning("GameManager yok, OnPlayerDeath() => Respawn veya GameOver çaðrýlamadý.");
            return;
        }

        // FinalFight durumundaysa => GameOver
        if (m_gameManager.CurrentState == GameStates.FinalFight)
            m_gameManager.ChangeState(GameStates.GameOver);
        else
            m_gameManager.ChangeState(GameStates.Respawn);
    }
}