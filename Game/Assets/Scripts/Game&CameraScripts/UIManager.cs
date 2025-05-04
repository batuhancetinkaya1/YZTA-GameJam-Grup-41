using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menu Panels")]
    public GameObject m_mainMenuPanel;
    public GameObject m_creditsPanel;
    public GameObject m_controlsPanel;

    // Yeni eklenen ArenaPanel
    public GameObject m_arenaPanel;

    // Volume Slider (Menu’deki)
    public Slider m_volumeSlider;

    [Header("Game Panels")]
    public GameObject m_pauseMenuPanel;
    public GameObject m_gamePanel;

    [Header("GameOver Panel")]
    public GameObject m_gameOverPanel;
    public RawImage m_winImage;
    public RawImage m_loseImage;

    [Header("Game UI Elements")]
    public TMP_Text m_starText;
    public RawImage m_keyImage;

    private int m_totalStars;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetStars();
    }

    private void Start()
    {
        // VolumeSlider PlayerPrefs ayarý:
        if (m_volumeSlider != null)
        {
            float storedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            m_volumeSlider.value = storedVolume;

            // Slider'daki deðiþim event'ini baðla
            m_volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

            // AudioManager üzerinden ses ayarlýyorsanýz, orada volume set fonksiyonu çaðýrabilirsiniz.
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(storedVolume);
            }
            else
            {
                Debug.LogWarning("AudioManager.Instance is null. Ensure AudioManager is correctly initialized.");
            }
        }
    }

    #region Volume Kontrol
    // VolumeSlider’ýn OnValueChanged event’ine baðlanýr:
    public void OnVolumeSliderChanged(float value)
    {
        // Hem AudioManager'daki müzik sesini hem de PlayerPrefs kaydýný ayarla
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    #endregion

    #region Panel Control
    public void LoadGameScene()
    {
        HideAllPanels();
        AudioManager.Instance.StopPlaylist();
        AudioManager.Instance.PlaySequentialMusic("GameMusic1", "GameMusic2");
        GameManager.Instance.ChangeState(GameStates.GameOn);
        GameManager.Instance.LoadScene(SceneNames.GameScene);
        GameManager.Instance.ControlTime(false);
    }

    // Menü Sahnesi
    public void LoadMenuScene()
    {
        HideAllPanels();
        AudioManager.Instance.StopPlaylist();
        AudioManager.Instance.PlayMusic("MenuMusic");
        GameManager.Instance.ChangeState(GameStates.Menu);
        GameManager.Instance.LoadScene(SceneNames.MenuScene);
        GameManager.Instance.ControlTime(true);
    }

    // Arena/Fight Sahnesi
    public void LoadFightScene()
    {
        HideAllPanels();
        AudioManager.Instance.StopPlaylist();
        AudioManager.Instance.PlaySequentialMusic("ArenaMusic1", "ArenaMusic2");
        GameManager.Instance.ChangeState(GameStates.FinalFight);

        // Burada sahne ismini FightScene olarak çaðýrýn:
        GameManager.Instance.LoadScene(SceneNames.FightScene);

        // Ýsterseniz zamaný dondurun ya da dondurmayýn, tasarýma baðlý:
        GameManager.Instance.ControlTime(false);
    }

    public void ShowGameOverPanel(bool isPlayerWin)
    {
        m_gamePanel.SetActive(false);
        m_gameOverPanel.SetActive(true);

        if (isPlayerWin)
        {
            Debug.Log("WÝN");
            m_winImage.enabled = true;
            m_winImage.gameObject.SetActive(true);
        }
        else if (!isPlayerWin)
        {
            Debug.Log("LOSE");
            m_loseImage.enabled = true;
            m_loseImage.gameObject.SetActive(true);
        }

        GameManager.Instance.ControlTime(true);
    }

    public void ShowPauseMenu()
    {
        m_gamePanel.SetActive(false);
        m_pauseMenuPanel.SetActive(true);
        GameManager.Instance.ControlTime(true);
    }

    public void HidePauseMenu()
    {
        m_gamePanel.SetActive(true);
        m_pauseMenuPanel.SetActive(false);
        GameManager.Instance.ControlTime(false);
    }

    public void ShowCredits()
    {
        HideAllPanels();
        m_creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        m_creditsPanel.SetActive(false);
        ShowMainMenu();
    }

    public void ShowControls()
    {
        HideAllPanels();
        m_controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        m_controlsPanel.SetActive(false);
        ShowMainMenu();
    }

    // Yeni: ArenaPanel’i gösteren fonksiyon
    public void ShowArenaPanel()
    {
        HideAllPanels();
        if (m_arenaPanel != null)
            m_arenaPanel.SetActive(true);
    }

    public void HideArenaPanel()
    {
        if (m_arenaPanel != null)
            m_arenaPanel.SetActive(false);

        ShowMainMenu();
    }

    private void HideAllPanels()
    {
        if (m_mainMenuPanel != null) m_mainMenuPanel.SetActive(false);
        if (m_creditsPanel != null) m_creditsPanel.SetActive(false);
        if (m_controlsPanel != null) m_controlsPanel.SetActive(false);
        if (m_pauseMenuPanel != null) m_pauseMenuPanel.SetActive(false);
        if (m_gameOverPanel != null) m_gameOverPanel.SetActive(false);
        if (m_gamePanel != null) m_gamePanel.SetActive(false);

        // ArenaPanel da kapatýlsýn:
        if (m_arenaPanel != null) m_arenaPanel.SetActive(false);
    }

    private void ShowMainMenu()
    {
        if (m_mainMenuPanel != null)
            m_mainMenuPanel.SetActive(true);
    }
    #endregion

    #region Star & Key
    public void AddStars(int amount)
    {
        m_totalStars += amount;
        UpdateStarText();
    }
    public void RemoveStars(int amount)
    {
        m_totalStars -= amount;
        UpdateStarText();
    }

    public void ResetStars()
    {
        m_totalStars = 0;
        UpdateStarText();
    }

    public int StarCount()
    {
        return m_totalStars;
    }

    private void UpdateStarText()
    {
        if (m_starText != null)
            m_starText.text = $"x {m_totalStars}";
    }

    public void ShowKey()
    {
        if (m_keyImage != null) m_keyImage.enabled = true;
    }

    public void HideKey()
    {
        if (m_keyImage != null) m_keyImage.enabled = false;
    }
    #endregion

    private void PlayButtonSound()
    {
        // Ses efektlerini AudioManager üzerinden tetikleyebilirsiniz:
        AudioManager.Instance.PlaySFX("Button");
    }

    #region Buttons MenuScene
    public void OnPlayButton()
    {
        PlayButtonSound();
        LoadGameScene();
    }

    public void OnCreditsButton()
    {
        PlayButtonSound();
        ShowCredits();
    }

    public void OnControlsButton()
    {
        PlayButtonSound();
        ShowControls();
    }

    public void OnHideCreditsButton()
    {
        PlayButtonSound();
        HideCredits();
    }

    public void OnHideControlsButton()
    {
        PlayButtonSound();
        HideControls();
    }

    // ArenaPanel’i açan buton:
    public void OnArenaButton()
    {
        PlayButtonSound();
        ShowArenaPanel();
    }
    #endregion

    #region ArenaPanel Buttons
    // Bu üç fonksiyon da arenada hangi modu seçtiðini saklayacak:
    public void OnPVPButton()
    {
        PlayButtonSound();

        // Seçimi PlayerPrefs’a kaydet
        PlayerPrefs.SetString("ArenaMode", "PVP");
        PlayerPrefs.Save();

        // Fight sahnesini yükle
        LoadFightScene();
    }

    public void OnPVBButton()
    {
        PlayButtonSound();
        PlayerPrefs.SetString("ArenaMode", "PVB");
        PlayerPrefs.Save();

        LoadFightScene();
    }

    public void OnAIvsAIButton()
    {
        PlayButtonSound();
        PlayerPrefs.SetString("ArenaMode", "AIvsAI");
        PlayerPrefs.Save();

        LoadFightScene();
    }

    public void OnReturnMenuButtonFromArena()
    {
        PlayButtonSound();
        HideArenaPanel();
    }
    #endregion

    #region Buttons GameScene
    public void OnReturnMenuButton()
    {
        PlayButtonSound();
        LoadMenuScene();
    }

    public void OnPauseButton()
    {
        PlayButtonSound();
        ShowPauseMenu();
    }

    public void OnContinueButton()
    {
        PlayButtonSound();
        HidePauseMenu();
    }

    public void OnGameOverReturnMenu()
    {
        PlayButtonSound();
        LoadMenuScene();
    }
    #endregion
}
