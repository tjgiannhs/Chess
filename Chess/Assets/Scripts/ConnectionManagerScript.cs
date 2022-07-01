using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ConnectionManagerScript : NetworkBehaviour
{

    public NetworkList<int> gridNumbersCode;
    List<int> localLastGridNumbersCode = new List<int>();
    GameObject gameManager;
    bool playingWhite;

    void Awake()
    {
        gridNumbersCode = new NetworkList<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = new GameObject();
    }


    void Update()
    {
        if(SceneManager.GetActiveScene().name == "GameScene")
        {
            if(!CompareGridNumbersLists(gridNumbersCode,localLastGridNumbersCode))
            {
                UpdateLocalGridNumbersCode(); 
                if(gameManager == null)
                    gameManager = GameObject.FindWithTag("GameManager");
                gameManager.GetComponent<GameManagerBehavior>().UpdateBoard(localLastGridNumbersCode);
            }
        }else if(NetworkManager.Singleton.IsHost)
        {
            if(NetworkManager.Singleton.ConnectedClientsList.Count==2)
                LoadGameSceneClientRpc();
        }
    }

    bool CompareGridNumbersLists(NetworkList<int> first, List<int> second)
    {
        if(first.Count != second.Count)
            return false;
        for(int i=0; i<first.Count; i++)
        {
            if(first[i]!=second[i])
                return false;
        }
        return true;
    }

    [ClientRpc]
    void LoadGameSceneClientRpc()
    {
        playingWhite = false;
        if(NetworkManager.Singleton.IsHost) playingWhite = true;
        SceneManager.LoadScene("GameScene");
    }

    public void SetGridNumbersCodeList(List<int> l)
    {
        gridNumbersCode.Clear();
        for(int i=0; i<l.Count; i++)
        {
            gridNumbersCode.Add(l[i]);
        }
    }
    public void SetGridNumbersCodeList(int[] a)
    {
        gridNumbersCode.Clear();
        for(int i=0; i<a.Length; i++)
        {
            gridNumbersCode.Add(a[i]);
        }
    }

    void UpdateLocalGridNumbersCode()
    {
        localLastGridNumbersCode.Clear();
        for(int i=0; i<gridNumbersCode.Count; i++)
        {
            localLastGridNumbersCode.Add(gridNumbersCode[i]);
        }
    }

    public void UpdateGridNumbersCode(int[] gridCodeArray)
    {
        if(NetworkManager.Singleton.IsServer)
        {
            SetGridNumbersCodeList(gridCodeArray);
        }else
        {
            UpdateGridNumbersCodeServerRpc(gridCodeArray);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateGridNumbersCodeServerRpc(int[] gridCodeArray)
    {   
        SetGridNumbersCodeList(gridCodeArray);
    }
    public bool GetPlayingWhite()
    {
        return playingWhite;
    }
}
