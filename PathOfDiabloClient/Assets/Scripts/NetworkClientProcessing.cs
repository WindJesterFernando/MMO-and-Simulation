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
            clientGameLogic.InstantiateLocalPlayer(int.Parse(csv[1]));
        }
        else if (signifier == ServerToClientSignifiers.NewPlayerConnectedData)
        {
            clientGameLogic.InstantiateRemotePlayer(int.Parse(csv[1]), int.Parse(csv[2]));
        }
        else if(signifier == ServerToClientSignifiers.ExistingPlayerConnectionData)
        {
            clientGameLogic.InstantiateRemotePlayer(int.Parse(csv[1]), int.Parse(csv[2]), float.Parse(csv[3]), float.Parse(csv[4]));
        }
        else if (signifier == ServerToClientSignifiers.RemotePlayerLerpMove)
        {
            clientGameLogic.LerpMoveRemotePlayer(float.Parse(csv[1]), float.Parse(csv[2]), float.Parse(csv[3]), float.Parse(csv[4]), float.Parse(csv[5]), int.Parse(csv[6]));
        }

// public enum ServerToClientSignifiers
// {
//     RandomizedSpriteIndexForClient = 1,
//     NewPlayerConnectedData = 2,
//     ExistingPlayerConnectionData = 3,
//     RemotePlayerLerpMove = 4,
// }

        // else if (signifier == ServerToClientSignifiers.asd)
        // { wawa we wa >:) King of the castle
        //   high five
         // yay!
        // } YIPPE!

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
    NewPlayerConnectedData = 2,
    ExistingPlayerConnectionData = 3,
    RemotePlayerLerpMove = 4,
}

#endregion

