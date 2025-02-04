using System.Collections.Generic;
using UnityEngine;


public class ClientGameLogic : MonoBehaviour
{

    [SerializeField]
    GameObject playerCharacter;

    [SerializeField]
    List<Sprite> spritesToRandomlySelectFrom;


    void Start()
    {
        NetworkClientProcessing.Init(this);

    }

    void Update()
    {

    }

    public void SetSpriteForPlayerCharacter(int randomIndex)
    {
        playerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[randomIndex];
    }

    public void InstantiateOtherPlayerCharacter(int otherPlayerID, int otherSpriteIndex) 
    {
        GameObject otherPlayerCharacter = Instantiate(playerCharacter);
        otherPlayerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[otherSpriteIndex];
    }

}
