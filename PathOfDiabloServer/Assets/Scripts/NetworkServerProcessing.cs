using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkServerProcessing
{

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from connection id = " + clientConnectionID + ", from pipeline = " + pipeline);

        serverGameLogic.KeepClientConnectionAlive(clientConnectionID);

        string[] csv = msg.Split(',');
        ClientToServerSignifiers signifier = (ClientToServerSignifiers)int.Parse(csv[0]);

        if (signifier == ClientToServerSignifiers.asd)
        {

        }
        else if (signifier == ClientToServerSignifiers.LocalPlayerLerpMove)
        {
            serverGameLogic.ProcessPlayerLerpMove(float.Parse(csv[1]), float.Parse(csv[2]), float.Parse(csv[3]), float.Parse(csv[4]), float.Parse(csv[5]), clientConnectionID);
        }
        else if (signifier == ClientToServerSignifiers.Ping)
        {
            SendMessageToClient(((int)ServerToClientSignifiers.Pong).ToString(), clientConnectionID);
        }

    }
    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline = TransportPipeline.ReliableAndInOrder)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);

        serverGameLogic.CreatePlayerPrefab(clientConnectionID);



    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client disconnection, ID == " + clientConnectionID);
        serverGameLogic.RemovePlayer(clientConnectionID);
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static ServerGameLogic serverGameLogic;

    public const float TimeOutReset = 5;

    static public void SetNetworkServer(NetworkServer NetworkServer)
    {
        networkServer = NetworkServer;
    }
    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }
    static public void Init(ServerGameLogic serverGameLogic)
    {
        NetworkServerProcessing.serverGameLogic = serverGameLogic;
    }

    #endregion
}

#region Protocol Signifiers
public enum ClientToServerSignifiers
{
    asd = 1,
    LocalPlayerLerpMove = 2,
    Ping = 3,
}

public enum ServerToClientSignifiers
{
    RandomizedSpriteIndexForClient = 1,
    NewPlayerConnectedData = 2,
    ExistingPlayerConnectionData = 3,
    RemotePlayerLerpMove = 4,
    Pong = 5,


}

#endregion

