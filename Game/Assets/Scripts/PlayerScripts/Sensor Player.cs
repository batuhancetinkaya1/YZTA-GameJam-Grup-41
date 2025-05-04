using UnityEngine;
using System.Collections.Generic;

public class SensorPlayer : MonoBehaviour
{
    private List<Collider2D> m_Colliders = new List<Collider2D>(); // List to store colliders
    private float m_DisableTimer;

    public List<Collider2D> Colliders => m_Colliders; // Expose the list of colliders

    private void OnEnable()
    {
        m_Colliders.Clear();
    }

    public bool State()
    {
        if (m_DisableTimer > 0)
            return false;
        return m_Colliders.Count > 0;
    }
    public void Stater()
    {
        Debug.Log(m_Colliders);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!m_Colliders.Contains(other))
        {
            m_Colliders.Add(other); // Add collider to the list
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (m_Colliders.Contains(other))
        {
            m_Colliders.Remove(other); // Remove collider from the list
        }
    }

    void Update()
    {
        m_DisableTimer -= Time.deltaTime;
    }

    public void Disable(float duration)
    {
        m_DisableTimer = duration;
    }
}
