using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private Transform m_spawnPoint; // Sahnedeki spawn noktalarý
    [SerializeField] private GameObject m_playerPrefab;

    private void Start()
    {
        SetupGame();
    }

    private void SetupGame()
    {
        GameObject player = Instantiate(m_playerPrefab, m_spawnPoint.position, Quaternion.identity);
    }
}
