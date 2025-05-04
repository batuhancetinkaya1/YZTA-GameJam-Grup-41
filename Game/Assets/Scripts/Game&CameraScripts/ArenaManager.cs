using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaManager : MonoBehaviour
{
    [SerializeField] private Transform[] m_spawnPoints; // Sahnedeki spawn noktalarý
    [SerializeField] private GameObject m_playerPrefab;
    [SerializeField] private GameObject m_player2Prefab;
    [SerializeField] private GameObject m_botPrefab;
    [SerializeField] private GameObject m_bot2Prefab;

    private void Start()
    {
        string mode = PlayerPrefs.GetString("ArenaMode", "PVB");
        SetupArena(mode);
    }

    private void SetupArena(string mode)
    {
        switch (mode)
        {
            case "PVP":
                // Örneðin 2 player instantiate et
                GameObject player1 = Instantiate(m_playerPrefab, m_spawnPoints[0].position, Quaternion.identity);
                GameObject player2 = Instantiate(m_player2Prefab, m_spawnPoints[1].position, Quaternion.identity);
                break;

            case "PVB":
                // Bir player + bir bot
                GameObject player3 = Instantiate(m_playerPrefab, m_spawnPoints[0].position, Quaternion.identity);
                GameObject enemy1 = Instantiate(m_bot2Prefab, m_spawnPoints[1].position, Quaternion.identity);
                break;

            case "AIvsAI":
                // Ýki bot
                GameObject enemy2 = Instantiate(m_bot2Prefab, m_spawnPoints[0].position, Quaternion.identity);
                GameObject enemy3 = Instantiate(m_botPrefab, m_spawnPoints[1].position, Quaternion.identity);
                break;

            default:
                GameObject player4 = Instantiate(m_playerPrefab, m_spawnPoints[0].position, Quaternion.identity);
                GameObject enemy4 = Instantiate(m_bot2Prefab, m_spawnPoints[1].position, Quaternion.identity);
                break;
        }
    }
}
