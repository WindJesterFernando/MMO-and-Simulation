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

        string netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.RandomizedSpriteIndexForClient, randSprInd.ToString());
        NetworkServerProcessing.SendMessageToClient(netMsg, clientID);

        //NetworkServerProcessing.SendMessageToClient(ServerToClientSignifiers.RandomizedSpriteIndexForClient + "," + randSprInd, clientID);


        foreach (ClientPlayerCharacterData player in idToPlayerDictionary.Values)
        {
            if (player.id != clientID)
            {
                netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.OtherConnectedClientData, clientID.ToString(), randSprInd.ToString());
                NetworkServerProcessing.SendMessageToClient(netMsg, player.id);

                netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.OtherConnectedClientData, player.id.ToString(), player.spriteIndex.ToString());
                NetworkServerProcessing.SendMessageToClient(netMsg, clientID);
            }
        }

    }

    public void RemovePlayer(int clientID)
    {
        ClientPlayerCharacterData player = idToPlayerDictionary[clientID];
        idToPlayerDictionary.Remove(clientID);
        Destroy(player.playerGameObject);
    }

    public void ProcessPlayerLerpMove(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete, int playerID)
    {
        string netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.RemotePlayerLerpMove, lerpMoveStartX.ToString(), lerpMoveStartY.ToString(), lerpMoveEndX.ToString(), lerpMoveEndY.ToString(), lerpMoveTimeUntilComplete.ToString(), playerID.ToString());

        foreach (int id in idToPlayerDictionary.Keys)
        {
            if (id != playerID)
            {
                NetworkServerProcessing.SendMessageToClient(netMsg, id);
            }
        }
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
