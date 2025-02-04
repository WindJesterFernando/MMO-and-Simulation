using UnityEngine;

public class ServerGameLogic : MonoBehaviour
{

    [SerializeField]
    GameObject playerPrefab;


    void Start()
    {
        NetworkServerProcessing.Init(this);
    }

    void Update()
    {
        
    }

    public void CreatePlayerPrefab(int id)
    {
        Instantiate(playerPrefab);
    }

}
