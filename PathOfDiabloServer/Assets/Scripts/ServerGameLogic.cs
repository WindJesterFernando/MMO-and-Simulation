using System.Collections.Generic;
using UnityEngine;

public class ServerGameLogic : MonoBehaviour
{

    [SerializeField]
    GameObject playerPrefab;

    Dictionary<int, ClientPlayerCharacterData> idToPlayerDictionary;

    void Start()
    {
        idToPlayerDictionary = new Dictionary<int, ClientPlayerCharacterData>();
        NetworkServerProcessing.Init(this);
    }

    void Update()
    {

    }

    public void CreatePlayerPrefab(int clientID)
    {
        GameObject p = Instantiate(playerPrefab);
        const int NumberOfSpritesToRandomlySelectFrom = 12;
        int randSprInd = Random.Range(0, NumberOfSpritesToRandomlySelectFrom);
        ClientPlayerCharacterData pData = new ClientPlayerCharacterData(clientID, randSprInd, p);
        idToPlayerDictionary.Add(clientID, pData);

        NetworkServerProcessing.SendMessageToClient(ServerToClientSignifiers.RandomizedSpriteIndexForClient + "," + randSprInd, clientID);

        
        foreach(ClientPlayerCharacterData player in idToPlayerDictionary.Values)
        {
            if (player.id != clientID)
            {
                NetworkServerProcessing.SendMessageToClient(ServerToClientSignifiers.OtherConnectedClientData + "," + clientID + "," + randSprInd, player.id);
            }
        }

    }

    public void RemovePlayer(int clientID) 
    {
        ClientPlayerCharacterData player = idToPlayerDictionary[clientID];
        idToPlayerDictionary.Remove(clientID);
        Destroy(player.playerGameObject);
    }
}

public class ClientPlayerCharacterData
{
    public int id;
    public int spriteIndex;
    public GameObject playerGameObject;

    public ClientPlayerCharacterData(int id, int spriteIndex, GameObject playerGameObject)
    {
        this.id = id;
        this.spriteIndex = spriteIndex;
        this.playerGameObject = playerGameObject;
    }
}
