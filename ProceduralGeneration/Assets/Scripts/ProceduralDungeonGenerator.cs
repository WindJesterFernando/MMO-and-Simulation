
using System;
using System.Collections.Generic;
using System.Diagnostics;

public static partial class ProceduralDungeonGenerator
{
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public static Direction GetRandomDirection()
    {
        double randomDirection = GetRandomValueFromZeroToOne();

        if (randomDirection > 0.75f)
            return Direction.Up;
        else if (randomDirection > 0.5f)
            return Direction.Down;
        else if (randomDirection > 0.25f)
            return Direction.Left;
        else
            return Direction.Right;

    }

    public static bool DoesRoomExistAtCoordinate(Coordinate coordinate)
    {
        foreach (Room dr in GetDungeonRooms())
        {
            if (dr.coordinate.x == coordinate.x && dr.coordinate.y == coordinate.y)
            {
                return true;
            }
        }

        return false;
    }

    public static Coordinate GetCoordinateInRandomDirection(Coordinate coordinateToCenterOn)
    {
        Direction randomDirection = GetRandomDirection();

        Coordinate coordinateToTryAndCreateRoomAt;

        if (randomDirection == Direction.Up)
        {
            coordinateToTryAndCreateRoomAt = new Coordinate(coordinateToCenterOn.x + 0, coordinateToCenterOn.y + 1);
        }
        else if (randomDirection == Direction.Down)
        {
            coordinateToTryAndCreateRoomAt = new Coordinate(coordinateToCenterOn.x + 0, coordinateToCenterOn.y - 1);
        }
        else if (randomDirection == Direction.Right)
        {
            coordinateToTryAndCreateRoomAt = new Coordinate(coordinateToCenterOn.x + 1, coordinateToCenterOn.y + 0);
        }
        else
        {
            coordinateToTryAndCreateRoomAt = new Coordinate(coordinateToCenterOn.x - 1, coordinateToCenterOn.y + 0);
        }

        return coordinateToTryAndCreateRoomAt;
    }

    public static Room AttemptToCreateRoomInRandomDirection(Coordinate coordinateToCenterOn)
    {
        Coordinate coordinateToTryAndCreateRoomAt = GetCoordinateInRandomDirection(coordinateToCenterOn);

        if (!DoesRoomExistAtCoordinate(coordinateToTryAndCreateRoomAt))
        {
            Room newRoom = AddRoom(RoomType.Normal, coordinateToTryAndCreateRoomAt);
            return newRoom;
        }

        return null;
    }

    public static Room AttemptToCreateRoomInRandomDirectionWithNeighbourConstraints(Coordinate coordinateToCenterOn)
    {
        Coordinate coordinateToTryAndCreateRoomAt = GetCoordinateInRandomDirection(coordinateToCenterOn);

        if (!DoesRoomExistAtCoordinate(coordinateToTryAndCreateRoomAt))
        {
            List<Room> neighbours = GetNeighbours(coordinateToTryAndCreateRoomAt);

            if (neighbours.Count < 2)
            {
                Room newRoom = AddRoom(RoomType.Normal, coordinateToTryAndCreateRoomAt);
                return newRoom;
            }
        }

        return null;
    }

    public static List<Room> GetNeighbours(Coordinate coordOfRoomToCenterOn)
    {
        List<Room> neighbours = new List<Room>();
        foreach (Room r in GetDungeonRooms())
        {
            if (r.coordinate.x == coordOfRoomToCenterOn.x && r.coordinate.y == coordOfRoomToCenterOn.y)
                continue;

            bool isNeighbourOnX =
                Math.Abs(coordOfRoomToCenterOn.x - r.coordinate.x) < 2
                && coordOfRoomToCenterOn.y == r.coordinate.y;

            bool isNeighbourOnY =
                Math.Abs(coordOfRoomToCenterOn.y - r.coordinate.y) < 2
                && coordOfRoomToCenterOn.x == r.coordinate.x;

            if (isNeighbourOnX || isNeighbourOnY)
                neighbours.Add(r);
        }

        return neighbours;
    }


    public static Room GetRandomRoom(LinkedList<Room> rooms)
    {
        int indexOfRoomToUse = (int)(GetRandomValueFromZeroToOne() * (double)rooms.Count);
        int i = 0;
        foreach (Room r in rooms)
        {
            if (i == indexOfRoomToUse)
            {
                return r;
            }
            i++;
        }

        return null;
    }

    public static void ProcedurallyGenerateDungeon()
    {
        LinkedList<Room> roomsOpenToExpansion = new LinkedList<Room>();
        Room r;

        Coordinate startCoord = new Coordinate(0, 0);
        r = AddRoom(RoomType.Start, startCoord);
        roomsOpenToExpansion.AddLast(r);

        while (GetDungeonRooms().Count < 3)
        {
            r = AttemptToCreateRoomInRandomDirection(startCoord);
            if (r != null)
                roomsOpenToExpansion.AddLast(r);
        }

        int catchInfitity = 9000;

        while (GetDungeonRooms().Count < 7)
        {
            #region Inifite Loop Protection

            catchInfitity--;
            if (catchInfitity == 0)
            {
                UnityEngine.Debug.Log("-----Caught Infinity-----");
                break;
            }

            #endregion

            Coordinate coordOfRoomToCenterOn = GetRandomRoom(roomsOpenToExpansion).coordinate;//GetDungeonRooms().Last.Value.coordinate;//

            Room newRoom;

            if (Roll(85))
                newRoom = AttemptToCreateRoomInRandomDirectionWithNeighbourConstraints(coordOfRoomToCenterOn);
            else
                newRoom = AttemptToCreateRoomInRandomDirection(coordOfRoomToCenterOn);

            if (newRoom == null)
                continue;

            // List<Room> neighboursOfNewRoom = GetNeighbours(newRoom.coordinate);
            // if (neighboursOfNewRoom.Count > 2)
            //     continue;
            // if (neighboursOfNewRoom.Count > 1 && Roll(50))
            //     continue;

            roomsOpenToExpansion.AddLast(GetDungeonRooms().Last.Value);
        }

        MakeBossRoom();

    }

    static private void MakeBossRoom()
    {
        double hyp = -1;
        Room furthestRoom = null;
        foreach (Room r in GetDungeonRooms())
        {
            double x = (double)r.coordinate.x;
            double y = (double)r.coordinate.y;
            double newHyp = Math.Sqrt(x * x + y * y);

            if (newHyp > hyp)
            {
                furthestRoom = r;
                hyp = newHyp;
            }
        }

        Coordinate bossRoomCoord;
        bossRoomCoord = new Coordinate(furthestRoom.coordinate);

        if (Math.Abs(bossRoomCoord.x) > Math.Abs(bossRoomCoord.y))
        {
            if (bossRoomCoord.x > 0)
                bossRoomCoord.x++;
            else
                bossRoomCoord.x--;
        }
        else
        {
            if (bossRoomCoord.y > 0)
                bossRoomCoord.y++;
            else
                bossRoomCoord.y--;
        }


        AddRoom(RoomType.Boss, bossRoomCoord); //>:)
        //furthestRoom.type = RoomType.Boss;

        // Coordinate bossRoomCoord = null;
        // double bossRoomHyp = -1;
        // bool hasOnlyOneNeighbour = false;




        // while (true)
        // {
        // bossRoomCoord = GetCoordinateInRandomDirection(furthestRoom.coordinate);
        // double x = (double)bossRoomCoord.x;
        // double y = (double)bossRoomCoord.y;
        // bossRoomHyp = Math.Sqrt(x * x + y * y);

        // hasOnlyOneNeighbour = (GetNeighbours(bossRoomCoord).Count == 1);
        // if (bossRoomHyp < hyp)
        // {
        //     if (hasOnlyOneNeighbour)
        //         break;
        // }
        // break;
        // }

        //AddRoom(RoomType.Boss, bossRoomCoord);


    }





    //fix infinite loop
    //
    //
    ///
    ///



    // Procedural Generation:
    // Starting room is always at the center
    // Starting room must have 2 or 3 neighbours
    // We want 20 rooms
    // All rooms must be connected
    // Rooms must not snake out too much
    // Rooms must “tree” out not “bush” in. Rooms should be spread out.








    //function check if room exists
    //enum for direction
    //function that returns a random direction

    //"we are always progressign from the center, 
    //rather than moving to the most recent generated room"

    //associate direction with coord (dictionary)
    //use switch statement




    // for(int i = 0; i < 20; i++)
    // {
    //     if (GetDungeonRooms().Any())
    //     {
    //         Coordinate lastRoomCoord = GetDungeonRooms().Last.Value.coordinate;

    //         AttemptToCreateRoomInRandomDirection(lastRoomCoord);
    //     }
    // }







    // AddRoom(RoomType.Normal, new Coordinate(1, 0));

    // AddRoom(RoomType.Normal, new Coordinate(-1, 0));

    // //pick a random direction and put the room there


    // if (Roll(50))
    // {
    //     // AddRoom(RoomType.Normal, new Coordinate(0, -1));

    //     randomDirection = GetRandomValueFromZeroToOne();

    //     if (randomDirection > 0.75f)
    //         AddRoom(RoomType.Normal, new Coordinate(0, 1));
    //     else if (randomDirection > 0.5f)
    //         AddRoom(RoomType.Normal, new Coordinate(0, -1));
    //     else if (randomDirection > 0.25f)
    //         AddRoom(RoomType.Normal, new Coordinate(1, 0));
    //     else
    //         AddRoom(RoomType.Normal, new Coordinate(-1, 0));
    // }




    // UnityEngine.Debug.Log(GetRandomValueFromZeroToOne());


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





// Procedural Generation:
// Starting room is always at the center
// Starting room must have 2 or 3 doors
// We want 20 rooms
// All rooms must be connected
// Rooms must “tree” out not “bush” in. Rooms should be spread out.



/// 
// Secret doors must not interfere with main path, they must be off to the side
// One Shop room
// Shop room must be locked & only have one door accessing it
// One Boss room
// Boss room must not be next to the start, must be a certain distance
// Boss room must only be connected to one other room
/// Add doors!





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


















// static public bool DoesTileExist(Coordinate coordinate)
// {
//     foreach (Room r in GetDungeonRooms())
//     {
//         if (r.coordinate.x == coordinate.x && r.coordinate.y == coordinate.y)
//             return true;
//     }

//     return false;

//     // bool tileExists = false;
//     // foreach (Room r in GetDungeonRooms())
//     // {
//     //     if (r.coordinate.x == coordinate.x && r.coordinate.y == coordinate.y)
//     //         tileExists = true;
//     // }
//     // return tileExists;
// }

// public static void ProcedurallyGenerateDungeon()
// {
//     Room startRoom = AddRoom(RoomType.Start, new Coordinate(0, 0));

//     Coordinate coord = new Coordinate(startRoom.coordinate);
//     coord.x += 1;
//     coord.y += 0;

//     if (DoesTileExist(coord))
//         UnityEngine.Debug.Log("1 Tile Does Exist");

//     AddRoom(RoomType.Normal, new Coordinate(1, 0));

//     if (DoesTileExist(coord))
//         UnityEngine.Debug.Log("2 Tile Does Exist");



//CREATE A NEW Room (Coordinate coordinate, )
//


//check if valid
//create room

// if (DoesTileExist(coord))
//     Room secondRoom = AddRoom(RoomType.Normal, new Coordinate(2, 0));





// AddRoom(RoomType.Normal, new Coordinate(1, 0));
// AddRoom(RoomType.Normal, new Coordinate(-1, 0));

// if (Roll(50))
//     AddRoom(RoomType.Normal, new Coordinate(0, -1));

// for (int i = 0; i < 100; i++)
//     UnityEngine.Debug.Log(GetRandomValueFromZeroToOne());



//how about finding all the tiles that would be adjacent to a room 
//and then creating a room in a random one of those tiles
//, then doing that 20 times





// AddRoom(RoomType.Start, new Coordinate(0, 0));


// AddRoom(RoomType.Normal, new Coordinate(1, 0));
// AddRoom(RoomType.Normal, new Coordinate(-1, 0));

// if (Roll(50))
// {
//     AddRoom(RoomType.Normal, new Coordinate(0, -1));
// }


// // for (int i = 0; i < 100; i++)
// //     UnityEngine.Debug.Log((int) (GetRandomValueFromZeroToOne() * 2));


// // for (int i = 0; i < 100; i++)
// // {
// //     int newX = (int)(GetRandomValueFromZeroToOne() * 2);
// //     int newY = (int)(GetRandomValueFromZeroToOne() * 2);

// //     Coordinate coord = new Coordinate(newX, newY);

// //     UnityEngine.Debug.Log(coord.x + "," + coord.y);
// // }

// //



// int newX = (int)(GetRandomValueFromZeroToOne() * 2);
// int newY = (int)(GetRandomValueFromZeroToOne() * 2);

// Coordinate coord = new Coordinate(newX, newY);

// UnityEngine.Debug.Log(coord.x + "," + coord.y);


// bool roomExists = false;

// foreach (Room r in GetDungeonRooms())
// {
//     if (r.coordinate.x == coord.x && r.coordinate.y == coord.y)
//         roomExists = true;
// }

// if(roomExists)
//     UnityEngine.Debug.Log("room does exist at: " + coord.x + "," + coord.y);
// else
//     UnityEngine.Debug.Log("room does not exist at: " + coord.x + "," + coord.y);


//GetRandomValueFromZeroToOne()

//else add room at those coordinates

// for (int i = 0; i < 100; i++)
//     UnityEngine.Debug.Log();


// "find one of two of empty space", "adjacency information", "check if a room already has those coords"
///randomize the Coordinate of the rooms being added
//
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
//}

//}


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


