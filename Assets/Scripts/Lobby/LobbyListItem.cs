using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class LobbyListItem : MonoBehaviour
{
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;

    private SteamId lobbyID;

    public void SetLobby(SteamId id, string name, int memberCount, int maxMembers)
    {
        lobbyID = id;
        lobbyNameText.text = name;
        playerCountText.text = $"{memberCount}/{maxMembers}";

        joinButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinLobby(lobbyID);
        });
    }
}
