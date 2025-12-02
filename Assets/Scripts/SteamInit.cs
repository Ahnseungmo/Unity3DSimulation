using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class SteamInit : MonoBehaviour
{
    Lobby? lobby;
    private List<Lobby> lobbyList = new List<Lobby>();
    void Awake()
    {
    

    }

    private async Task Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            /*
            foreach (var friend in SteamFriends.GetFriends())
            {
                if (!friend.IsOnline)
                {
                    Debug.Log($"{friend.Name}"); // 온라인인 친구 닉넴
                }
            }
            */
            lobby = await SteamMatchmaking.CreateLobbyAsync(4);
            lobby.Value.SetData("name", "asdf");
            StartHost();
//            NetworkManager.Singleton.server
//            SteamMatchmaking.lobb
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            /*
            foreach (var friend in SteamFriends.GetFriends())
            {
                if (!friend.IsOnline)
                {
                    Debug.Log($"{friend.Name}"); // 온라인인 친구 닉넴
                }
            }
            */
            StartClient();
//            Lobby lobby = SteamMatchmaking.lobby
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            /*
            foreach (var friend in SteamFriends.GetFriends())
            {
                if (!friend.IsOnline)
                {
                    Debug.Log($"{friend.Name}"); // 온라인인 친구 닉넴
                }
            }
            */

            RequestLobbyList();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            /*
            foreach (var friend in SteamFriends.GetFriends())
            {
                if (!friend.IsOnline)
                {
                    Debug.Log($"{friend.Name}"); // 온라인인 친구 닉넴
                }
            }
            */
            //            SteamMatchmaking.JoinLobbyAsync();
            LeaveLobby();
            Debug.Log("Leave Lobby");

        }
    }
    void OnApplicationQuit()
    {

    }
    public void LeaveLobby()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            lobby.Value.Leave();
            lobby = null;
        }

        NetworkManager.Singleton.Shutdown();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started on Steam P2P");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client connecting to host via Steam P2P");
    }

    public async void RequestLobbyList()
    {

         Debug.Log("Searching lobby list...");

        var result = await SteamMatchmaking.LobbyList
//            .WithSlotsAvailable()      // 빈 자리 있는 로비만
            .FilterDistanceClose()     // 거리 가까운 순
            .RequestAsync();           // 비동기로 요청

        if (result == null || result.Length == 0)
        {
            Debug.Log("No lobby found");
            return;
        }

        lobbyList.Clear();
        lobbyList.AddRange(result);

        Debug.Log($"Lobby found: {lobbyList.Count}");


        foreach (var lobby in lobbyList)
        {
                Debug.Log($"{lobby.GetData("name")}"); 
            
        }

        // UI 업데이트
        //        LobbyUIManager.Instance.UpdateLobbyList(lobbyList);
    }
}
