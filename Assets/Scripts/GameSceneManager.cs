using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async Task Start()
    {
        //////////////디버깅용 호스트 생성 빌드시 삭제/////////////
        var lobby = await SteamMatchmaking.CreateLobbyAsync(4);
        lobby.Value.SetData("name", "asdf");
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started on Steam P2P");
        ///////////////////////////////////////

  
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            WorldObjectManager.Instance.SpawnObjectServerRpc("table", new Vector3(5, 0, 0),new Quaternion(0,0,0,0));
        }
    }
}
