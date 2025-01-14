
using System.Collections.Generic;
using System.Diagnostics;

public static partial class ProceduralDungeonGenerator
{
    public static void ProcedurallyGenerateDungeon()
    {

        AddRoom(RoomType.Start, new Coordinate(0, 0));


        AddRoom(RoomType.Normal, new Coordinate(1, 0));
        AddRoom(RoomType.Normal, new Coordinate(-1, 0));

        if (Roll(50))
        {
            AddRoom(RoomType.Normal, new Coordinate(0, -1));
        }



        ///randomize the Coordinate of the rooms being added
        /// randomize Room type 
        /// 




        //figure out how to randomly decide a direction for doors
        //


        // for (int i = 0; i < 100; i++)
        // {
        //     if (Roll(10))
        //         UnityEngine.Debug.Log("Roll with 10% chance: True");
        //     else
        //         UnityEngine.Debug.Log("Roll with 10% chance: False");
        // }

        // Room startRoom0x0 = AddRoom(RoomType.Start, new Coordinate(0, 0));
        // Room room1x0 = AddRoom(RoomType.Normal, new Coordinate(1, 0));
        // Room room_1x0 = AddRoom(RoomType.Normal, new Coordinate(-1, 0));
        // Room room0x_1 = AddRoom(RoomType.Normal, new Coordinate(0, -1));
        // Room shopRoom_2x0 = AddRoom(RoomType.Shop, new Coordinate(-2, 0));
        // Room room1x_1 = AddRoom(RoomType.Trap, new Coordinate(1, -1));
        // Room room0x_2 = AddRoom(RoomType.Normal, new Coordinate(0, -2));
        // Room room0x_3 = AddRoom(RoomType.Trap, new Coordinate(0, -3));
        // Room treasureRoom1x_3 = AddRoom(RoomType.Treasure, new Coordinate(1, -3));
        // Room secretRoom1x_2 = AddRoom(RoomType.Secret, new Coordinate(1, -2));
        // Room bossRoom_1x_3 = AddRoom(RoomType.Boss, new Coordinate(-1, -3));
        // Room superSecretRoom0x_4 = AddRoom(RoomType.SuperSecret, new Coordinate(0, -4));

        // AddDoor(DoorType.Open, startRoom0x0, room1x0);
        // AddDoor(DoorType.Open, startRoom0x0, room_1x0);
        // AddDoor(DoorType.Open, startRoom0x0, room0x_1);
        // AddDoor(DoorType.Locked, room_1x0, shopRoom_2x0);
        // AddDoor(DoorType.Open, room1x0, room1x_1);
        // AddDoor(DoorType.Open, room0x_1, room1x_1);
        // AddDoor(DoorType.Open, room0x_1, room0x_2);
        // AddDoor(DoorType.Open, room0x_2, room0x_3);
        // AddDoor(DoorType.Locked, room0x_3, treasureRoom1x_3);
        // AddDoor(DoorType.Bombable, room0x_2, secretRoom1x_2);
        // AddDoor(DoorType.Bombable, room1x_1, secretRoom1x_2);
        // AddDoor(DoorType.Bombable, treasureRoom1x_3, secretRoom1x_2);
        // AddDoor(DoorType.Open, room0x_3, bossRoom_1x_3);
        // AddDoor(DoorType.Bombable, room0x_3, superSecretRoom0x_4);

        // UnityEngine.Debug.Log("There are " + GetDungeonRooms().Count + " rooms in this dungeon.");
        // UnityEngine.Debug.Log("There are " + GetDungeonDoors().Count + " doors in this dungeon.");

        // foreach(Room r in GetDungeonRooms()) { }
        // foreach(Door d in GetDungeonDoors()) { }
    }

}


// Procedural Generation:
// Starting room is always at the center
// Starting room must have 2 or 3 doors
// We want 20 rooms
// All rooms must be connected
// Rooms must “tree” out not “bush” in. Rooms should be spread out.
// Secret doors must not interfere with main path, they must be off to the side
// One Shop room
// Shop room must be locked & only have one door accessing it
// One Boss room
// Boss room must not be next to the start, must be a certain distance
// Boss room must only be connected to one other room

