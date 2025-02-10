using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        ServerToClientSignifiers signifier = (ServerToClientSignifiers)int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.RandomizedSpriteIndexForClient)
        {
            clientGameLogic.InstantiatePlayerCharacter(int.Parse(csv[1]));
        }
        else if (signifier == ServerToClientSignifiers.OtherConnectedClientData)
        {
            clientGameLogic.InstantiateOtherPlayerCharacter(int.Parse(csv[1]), int.Parse(csv[2]));
        }
        else if (signifier == ServerToClientSignifiers.RemotePlayerLerpMove)
        {
            clientGameLogic.InstantiateOtherPlayerCharacter(int.Parse(csv[1]), int.Parse(csv[2]));
        }
        // else if (signifier == ServerToClientSignifiers.asd)
        // {

        // }

        //gameLogic.DoSomething();

    }

    static public void SendMessageToServer(string msg, TransportPipeline pipeline = TransportPipeline.ReliableAndInOrder)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    #endregion

    #region Connection Related Functions and Events
    static public void ConnectionEvent()
    {
        Debug.Log("Network Connection Event!");
    }
    static public void DisconnectionEvent()
    {
        Debug.Log("Network Disconnection Event!");
    }
    static public bool IsConnectedToServer()
    {
        return networkClient.IsConnected();
    }
    static public void ConnectToServer()
    {
        networkClient.Connect();
    }
    static public void DisconnectFromServer()
    {
        networkClient.Disconnect();
    }

    #endregion

    #region Setup
    static NetworkClient networkClient;
    static ClientGameLogic clientGameLogic;

    static public void SetNetworkedClient(NetworkClient NetworkClient)
    {
        networkClient = NetworkClient;
    }
    static public NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }
    static public void Init(ClientGameLogic clientGameLogic)
    {
        NetworkClientProcessing.clientGameLogic = clientGameLogic;
    }

    #endregion

}

#region Protocol Signifiers
public enum ClientToServerSignifiers
{
    asd = 1,
    LocalPlayerLerpMove = 2,
}

public enum ServerToClientSignifiers
{
    RandomizedSpriteIndexForClient = 1,
    OtherConnectedClientData = 2,
    RemotePlayerLerpMove = 3,
}

#endregion

