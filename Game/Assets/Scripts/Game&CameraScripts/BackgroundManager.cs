using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundManager : MonoBehaviour
{
    public List<Tilemap> m_tilemaps; // T�m Tilemap bile�enleri burada
    private Color m_morningColor = new Color(1f, 0.9f, 0.7f, 1f); // Sabah tonu
    private Color m_noonColor = new Color(1f, 1f, 1f, 1f);         // ��len tonu
    private Color m_eveningColor = new Color(0.7f, 0.5f, 0.5f, 1f); // Ak�am tonu
    private Color m_nightColor = new Color(0.4f, 0.4f, 0.6f, 1f);  // Gece tonu

    private void Start()
    {
        UpdateBackgroundLighting();
    }

    private void UpdateBackgroundLighting()
    {
        int currentHour = System.DateTime.Now.Hour;

        Color selectedColor;

        if (currentHour >= 6 && currentHour < 12)
        {
            selectedColor = m_morningColor; // Sabah
        }
        else if (currentHour >= 12 && currentHour < 18)
        {
            selectedColor = m_noonColor; // ��len
        }
        else if (currentHour >= 18 && currentHour < 21)
        {
            selectedColor = m_eveningColor; // Ak�am
        }
        else
        {
            selectedColor = m_nightColor; // Gece
        }

        // T�m Tilemap'lerin rengini ayarla
        foreach (Tilemap tilemap in m_tilemaps)
        {
            tilemap.color = selectedColor;
        }
    }
}
