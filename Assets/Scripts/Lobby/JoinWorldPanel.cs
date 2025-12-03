using UnityEngine;

public class JoinWorldPanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
