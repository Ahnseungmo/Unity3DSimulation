using UnityEngine;

public class TestSceneManager : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HousingRoomManager.Instance.AddRoom(new Vector3Int(0, 0, 0));
        HousingRoomManager.Instance.AddRoom(new Vector3Int(1, 0, 0));
        HousingRoomManager.Instance.AddRoom(new Vector3Int(2, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
