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

    static int lastGenerationNumberUsedToBenchMark;

    static Queue<WorkToBeDone> workToBeDones;
    static Queue<WorkToBeDone> workToBeDones2;

    static DebugStuffs debugStuffs;



    //static bool pauseForEnqueueing;

    //static SimulationState simState = SimulationState.DetermineIfAliveNextGen;

    static public void Init(DisplayAndApplicationManager displayAndApplicationManager)
    {
        //Debug.Log("Environment.ProcessorCount == " + Environment.ProcessorCount);

        debugStuffs = new DebugStuffs();

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

        workToBeDones = new Queue<WorkToBeDone>();
        workToBeDones2 = new Queue<WorkToBeDone>();
        EnqueueWorkToBeDones();

        // while (workToBeDones.Count != 0)
        // {
        //     WorkToBeDone w = workToBeDones.Dequeue();
        //     Debug.Log(w.state + ", " + w.rowToWorkOn);
        // }

        //EnqueueWorkToBeDones();

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

            lock (workToBeDones)
            {
                lock (workToBeDones2)
                {
                    //Debug.Log("---------");

                    //Debug.Log(numberOfWorkerThreadsDoingWork);

                    if (workToBeDones.Count == 0 && workToBeDones2.Count == 0 && debugStuffs.threadsDoingWork == 0)
                    // && debugStuffs.DebugCountForDetermineIfAliveNextGen % 100 == 0
                    // && debugStuffs.DebugCountForLoadAndClearBuffer % 100 == 0)
                    {
                        // lock (debugStuffs)
                        //     Debug.Log("AliveNextGen = " + debugStuffs.DebugCountForDetermineIfAliveNextGen + "   Buffer = " + debugStuffs.DebugCountForLoadAndClearBuffer);

                        //if (simState == SimulationState.EnqueBufferOfModelDataForVisuals)
                        {
                            generationNumber++;

                            if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
                            {
                                bool[,] toBuffer = CreateDeepCopyOfGrid(gridData);
                                displayAndApplicationManager.EnqueBufferOfModelDataForVisuals(toBuffer);
                            }
                        }

                        EnqueueWorkToBeDones();
                    }
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

            WorkToBeDone workToBeDone;

            SimulationState simState;
            int x;

            lock (workToBeDones)
            {
                lock (workToBeDones2)
                {
                    bool useFirstQueue = true;
                    // https://i.imgur.com/y5qT1WV.png
                    if (workToBeDones.Count == 0)
                    {
                        // todo allow more than one thread to use second queue
                        if (debugStuffs.threadsDoingWork > 0)
                        {
                            continue;
                        }

                        useFirstQueue = false;
                    }

                    if (workToBeDones2.Count == 0)
                    {
                        continue;
                    }

                    lock (debugStuffs)
                        debugStuffs.threadsDoingWork++;

                    workToBeDone = (useFirstQueue ? workToBeDones : workToBeDones2).Dequeue();
                }
            }

            x = workToBeDone.rowToWorkOn;
            simState = workToBeDone.state;

            //Debug.Log(simState + ", " + x);

            #region DetermineIfCellsAreAliveNextGenerationOnRow

            if (simState == SimulationState.DetermineIfAliveNextGen)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAliveNextGeneration = DetermineIfCellIsAliveNextGeneration(x, y);
                }

                lock (debugStuffs)
                    debugStuffs.DebugCountForDetermineIfAliveNextGen++;
            }

            #endregion

            #region LoadAndClearNextGenerationBuffer

            if (simState == SimulationState.LoadAndClearBuffer)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
                    gridData[x, y].isAliveNextGeneration = false;
                }

                lock (debugStuffs)
                    debugStuffs.DebugCountForLoadAndClearBuffer++;
            }

            #endregion

            // lock (debugStuffs)
            //     Debug.Log("***AliveNextGen = " + debugStuffs.DebugCountForDetermineIfAliveNextGen + "   Buffer = " + debugStuffs.DebugCountForLoadAndClearBuffer);

            lock (debugStuffs)
                debugStuffs.threadsDoingWork--;

        }
    }

    static public void EnqueueWorkToBeDones()
    {
        Queue<WorkToBeDone> temp = new Queue<WorkToBeDone>();
        Queue<WorkToBeDone> temp2 = new Queue<WorkToBeDone>();

        for (int x = 0; x < GridSizeX; x++)
        {
            temp.Enqueue(new WorkToBeDone(SimulationState.DetermineIfAliveNextGen, x));
        }

        //temp.Enqueue(new WorkToBeDone(SimulationState.EnqueNextBatchOfDetermineIfAliveNextGen));

        for (int x = 0; x < GridSizeX; x++)
        {
            temp2.Enqueue(new WorkToBeDone(SimulationState.LoadAndClearBuffer, x));
        }

        //temp.Enqueue(new WorkToBeDone(SimulationState.EnqueNextBatchOfLoadAndClearBuffer));

        lock (workToBeDones)
        {
            lock (workToBeDones2)
            {
                workToBeDones = temp;
                workToBeDones2 = temp2;
            }
        }
    }


}

public class CellData
{
    public bool isAlive;
    public bool isAliveNextGeneration;
}

public enum SimulationState
{
    DetermineIfAliveNextGen,
    LoadAndClearBuffer,
    EnqueBufferOfModelDataForVisuals,

}

public struct WorkToBeDone
{
    public SimulationState state;
    public int rowToWorkOn;

    public WorkToBeDone(SimulationState state, int rowToWorkOn)
    {
        this.state = state;
        this.rowToWorkOn = rowToWorkOn;
    }
    public WorkToBeDone(SimulationState state)
    {
        this.state = state;
        this.rowToWorkOn = 0;
    }
}

public class DebugStuffs
{
    public int DebugCountForDetermineIfAliveNextGen;
    public int DebugCountForLoadAndClearBuffer;
    public int threadsDoingWork;
}

//
//
//
//
//Go look at answers online