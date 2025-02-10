using System.Collections.Generic;
using UnityEngine;


public class ClientGameLogic : MonoBehaviour
{

    // [SerializeField]
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

    public void InstantiatePlayerCharacter(int spriteIndex)
    {
        playerCharacter = Instantiate(Resources.Load<GameObject>("PlayerCharacter"));
        playerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[spriteIndex];
    }

    public void InstantiateOtherPlayerCharacter(int otherPlayerID, int otherSpriteIndex) 
    {

        GameObject playerCharacter =  Instantiate(Resources.Load<GameObject>("PlayerCharacter"));
        playerCharacter.GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[otherSpriteIndex];
    }

}
