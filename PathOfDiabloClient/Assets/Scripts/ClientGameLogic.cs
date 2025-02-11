using System.Collections.Generic;
using UnityEngine;


public class ClientGameLogic : MonoBehaviour
{
    GameObject localPlayer;

    [SerializeField]
    List<Sprite> spritesToRandomlySelectFrom;

    Dictionary<int, GameObject> remotePlayerDictionary;


    void Start()
    {
        remotePlayerDictionary = new Dictionary<int, GameObject>();
        NetworkClientProcessing.Init(this);

    }

    void Update()
    {

    }

    public void InstantiateLocalPlayer(int spriteIndex)
    {
        localPlayer = Instantiate(Resources.Load<GameObject>("Player"));
        localPlayer.AddComponent<LocalPlayerController>();
        localPlayer.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[spriteIndex];
    }

    public void InstantiateRemotePlayer(int otherPlayerID, int otherSpriteIndex)
    {
        GameObject remotePlayer;
        remotePlayer = Instantiate(Resources.Load<GameObject>("Player"));
        remotePlayer.AddComponent<RemotePlayerController>();
        remotePlayer.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[otherSpriteIndex];
        remotePlayerDictionary.Add(otherPlayerID, remotePlayer);
    }

    public void LerpMoveRemotePlayer(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete, int playerID)
    {
        GameObject rpc = remotePlayerDictionary[playerID];

        rpc.GetComponent<RemotePlayerController>().ReceiveLerpMoveData(lerpMoveStartX, lerpMoveStartY, lerpMoveEndX, lerpMoveEndY, lerpMoveTimeUntilComplete);
    }

}
