using UnityEngine;

public class GameLogic : MonoBehaviour
{

    [SerializeField]
    GameObject playerCharacter;


    void Start()
    {
        NetworkClientProcessing.Init(this);

    }

    void Update()
    {

    }

    public void SetSpriteForPlayerCharacter(int randomIndex)
    {
        playerCharacter.GetComponent<PlayerCharacterSpriteRandomizer>().SetSpriteFromRandomIndex(randomIndex);
    }

}
