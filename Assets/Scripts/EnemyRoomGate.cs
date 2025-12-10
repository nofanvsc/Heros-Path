using UnityEngine;
using System.Collections.Generic;

public class EnemyRoomGate : MonoBehaviour
{
    [Header("Enemies required to be killed")]
    public List<GameObject> enemies = new List<GameObject>();

    [Header("The object blocking the path")]
    public GameObject pathBlocker;

    private bool gateOpened = false;

    void Update()
    {
        if (gateOpened) return;
        CleanList();

        if (enemies.Count == 0)
        {
            OpenGate();
        }
    }

  
    void CleanList()
    {
        enemies.RemoveAll(e => e == null);
    }

    void OpenGate()
    {
        gateOpened = true;

        if (pathBlocker != null)
        {
            Destroy(pathBlocker); 
        }

        Debug.Log("All enemies defeated — gate opened!");
    }
}
