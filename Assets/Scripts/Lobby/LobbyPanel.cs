using Unity.VisualScripting;
using UnityEngine;

public class LobbyPanel : MonoBehaviour
{

    public GameObject NewWorldPanel;
    public GameObject WorldLoadPanel;
    public GameObject JoinWorldPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickNewWorldButton()
    {
        OnDisAbleAllPanel();
        NewWorldPanel.SetActive(true);
    }
    public void OnClickWorldLoadButton()
    {
        OnDisAbleAllPanel();
        WorldLoadPanel.SetActive(true);
    }
    public void OnClickJoinWorldButton()
    {
        OnDisAbleAllPanel();
        JoinWorldPanel.SetActive(true);
    }
    public void OnClickExitButton()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    void OnDisAbleAllPanel()
    {
        NewWorldPanel.SetActive(false);
        WorldLoadPanel.SetActive(false);
        JoinWorldPanel.SetActive(false);
    }

}
