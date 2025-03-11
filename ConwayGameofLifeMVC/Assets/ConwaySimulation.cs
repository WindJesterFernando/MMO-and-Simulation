using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

static public class ConwaySimulation
{
    static DisplayAndApplicationManager displayAndApplicationManager;
    static int generationNumber = 1;
    public const int GridSizeX = 100, GridSizeY = 100;
    static CellData[,] gridData;
    static Thread simulationThread;//rename
    static float timeSinceLastBenchmark;
    static long numberOfNanoSecondsElapsedSinceLastUpdate = 0;
    const int GenerationsUntilBenchmarkCheck = 10000;

    const int NumberOfWorkerThreads = 8;
    static LinkedList<Thread> workerThreadPool;
    static int rowToBeDetermineIfAlive;
    static int rowsDeterminedComplete;
    //https://stackoverflow.com/questions/420825/how-to-properly-lock-a-value-type

    static object rowToBeWorkedOnLock;
    static object rowsCompleteLock;
    

    static public void Init(DisplayAndApplicationManager displayAndApplicationManager)
    {
        //Debug.Log("Environment.ProcessorCount == " + Environment.ProcessorCount);
        rowToBeWorkedOnLock = new object();
        rowsCompleteLock = new object();

        ConwaySimulation.displayAndApplicationManager = displayAndApplicationManager;

        #region Instantiate Grid Model Data

        gridData = new CellData[GridSizeX, GridSizeY];

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                gridData[x, y] = new CellData();
            }
        }

        #endregion

        #region Hardcoded Live Cells, For Testing

        gridData[4, 4].isAlive = true;
        gridData[4, 5].isAlive = true;
        gridData[4, 6].isAlive = true;

        gridData[10, 5].isAlive = true;
        gridData[10, 5].isAlive = true;
        gridData[11, 6].isAlive = true;
        gridData[11, 6].isAlive = true;
        gridData[11, 7].isAlive = true;

        //Correct pattern
        gridData[1, 19].isAlive = true;
        gridData[2, 19].isAlive = true;
        gridData[3, 19].isAlive = true;
        gridData[4, 19].isAlive = true;

        //Top edge case test
        gridData[18, 1].isAlive = true;
        gridData[18, 2].isAlive = true;
        gridData[18, 3].isAlive = true;
        gridData[18, 4].isAlive = true;

        // gridData[4, 4].isAlive = true;
        // gridData[3, 4].isAlive = true;

        for (int i = 0; i < GridSizeX; i++)
        {
            gridData[i, 15].isAlive = true;
        }

        for (int i = 0; i < GridSizeY; i++)
        {
            gridData[15, i].isAlive = true;
        }


        for (int i = 0; i < GridSizeX; i++)
        {
            gridData[i, GridSizeY - 5].isAlive = true;
        }

        for (int i = 0; i < GridSizeY; i++)
        {
            gridData[GridSizeX - 5, i].isAlive = true;
        }

        #endregion
    }
    static public void StartSimulationThread()
    {
        simulationThread = new Thread(new ThreadStart(ProcessSimThread));
        simulationThread.Start();

        workerThreadPool = new LinkedList<Thread>();

        for (int i = 0; i < NumberOfWorkerThreads; i++)
        {
            Thread t = new Thread(new ThreadStart(DetermineIfCellsAreAliveNextGenerationOnRow));
            workerThreadPool.AddLast(t);
        }

        foreach (Thread t in workerThreadPool)
        {
            t.Start();
        }
    }
    static public void ProcessSimThread()
    {
        numberOfNanoSecondsElapsedSinceLastUpdate = Environment.TickCount;
        numberOfNanoSecondsElapsedSinceLastUpdate = Environment.TickCount;

        //for (int i = 0; i < 100; i++)
        while (true)
        {
            if (displayAndApplicationManager.IsThreadPaused())
            {
                continue;
            }

            #region DeltaTime

            long numberOfNanoSecondsElapsed = Environment.TickCount;
            long difInNanoSecs = (numberOfNanoSecondsElapsed - numberOfNanoSecondsElapsedSinceLastUpdate);
            float deltaTime = (float)difInNanoSecs / 1000f;
            numberOfNanoSecondsElapsedSinceLastUpdate = numberOfNanoSecondsElapsed;
            timeSinceLastBenchmark += deltaTime;

            #endregion


            lock (rowsCompleteLock)
            {
                //Debug.Log(rowsComplete);

                if (rowsDeterminedComplete >= GridSizeX)
                {
                    generationNumber++;

                    #region Process Generation On Model Data

                    lock (rowToBeWorkedOnLock)
                    {

                        for (int x = 0; x < GridSizeX; x++)
                        {
                            for (int y = 0; y < GridSizeY; y++)
                            {
                                gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
                                gridData[x, y].isAliveNextGeneration = false;
                            }
                        }

                        if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
                        {
                            bool[,] toBuffer = CreateDeepCopyOfGrid(gridData);
                            displayAndApplicationManager.EnqueBufferOfModelDataForVisuals(toBuffer);
                        }

                        rowToBeDetermineIfAlive = 0;
                    }


                    rowsDeterminedComplete = 0;

                    #endregion

                    #region Benchmark Check

                    if (generationNumber % GenerationsUntilBenchmarkCheck == 0)
                    {
                        Debug.Log("Benchmark #" + generationNumber / GenerationsUntilBenchmarkCheck + ", time taken == " + timeSinceLastBenchmark);
                        timeSinceLastBenchmark = 0;
                    }

                    #endregion
                }
            }
        }

    }
    static public bool DetermineIfCellIsAliveNextGeneration(int x, int y)
    {
        int liveNeighbourCount = 0;

        #region CountNeighbours

        // Left
        if (x > 0)
        {
            if (gridData[x - 1, y].isAlive)
                liveNeighbourCount++;
        }

        // Right
        if (x < GridSizeX - 1)
        {
            if (gridData[x + 1, y].isAlive)
                liveNeighbourCount++;
        }

        // Top
        if (y < GridSizeY - 1)
        {
            if (gridData[x, y + 1].isAlive)
                liveNeighbourCount++;
        }

        // Bottom
        if (y > 0)
        {
            if (gridData[x, y - 1].isAlive)
                liveNeighbourCount++;
        }

        // Left Top
        if (x > 0 && y < GridSizeY - 1)
        {
            if (gridData[x - 1, y + 1].isAlive)
                liveNeighbourCount++;
        }

        // Left Bottom
        if (x > 0 && y > 0)
        {
            if (gridData[x - 1, y - 1].isAlive)
                liveNeighbourCount++;
        }

        // Right Top
        if (x < GridSizeX - 1 && y < GridSizeY - 1)
        {
            if (gridData[x + 1, y + 1].isAlive)
                liveNeighbourCount++;
        }

        // Right Bottom
        if (x < GridSizeX - 1 && y > 0)
        {
            if (gridData[x + 1, y - 1].isAlive)
                liveNeighbourCount++;
        }

        #endregion

        #region Check If Cell Is Alive Next Generation

        bool cellIsAlive = gridData[x, y].isAlive;

        // Any live cell with fewer than two live neighbours dies, as if by underpopulation.
        // Any live cell with two or three live neighbours lives on to the next generation.
        // Any live cell with more than three live neighbours dies, as if by overpopulation.
        // Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.

        if (cellIsAlive && liveNeighbourCount < 2)
            return false;
        else if (cellIsAlive && (liveNeighbourCount == 2 || liveNeighbourCount == 3))
            return true;
        else if (cellIsAlive && liveNeighbourCount > 3)
            return false;
        else if (!cellIsAlive && liveNeighbourCount == 3)
            return true;

        #endregion

        return false;
    }
    static private bool[,] CreateDeepCopyOfGrid(CellData[,] toCopy)
    {
        bool[,] newCopy = new bool[GridSizeX, GridSizeY];

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeY; y++)
            {
                newCopy[x, y] = toCopy[x, y].isAlive;
            }
        }

        return newCopy;
    }
    static public void AbortThreads()
    {
        simulationThread.Abort();

        foreach (Thread t in workerThreadPool)
        {
            t.Abort();
        }
    }
    static public int GetGenerationNumber()
    {
        return generationNumber;
    }
    static public void DetermineIfCellsAreAliveNextGenerationOnRow()
    {
        while (true)
        {
            int x;

            lock (rowToBeWorkedOnLock)
            {
                x = rowToBeDetermineIfAlive;
                rowToBeDetermineIfAlive++;
            }

            if (x < GridSizeX)
            {
                //Debug.Log("X == " + x);
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAliveNextGeneration = DetermineIfCellIsAliveNextGeneration(x, y);
                }

                lock (rowsCompleteLock)
                {
                    rowsDeterminedComplete++;
                }
            }




//if appropriate, advance buffer

            // lock (rowToBeWorkedOnLock)
            // {
            //     x = rowToBeDetermineIfAlive;
            //     rowToBeDetermineIfAlive++;
            // }

            // for (int y = 0; y < GridSizeY; y++)
            // {
            //     gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
            //     gridData[x, y].isAliveNextGeneration = false;
            // }



        }
    }


    // static private void FlipMainSimulationBool()
    // {
    //     if (topLeftCompleted && bottomLeftCompleted && topRightCompleted && bottomRightCompleted)
    //     {
    //         //while (isFlippingBools) { }

    //         isFlippingBools = true;
    //         mainSimulationThreadIsProcessing = true;
    //         isFlippingBools = false;

    //         //Debug.Log("Flipping over to main sim thread");
    //     }
    // }

}

public class CellData
{
    public bool isAlive;
    public bool isAliveNextGeneration;
}

//
//
//
//




//"Worker" threads: takes a chunk of the conways grid and does all the checks.
//	Maybe doesn't directly affect model data. Takes a copy of the section that it needs, and deposits into a queue when done?
//"Checker" thread: AFTER the worker threads, checks all intersections. probably uses a lock
//	Could take specefied generation from all queues to do intersection checks, then pushes to the queue read by visuals