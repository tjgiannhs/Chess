using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MyNetworkManagerScript : MonoBehaviour
{
    [SerializeField] GameObject connectionManagerPrefab;

    public void SpawnConnectionManager()
    {
        GameObject g = Instantiate(connectionManagerPrefab, Vector3.zero, Quaternion.identity);
        g.GetComponent<NetworkObject>().Spawn();
    }


}
