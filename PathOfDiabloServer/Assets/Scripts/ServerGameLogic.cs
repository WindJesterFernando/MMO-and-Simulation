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

        // if(Input.GetKeyDown(KeyCode.D))
        // {
            
            
        // }

        LinkedList<int> removeMes = new LinkedList<int>();
        foreach (ClientPlayerCharacterData player in idToPlayerDictionary.Values)
        {
            player.timeOut -= Time.deltaTime;

            if(player.timeOut <= 0)
            {
                removeMes.AddLast(player.id);
            }
        }

        foreach (int me in removeMes)
        {
            NetworkServerProcessing.GetNetworkServer().DisconnectClient(me);
            // todo:
            // disconnect
            // remove from idToPlayer dictionary
            // remove player from scene (server)
            // possibly some shit in NetworkServer class (?)
            //      related to NetworkEvent.Type.Disconnect?
            // send message to clients that player disconnected
            //      when received on client, remove from scene on client too
        }
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
                netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.NewPlayerConnectedData,
                                                clientID.ToString(),
                                                randSprInd.ToString());
                NetworkServerProcessing.SendMessageToClient(netMsg, player.id);
            }
        }

        foreach (ClientPlayerCharacterData player in idToPlayerDictionary.Values)
        {
            if (player.id != clientID)
            {
                float xPos = player.playerGameObject.transform.position.x;
                float yPos = player.playerGameObject.transform.position.y;
                netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.ExistingPlayerConnectionData,
                                                player.id.ToString(),
                                                player.spriteIndex.ToString(),
                                                xPos.ToString(),
                                                yPos.ToString());
                NetworkServerProcessing.SendMessageToClient(netMsg, clientID);
            }
        }

        //         NewPlayerConnectedData = 2,
        // ExistingPlayerConnectionData = 3,

    }

    public void RemovePlayer(int clientID)
    {
        ClientPlayerCharacterData player = idToPlayerDictionary[clientID];
        idToPlayerDictionary.Remove(clientID);
        Destroy(player.playerGameObject);
    }

    public void ProcessPlayerLerpMove(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete, int playerID)
    {
        // todo set player position (on server) to destination
        idToPlayerDictionary[playerID].playerGameObject.transform.position = new Vector3(lerpMoveEndX, lerpMoveEndY, 10);

        string netMsg = Utilities.Concatenate((int)ServerToClientSignifiers.RemotePlayerLerpMove, lerpMoveStartX.ToString(), lerpMoveStartY.ToString(), lerpMoveEndX.ToString(), lerpMoveEndY.ToString(), lerpMoveTimeUntilComplete.ToString(), playerID.ToString());

        foreach (int id in idToPlayerDictionary.Keys)
        {
            if (id != playerID)
            {
                NetworkServerProcessing.SendMessageToClient(netMsg, id);
            }
        }
    }

    public void KeepClientConnectionAlive(int clientConnectionID)
    {
        idToPlayerDictionary[clientConnectionID].timeOut = NetworkServerProcessing.TimeOutReset;
    }
}

public class ClientPlayerCharacterData
{
    public int id;
    public int spriteIndex;
    public GameObject playerGameObject;

    public float timeOut = NetworkServerProcessing.TimeOutReset;

    public ClientPlayerCharacterData(int id, int spriteIndex, GameObject playerGameObject)
    {
        this.id = id;
        this.spriteIndex = spriteIndex;
        this.playerGameObject = playerGameObject;
    }



}
