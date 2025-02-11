using System.Collections.Generic;
using UnityEngine;


public class ClientGameLogic : MonoBehaviour
{
    GameObject localPlayer;

    [SerializeField]
    List<Sprite> spritesToRandomlySelectFrom;

    Dictionary<int, GameObject> remotePlayerDictionary;

    float pingTimer;
    float nextPingTimer = TimeUntilNextPing;
    const float TimeUntilNextPing = 1;

    bool pingTimerHasBeenStarted;

    void Start()
    {
        remotePlayerDictionary = new Dictionary<int, GameObject>();
        NetworkClientProcessing.Init(this);

    }

    void Update()
    {
        if (pingTimerHasBeenStarted)//NetworkClientProcessing.IsConnectedToServer())
        {
            if (nextPingTimer <= 0)
            {
                NetworkClientProcessing.SendMessageToServer(((int)ClientToServerSignifiers.Ping).ToString());
                pingTimer = 0;
                nextPingTimer = TimeUntilNextPing;
            }

            pingTimer += Time.deltaTime;
            nextPingTimer -= Time.deltaTime;
        }
    }

    public void InstantiateLocalPlayer(int spriteIndex)
    {
        localPlayer = Instantiate(Resources.Load<GameObject>("Player"));
        localPlayer.AddComponent<LocalPlayerController>();
        localPlayer.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[spriteIndex];
    }

    public void InstantiateRemotePlayer(int otherPlayerID, int otherSpriteIndex, float xPos = 0, float yPos = 0)
    {
        GameObject remotePlayer;
        remotePlayer = Instantiate(Resources.Load<GameObject>("Player"));
        float cameraDistanceInZ = Mathf.Abs(Camera.main.transform.position.z);
        remotePlayer.transform.position = new Vector3(xPos, yPos, cameraDistanceInZ);
        RemotePlayerController rpc = remotePlayer.AddComponent<RemotePlayerController>();
        rpc.SetLerpMoveEndToCurrentPosition();
        remotePlayer.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[otherSpriteIndex];
        remotePlayerDictionary.Add(otherPlayerID, remotePlayer);
    }

    public void LerpMoveRemotePlayer(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete, int playerID)
    {
        GameObject rpc = remotePlayerDictionary[playerID];

        rpc.GetComponent<RemotePlayerController>().ReceiveLerpMoveData(lerpMoveStartX, lerpMoveStartY, lerpMoveEndX, lerpMoveEndY, lerpMoveTimeUntilComplete);
    }

    public void PrintPingTimer()
    {
        Debug.Log("Ping return time == " + (pingTimer * 1000) + " ms");
    }

    public void StartPingTimer()
    {
        pingTimerHasBeenStarted = true;
    }

    public void RemoveRemotePlayerFromDictionary(int id)
    {
        GameObject go = remotePlayerDictionary[id];
        remotePlayerDictionary.Remove(id);
        Destroy(go);
    }

}
