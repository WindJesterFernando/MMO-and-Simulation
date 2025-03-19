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
    static Thread unityAndSimulationCommunicationThread;
    static float timeSinceLastBenchmark;
    static long numberOfNanoSecondsElapsedSinceLastUpdate = 0;
    const int GenerationsUntilBenchmarkCheck = 10000;

    const int NumberOfWorkerThreads = 4;
    static LinkedList<Thread> workerThreadPool;
    static int currentRowToBeDetermineIfAliveNextGen;
    static int numberOfRowsDeterminedToBeAliveNextGen;
    //https://stackoverflow.com/questions/420825/how-to-properly-lock-a-value-type

    static object rowToBeWorkedOnLock;
    static object rowsCompleteLock;

    static int currentRowToLoadAndClearBuffer;
    static int numberOfRowsWithBufferLoadedAndCleared;

    static int lastGenerationNumberUsedToBenchMark;

    static SimulationState simState = SimulationState.DetermineIfAliveNextGen;

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
    static public void StartSimulationThreads()
    {
        unityAndSimulationCommunicationThread = new Thread(new ThreadStart(ProcessUnityAndSimulationCommunicationThread));
        unityAndSimulationCommunicationThread.Start();

        workerThreadPool = new LinkedList<Thread>();

        for (int i = 0; i < NumberOfWorkerThreads; i++)
        {
            Thread t = new Thread(new ThreadStart(ProcessWorkerThread));
            //t.Priority = System.Threading.ThreadPriority.Highest;
            //t.Priority = System.Threading.ThreadPriority.Lowest;
            workerThreadPool.AddLast(t);
        }

        foreach (Thread t in workerThreadPool)
        {
            t.Start();
        }
    }
    static public void ProcessUnityAndSimulationCommunicationThread()
    {
        //numberOfNanoSecondsElapsedSinceLastUpdate = Environment.TickCount;
        numberOfNanoSecondsElapsedSinceLastUpdate = Environment.TickCount;

        //for (int i = 0; i < 100; i++)
        while (true)
        {
            #region DeltaTime

            long numberOfNanoSecondsElapsed = Environment.TickCount;
            long difInNanoSecs = (numberOfNanoSecondsElapsed - numberOfNanoSecondsElapsedSinceLastUpdate);
            float deltaTime = (float)difInNanoSecs / 1000f;
            numberOfNanoSecondsElapsedSinceLastUpdate = numberOfNanoSecondsElapsed;

            #endregion

            if (displayAndApplicationManager.IsThreadPaused())
            {
                continue;
            }

            timeSinceLastBenchmark += deltaTime;

            #region Benchmark Check

            if (generationNumber % GenerationsUntilBenchmarkCheck == 0
                && lastGenerationNumberUsedToBenchMark != generationNumber)
            {
                Debug.Log("Benchmark #" + generationNumber / GenerationsUntilBenchmarkCheck + ", time taken == " + timeSinceLastBenchmark);
                timeSinceLastBenchmark = 0;
                lastGenerationNumberUsedToBenchMark = generationNumber;
            }

            #endregion

            lock (rowsCompleteLock)
            {
                //Debug.Log(rowsComplete);

                if (simState == SimulationState.EnqueBufferOfModelDataForVisuals)
                //if (numberOfRowsWithBufferLoadedAndCleared >= GridSizeX)
                {
                    generationNumber++;

                    #region Process Generation On Model Data

                    lock (rowToBeWorkedOnLock)
                    {
                        if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
                        {
                            bool[,] toBuffer = CreateDeepCopyOfGrid(gridData);
                            displayAndApplicationManager.EnqueBufferOfModelDataForVisuals(toBuffer);
                        }

                        // currentRowToBeDetermineIfAliveNextGen = 0;
                        // currentRowToLoadAndClearBuffer = 0;
                    }

                    // numberOfRowsDeterminedToBeAliveNextGen = 0;
                    // numberOfRowsWithBufferLoadedAndCleared = 0;

                    //ChangeSimulationState(SimulationState.DetermineIfAliveNextGen);

                    #endregion

                    // #region Benchmark Check

                    // if (generationNumber % GenerationsUntilBenchmarkCheck == 0)
                    // {
                    //     Debug.Log("Benchmark #" + generationNumber / GenerationsUntilBenchmarkCheck + ", time taken == " + timeSinceLastBenchmark);
                    //     timeSinceLastBenchmark = 0;
                    // }

                    // #endregion
                }
            }


            if (simState == SimulationState.DetermineIfAliveNextGen)
            {
                if (numberOfRowsDeterminedToBeAliveNextGen >= GridSizeX)
                    ChangeSimulationState(SimulationState.LoadAndClearBuffer);
            }
            else if (simState == SimulationState.LoadAndClearBuffer)
            {
                if (numberOfRowsWithBufferLoadedAndCleared >= GridSizeX)
                    ChangeSimulationState(SimulationState.EnqueBufferOfModelDataForVisuals);
            }
            else if (simState == SimulationState.EnqueBufferOfModelDataForVisuals)
            {
                //if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
                    ChangeSimulationState(SimulationState.DetermineIfAliveNextGen);
            }







            // 

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
        unityAndSimulationCommunicationThread.Abort();

        foreach (Thread t in workerThreadPool)
        {
            t.Abort();
        }
    }
    static public int GetGenerationNumber()
    {
        return generationNumber;
    }
    static public void ProcessWorkerThread()
    {
        while (true)
        {
            if (displayAndApplicationManager.IsThreadPaused())
            {
                continue;
            }

            #region DetermineIfCellsAreAliveNextGenerationOnRow

            int x;

            //if (numberOfRowsDeterminedToBeAliveNextGen < GridSizeX)
            if (simState == SimulationState.DetermineIfAliveNextGen)
            {
                lock (rowToBeWorkedOnLock)
                {
                    x = currentRowToBeDetermineIfAliveNextGen;
                    currentRowToBeDetermineIfAliveNextGen++;
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
                        numberOfRowsDeterminedToBeAliveNextGen++;

                        // if (numberOfRowsDeterminedToBeAliveNextGen >= GridSizeX)
                        //     ChangeSimulationState(SimulationState.LoadAndClearBuffer);

                    }
                }

            }

            #endregion

            #region LoadAndClearNextGenerationBuffer

            //if (numberOfRowsDeterminedToBeAliveNextGen >= GridSizeX)
            if (simState == SimulationState.LoadAndClearBuffer)
            {
                lock (rowToBeWorkedOnLock)
                {
                    x = currentRowToLoadAndClearBuffer;
                    currentRowToLoadAndClearBuffer++;
                }

                if (x < GridSizeX)
                {

                    for (int y = 0; y < GridSizeY; y++)
                    {
                        gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
                        gridData[x, y].isAliveNextGeneration = false;
                    }

                    lock (rowsCompleteLock)
                    {
                        numberOfRowsWithBufferLoadedAndCleared++;

                        // if (numberOfRowsWithBufferLoadedAndCleared >= GridSizeX)
                        //     ChangeSimulationState(SimulationState.EnqueBufferOfModelDataForVisuals);
                    }
                }

            }

            #endregion





            // lock (rowsCompleteLock)
            // {
            //     if (numberOfRowsWithBufferLoadedAndCleared >= GridSizeX)
            //     {

            //         generationNumber++;

            //         #region Process Generation On Model Data

            //         lock (rowToBeWorkedOnLock)
            //         {
            //             if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
            //             {
            //                 bool[,] toBuffer = CreateDeepCopyOfGrid(gridData);
            //                 displayAndApplicationManager.EnqueBufferOfModelDataForVisuals(toBuffer);
            //             }

            //             currentRowToBeDetermineIfAliveNextGen = 0;
            //             currentRowToLoadAndClearBuffer = 0;
            //         }

            //         numberOfRowsDeterminedToBeAliveNextGen = 0;
            //         numberOfRowsWithBufferLoadedAndCleared = 0;

            //         #endregion

            //     }
            // }

        }
    }

    static private void ChangeSimulationState(SimulationState state)
    {

        if (state == SimulationState.DetermineIfAliveNextGen)
        {
            currentRowToBeDetermineIfAliveNextGen = 0;
            numberOfRowsDeterminedToBeAliveNextGen = 0;
        }
        else if (state == SimulationState.LoadAndClearBuffer)
        {
            currentRowToLoadAndClearBuffer = 0;
            numberOfRowsWithBufferLoadedAndCleared = 0;
        }
        simState = state;

        Debug.Log("-------State == " + state + " -----------------");
        Debug.Log("currentRowToBeDetermineIfAliveNextGen == " + currentRowToBeDetermineIfAliveNextGen);
        Debug.Log("currentRowToLoadAndClearBuffer == " + currentRowToLoadAndClearBuffer);
        Debug.Log("numberOfRowsDeterminedToBeAliveNextGen == " + numberOfRowsDeterminedToBeAliveNextGen);
        Debug.Log("numberOfRowsWithBufferLoadedAndCleared == " + numberOfRowsWithBufferLoadedAndCleared);
    }

    enum SimulationState
    {
        DetermineIfAliveNextGen,
        LoadAndClearBuffer,
        EnqueBufferOfModelDataForVisuals,

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