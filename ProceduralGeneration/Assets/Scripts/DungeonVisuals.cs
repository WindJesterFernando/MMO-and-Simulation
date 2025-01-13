using System.Collections.Generic;
using UnityEngine;

public static class DungeonVisuals
{
    static UnityEngine.GameObject DungeonVisualRepresentationParent;

    static Dictionary<RoomType, Color> roomTypeToColorLookup;
    static Dictionary<DoorType, Color> doorTypeToColorLookup;

    public static void Init()
    {
        roomTypeToColorLookup = new Dictionary<RoomType, Color>();
        roomTypeToColorLookup.Add(RoomType.Normal, Color.white);
        roomTypeToColorLookup.Add(RoomType.Start, Color.green / 2f + Color.white / 2f);
        roomTypeToColorLookup.Add(RoomType.Secret, Color.blue / 2f + Color.white / 2f);
        roomTypeToColorLookup.Add(RoomType.SuperSecret, Color.blue);
        roomTypeToColorLookup.Add(RoomType.Shop, Color.yellow / 2f + Color.white / 2f);
        roomTypeToColorLookup.Add(RoomType.Treasure, Color.yellow);
        roomTypeToColorLookup.Add(RoomType.Trap, Color.red / 2f + Color.white / 2f);
        roomTypeToColorLookup.Add(RoomType.Boss, Color.red);
        roomTypeToColorLookup.Add(RoomType.None, Color.magenta);

        doorTypeToColorLookup = new Dictionary<DoorType, Color>();
        doorTypeToColorLookup.Add(DoorType.Open, Color.white / 2f + Color.black / 2f);
        doorTypeToColorLookup.Add(DoorType.Locked, Color.green / 2f + Color.black / 2f);
        doorTypeToColorLookup.Add(DoorType.Bombable, Color.blue / 2f + Color.black / 2f);
        doorTypeToColorLookup.Add(DoorType.None, Color.magenta / 2f + Color.black / 2f);
    }

    private static GameObject CreateRoomVisual(Room room)
    {
        GameObject roomVisual = UnityEngine.GameObject.Instantiate(Resources.Load<GameObject>("Room"));
        room.visual = roomVisual;

        Coordinate coord = room.coordinate;
        roomVisual.name = "Room: " + room.type + " at " + coord.x + "," + coord.y;
        roomVisual.transform.position = new Vector3(coord.x, coord.y, 0);
        
        Color roomColor = roomTypeToColorLookup[room.type];
        roomVisual.GetComponent<SpriteRenderer>().color = roomColor;
        
        roomVisual.transform.parent = DungeonVisualRepresentationParent.transform;
        return roomVisual;
    }

    private static void CreateDoorVisual(Door door)
    {
        GameObject doorVisual = UnityEngine.GameObject.Instantiate(Resources.Load<GameObject>("Door"));
        door.visual = doorVisual;

        Vector2 positionOfRoom1 = new Vector2(door.room1.coordinate.x, door.room1.coordinate.y);
        Vector2 positionOfRoom2 = new Vector2(door.room2.coordinate.x, door.room2.coordinate.y);
        Vector2 positionOfDoor = (positionOfRoom1 + positionOfRoom2) / 2f;
        doorVisual.transform.position = new Vector3(positionOfDoor.x, positionOfDoor.y, 0);

        doorVisual.name = "Door: " + door.type + " at " + positionOfDoor.x + "," + positionOfDoor.y;

        Color doorColor = doorTypeToColorLookup[door.type];
        doorVisual.GetComponent<SpriteRenderer>().color = doorColor;

        doorVisual.transform.parent = DungeonVisualRepresentationParent.transform;
    }

    public static void CreateVisualsFromModelData()
    {
        DungeonVisualRepresentationParent = new GameObject();
        DungeonVisualRepresentationParent.name = "DungeonVisualRepresentation";

        foreach (DungeonModelData dmd in ProceduralDungeonGenerator.GetDungeonModelData())
        {
            if (typeof(Room) == dmd.GetType())
            {
                Room r = (Room)dmd;
                CreateRoomVisual(r);
            }
            else if (typeof(Door) == dmd.GetType())
            {
                Door d = (Door)dmd;
                CreateDoorVisual(d);
            }
        }

        //position camera to center on dungeon
        //move camera back
    }

    public static void DestroyDungeonVisuals()
    {
        UnityEngine.GameObject.Destroy(DungeonVisualRepresentationParent);
    }

}
