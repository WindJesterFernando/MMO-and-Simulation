using UnityEngine;
using System.Collections.Generic;

public class PlayerCharacterSpriteRandomizer : MonoBehaviour
{

    [SerializeField]
    List<Sprite> spritesToRandomlySelectFrom;

    void Start()
    {
        int randInd = Random.Range(0, spritesToRandomlySelectFrom.Count);
        GetComponent<SpriteRenderer>().sprite = spritesToRandomlySelectFrom[randInd];
        
    }

    // void Update()
    // {
        
    // }
}
