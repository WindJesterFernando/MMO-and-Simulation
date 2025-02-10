using System.Collections.Generic;
using UnityEngine;


public class ClientGameLogic : MonoBehaviour
{
    GameObject localPlayerCharacter;

    [SerializeField]
    List<Sprite> spritesToRandomlySelectFrom;

    Dictionary<int, GameObject> remotePlayerCharacterDictionary;


    void Start()
    {
        remotePlayerCharacterDictionary = new Dictionary<int, GameObject>();
        NetworkClientProcessing.Init(this);

    }

    void Update()
    {

    }

    public void InstantiatePlayerCharacter(int spriteIndex)
    {
        localPlayerCharacter = Instantiate(Resources.Load<GameObject>("PlayerCharacter"));
        localPlayerCharacter.AddComponent<PlayerCharacterController>();
        localPlayerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[spriteIndex];
    }

    public void InstantiateOtherPlayerCharacter(int otherPlayerID, int otherSpriteIndex)
    {
        GameObject remotePlayerCharacter;
        remotePlayerCharacter = Instantiate(Resources.Load<GameObject>("PlayerCharacter"));
        remotePlayerCharacter.AddComponent<RemotePlayerCharacter>();
        remotePlayerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[otherSpriteIndex];
        remotePlayerCharacterDictionary.Add(otherPlayerID, remotePlayerCharacter);
    }

    public void LerpMoveRemotePlayer(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete, int playerID)
    {
        GameObject rpc = remotePlayerCharacterDictionary[playerID];

        rpc.GetComponent<RemotePlayerCharacter>().ReceiveLerpMoveData(lerpMoveStartX, lerpMoveStartY, lerpMoveEndX, lerpMoveEndY, lerpMoveTimeUntilComplete);
    }

}
