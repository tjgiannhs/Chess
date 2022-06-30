using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MenuButtonScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.GetComponent<MyNetworkManagerScript>().SpawnConnectionManager();
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.StartClient();
    }
}
