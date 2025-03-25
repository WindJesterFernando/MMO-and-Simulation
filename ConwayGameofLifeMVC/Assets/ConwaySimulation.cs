using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

static public class ConwaySimulation
{
    static DisplayAndApplicationManager displayAndApplicationManager;
    static int generationNumber = 1;
    public const int GridSizeX = 150, GridSizeY = 100;
    static CellData[,] gridData;
    static Thread unityAndSimulationCommunicationThread;
    static float timeSinceLastBenchmark;
    static long numberOfNanoSecondsElapsedSinceLastUpdate = 0;
    const int GenerationsUntilBenchmarkCheck = 10000;

    const int NumberOfWorkerThreads = 4;
    static LinkedList<Thread> workerThreadPool;

    static int lastGenerationNumberUsedToBenchMark;

    static ConcurrentQueue<WorkToBeDone> workToBeDones;
    static ConcurrentQueue<WorkToBeDone> workToBeDones2;

    static int threadsDoingWork;

    static public void Init(DisplayAndApplicationManager displayAndApplicationManager)
    {

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

        workToBeDones = new ConcurrentQueue<WorkToBeDone>();
        workToBeDones2 = new ConcurrentQueue<WorkToBeDone>();
        
        workToBeDones = MakeQueueForDetermineIfAliveNextGenWork();

    }
    static public void StartSimulationThreads()
    {
        unityAndSimulationCommunicationThread = new Thread(new ThreadStart(ProcessUnityAndSimulationCommunicationThread));
        unityAndSimulationCommunicationThread.Start();

        workerThreadPool = new LinkedList<Thread>();

        for (int i = 0; i < NumberOfWorkerThreads; i++)
        {
            Thread t = new Thread(new ThreadStart(ProcessWorkerThread));
            workerThreadPool.AddLast(t);
        }

        foreach (Thread t in workerThreadPool)
        {
            t.Start();
        }
    }
    static public void ProcessUnityAndSimulationCommunicationThread()
    {
        numberOfNanoSecondsElapsedSinceLastUpdate = Environment.TickCount;

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

            WorkerThreadJobType jobType;
            int x;
            
            Interlocked.Increment(ref threadsDoingWork);

            if (!workToBeDones.TryDequeue(out workToBeDone))
            {
                Interlocked.Decrement(ref threadsDoingWork);
                continue;
            }

            x = workToBeDone.rowToWorkOn;
            jobType = workToBeDone.jobType;

            if (jobType == WorkerThreadJobType.DetermineIfAliveNextGen)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAliveNextGeneration = DetermineIfCellIsAliveNextGeneration(x, y);
                }
            }
            else if (jobType == WorkerThreadJobType.LoadAndClearBuffer)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
                    gridData[x, y].isAliveNextGeneration = false;
                }
            }
            else if (jobType == WorkerThreadJobType.EnqueueLoadAndClearBufferWork)
            {
                ConcurrentQueue<WorkToBeDone> temp = MakeQueueForLoadAndClearBufferWork();

                while (threadsDoingWork > 1)
                    ;

                workToBeDones = temp;

            }
            else if (jobType == WorkerThreadJobType.SetupForNextGeneration)
            {
                ConcurrentQueue<WorkToBeDone> temp = MakeQueueForDetermineIfAliveNextGenWork();

                while (threadsDoingWork > 1)
                    ;

                generationNumber++;

                if (displayAndApplicationManager.IsBufferQueueOfModelDataForVisualsEmpty())
                {
                    bool[,] toBuffer = CreateDeepCopyOfGrid(gridData);
                    displayAndApplicationManager.EnqueueBufferOfModelDataForVisuals(toBuffer);
                }

                workToBeDones = temp;
            }

            Interlocked.Decrement(ref threadsDoingWork);

        }
    }

    static public ConcurrentQueue<WorkToBeDone> MakeQueueForDetermineIfAliveNextGenWork()
    {
        ConcurrentQueue<WorkToBeDone> temp = new ConcurrentQueue<WorkToBeDone>();

        for (int x = 0; x < GridSizeX; x++)
        {
            temp.Enqueue(new WorkToBeDone(WorkerThreadJobType.DetermineIfAliveNextGen, x));
        }

        temp.Enqueue(new WorkToBeDone(WorkerThreadJobType.EnqueueLoadAndClearBufferWork));

        return temp;
    }

    static public ConcurrentQueue<WorkToBeDone> MakeQueueForLoadAndClearBufferWork()
    {
        ConcurrentQueue<WorkToBeDone> temp = new ConcurrentQueue<WorkToBeDone>();

        for (int x = 0; x < GridSizeX; x++)
        {
            temp.Enqueue(new WorkToBeDone(WorkerThreadJobType.LoadAndClearBuffer, x));
        }

        temp.Enqueue(new WorkToBeDone(WorkerThreadJobType.SetupForNextGeneration));

        return temp;
    }

}

public class CellData
{
    public bool isAlive;
    public bool isAliveNextGeneration;
}

public enum WorkerThreadJobType
{
    DetermineIfAliveNextGen,
    LoadAndClearBuffer,
    SetupForNextGeneration,

    EnqueueLoadAndClearBufferWork,

}

public struct WorkToBeDone
{
    public WorkerThreadJobType jobType;
    public int rowToWorkOn;

    public WorkToBeDone(WorkerThreadJobType jobType, int rowToWorkOn)
    {
        this.jobType = jobType;
        this.rowToWorkOn = rowToWorkOn;
    }
    public WorkToBeDone(WorkerThreadJobType jobType)
    {
        this.jobType = jobType;
        this.rowToWorkOn = 0;
    }
}

