using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameOverHandler : MonoBehaviour
{
    private void OnEnable()
    {
        // GameState deðiþtiðinde tetiklenecek
        GameManager.OnGameStateChange += HandleGameStateChange;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChange -= HandleGameStateChange;
    }

    private void HandleGameStateChange(GameStates newState)
    {
        if (newState == GameStates.GameOver)
        {
            AudioManager.Instance.PlaySFXWithNewSource("Victory", transform.position);
            StartCoroutine(EndTheGame());
            //UIManager.Instance.ShowGameOverPanel(SetGameOverImage());
        }
    }

    private bool SetGameOverImage()
    {
        // Tüm oyuncularý kontrol et
        PlayerCore[] players = FindObjectsOfType<PlayerCore>();
        foreach (var player in players)
        {
            if (player.HealthManager.IsAlive) // Oyuncu hayatta mý?
            {
                if (player.PlayerType == PlayerType.PlayerOne || player.PlayerType == PlayerType.AI1)
                {
                    // Player1 kazandýysa GameOverImageWin aktif
                    return true;
                }
                else if (player.PlayerType == PlayerType.PlayerTwo || player.PlayerType == PlayerType.AI2)
                {
                    // Player2 kazandýysa GameOverImageLose aktif
                    return false;
                }
            }
            
        }
        return true;
    }

    private IEnumerator EndTheGame()
    {
        yield return new WaitForSeconds(5f);
        UIManager.Instance.ShowGameOverPanel(SetGameOverImage());
    }
}
