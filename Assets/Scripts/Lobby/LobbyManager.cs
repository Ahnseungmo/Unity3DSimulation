using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    [Header("UI")]
    public TMP_InputField roomNameInput;
    public TMP_InputField newRoomNameInput; // 이름 변경용
    public Transform lobbyListContent;
    public GameObject lobbyListItemPrefab;
    public Button refreshButton;
    public Button createButton;
    public Button applyNameButton;

    [HideInInspector]
    public Lobby? CurrentLobby;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        try
        {
            SteamClient.Init(480); // AppID 테스트용
        }
        catch
        {
            Debug.LogError("Steam 초기화 실패");
        }

        refreshButton.onClick.AddListener(RefreshLobbyList);
        createButton.onClick.AddListener(CreateLobby);
        applyNameButton.onClick.AddListener(ChangeLobbyName);
    }

    private async void CreateLobby()
    {
        Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(4); // 최대 4명
        if (!lobby.HasValue)
        {
            Debug.LogError("로비 생성 실패");
            return;
        }

        CurrentLobby = lobby;
        string lobbyName = string.IsNullOrEmpty(roomNameInput.text) ? "New Room" : roomNameInput.text;
        CurrentLobby.Value.SetData("name", lobbyName);

        NetworkManager.Singleton.StartHost();
        Debug.Log("Host 시작 + 로비 생성: " + lobbyName);
    }

    private void ChangeLobbyName()
    {
        if (!NetworkManager.Singleton.IsHost || !CurrentLobby.HasValue) return;

        string newName = string.IsNullOrEmpty(newRoomNameInput.text) ? "New Room" : newRoomNameInput.text;
        CurrentLobby.Value.SetData("name", newName);
        Debug.Log("로비 이름 변경: " + newName);
    }

    public async void RefreshLobbyList()
    {
        var lobbies = await SteamMatchmaking.LobbyList
//            .WithDistanceFilter(LobbyDistanceFilter.Worldwide)
            .RequestAsync();

        foreach (Transform child in lobbyListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var lobby in lobbies)
        {
            GameObject item = Instantiate(lobbyListItemPrefab, lobbyListContent);
            string name = lobby.GetData("name") ?? "No Name";
            int members = lobby.MemberCount;
            int maxMembers = lobby.MaxMembers;

            LobbyListItem listItem = item.GetComponent<LobbyListItem>();
            listItem.SetLobby(lobby.Id, name, members, maxMembers);
        }
    }

    public async void JoinLobby(Steamworks.SteamId id)
    {
        Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(id);
        if (!lobby.HasValue)
        {
            Debug.LogError("로비 입장 실패");
            return;
        }

        CurrentLobby = lobby;
        NetworkManager.Singleton.StartClient();
        Debug.Log("로비 입장: " + lobby.Value.Id);
    }

    public void LeaveLobby()
    {
        if (!CurrentLobby.HasValue) return;

        CurrentLobby.Value.Leave();
        CurrentLobby = null;

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.Shutdown();

        Debug.Log("로비 나감");
    }
}
