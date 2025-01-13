using UnityEngine;

public class MainManager : MonoBehaviour
{
    void Start()
    {
        ProceduralDungeonGenerator.Init();
        DungeonVisuals.Init();
        ProceduralDungeonGenerator.ProcedurallyGenerateDungeon();
        DungeonVisuals.CreateVisualsFromModelData();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ProceduralDungeonGenerator.DestroyDungeon();
            DungeonVisuals.DestroyDungeonVisuals();
            ProceduralDungeonGenerator.ProcedurallyGenerateDungeon();
            DungeonVisuals.CreateVisualsFromModelData();
        }
    }
}
