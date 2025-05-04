using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform m_target; // Ana hedef (Player)
    [SerializeField] private Transform m_finalFightCenter; // FinalFight merkezi

    [Header("Follow Settings")]
    [SerializeField] private float m_smoothTime = 0.2f;
    private Vector3 m_velocity = Vector3.zero;
    [SerializeField] private Vector3 m_offset = new Vector3(0f, 0f, -10f);

    [Header("Zoom Settings (Orthographic)")]
    [SerializeField] private float m_defaultZoom = 5f;
    [SerializeField] private float m_minZoom = 3f;
    [SerializeField] private float m_maxZoom = 8f;
    [SerializeField] private float m_zoomSpeed = 2f;

    [Header("State-Specific Settings")]
    [SerializeField] private float m_catEncounterZoom = 3f;
    [SerializeField] private float m_finalFightZoom = 8f;
    [SerializeField] private float m_respawnWaitTime = 1.5f;

    private Camera m_cam;
    private float m_currentZoom;
    private bool m_isRespawning = false;

    private void Awake()
    {
        // Kamerayý bul
        m_cam = GetComponent<Camera>();
        if (m_cam == null)
        {
            m_cam = Camera.main;
            Debug.LogWarning("CameraController: Script is not on the main camera, using Camera.main instead.");
        }
        m_currentZoom = m_defaultZoom;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChange += HandleGameStateChange;
        PlayerCore.OnPlayerSpawn += HandlePlayerSpawn;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= HandleGameStateChange;
        PlayerCore.OnPlayerSpawn -= HandlePlayerSpawn;
    }

    private void HandlePlayerSpawn()
    {
        // Eðer sahnede "Player" tag'li obje yoksa m_target null kalýr
        if (m_target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
            {
                m_target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraController: No Player found with tag 'Player'.");
            }
        }
    }

    private void FixedUpdate()
    {
        if (m_target == null || m_isRespawning)
        {
            return;
        }

        // Kamera pozisyonunu hedefle eþleþtir
        Vector3 desiredPosition = m_target.position + m_offset;
        float smoothedX = Mathf.SmoothDamp(transform.position.x, desiredPosition.x, ref m_velocity.x, m_smoothTime);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, desiredPosition.y, ref m_velocity.y, m_smoothTime);
        float newZ = desiredPosition.z; // Offset Z

        transform.position = new Vector3(smoothedX, smoothedY, newZ);

        // Zoom kontrolü
        if (m_cam.orthographic)
        {
            float nextZoom = Mathf.Lerp(m_cam.orthographicSize, m_currentZoom, Time.deltaTime * m_zoomSpeed);
            m_cam.orthographicSize = nextZoom;
        }
        else
        {
            float fov = Mathf.Lerp(m_cam.fieldOfView, m_currentZoom, Time.deltaTime * m_zoomSpeed);
            m_cam.fieldOfView = fov;
        }
    }

    private void HandleGameStateChange(GameStates newState)
    {
        switch (newState)
        {
            case GameStates.GameOn:
                ResetCamera();
                break;

            case GameStates.CatEncounter:
                FocusOnPlayer(m_catEncounterZoom);
                break;

            case GameStates.FinalFight:
                FocusOnFinalFight();
                break;

            case GameStates.Respawn:
                StartRespawnSequence();
                break;

            case GameStates.GameOver:
                StopCameraMovement();
                break;
        }
    }

    private void ResetCamera()
    {
        m_currentZoom = m_defaultZoom;
        m_offset = new Vector3(0, 0, -10);
    }

    private void FocusOnPlayer(float zoomLevel)
    {
        m_currentZoom = Mathf.Clamp(zoomLevel, m_minZoom, m_maxZoom);
        m_offset = new Vector3(0, 0, -10);
    }

    private void FocusOnFinalFight()
    {
        if (m_finalFightCenter != null)
        {
            m_currentZoom = Mathf.Clamp(m_finalFightZoom, m_minZoom, m_maxZoom);
            transform.position = new Vector3(m_finalFightCenter.position.x, m_finalFightCenter.position.y, m_offset.z);
        }
    }

    private void StartRespawnSequence()
    {
        m_isRespawning = true;
        Invoke(nameof(EndRespawnSequence), m_respawnWaitTime);
    }

    private void EndRespawnSequence()
    {
        m_isRespawning = false;
        ResetCamera();
    }

    private void StopCameraMovement()
    {
        m_isRespawning = true; // Kamerayý tamamen dondur
    }

    public void SetZoom(float newZoom)
    {
        m_currentZoom = Mathf.Clamp(newZoom, m_minZoom, m_maxZoom);
    }

    public void ResetZoom()
    {
        m_currentZoom = m_defaultZoom;
    }
}
