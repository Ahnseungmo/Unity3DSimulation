using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
