using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private GameStates m_currentState;
    public GameStates CurrentState => m_currentState;

    public delegate void GameStateChangeHandler(GameStates newState);
    public static event GameStateChangeHandler OnGameStateChange;

    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup m_fadeCanvasGroup; // Siyah bir Canvas Group
    [SerializeField] private float m_fadeDuration = 1f; // Geçiþ süresi

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (m_fadeCanvasGroup != null)
            m_fadeCanvasGroup.alpha = 0; // Ýlk baþta görünmez.

        ChangeState(GameStates.GameOn);

        ControlTime(false); //buna dikkat edelim
        //AudioManager.Instance.PlayMusic("MenuMusic");
    }

    public void ChangeState(GameStates newState)
    {
        if (m_currentState == newState)
            return;

        m_currentState = newState;
        Debug.Log($"Game State Changed: {newState}");
        OnGameStateChange?.Invoke(newState);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeTransition(sceneName));
    }

    private IEnumerator FadeTransition(string sceneName)
    {
        if (m_fadeCanvasGroup != null)
        {
            // Ekraný karart
            m_fadeCanvasGroup.DOFade(1, m_fadeDuration);
            yield return new WaitForSeconds(m_fadeDuration);
        }

        // Sahneyi yükle
        //DOTween.Kill();
        SceneManager.LoadScene(sceneName);

        if (m_fadeCanvasGroup != null)
        {
            // Ekraný aç
            m_fadeCanvasGroup.DOFade(0, m_fadeDuration);
            yield return new WaitForSeconds(m_fadeDuration);
        }
    }

    public void ControlTime(bool isStop)
    {
        Time.timeScale = isStop ? 0f : 1f;
    }
}
