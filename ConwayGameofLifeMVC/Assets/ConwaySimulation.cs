using UnityEngine;
using TMPro;
using System.Threading;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

static public class ConwaySimulation
{
    static DisplayAndApplicationManager displayAndApplicationManager;
    static int generationNumber = 1;
    public const int GridSizeX = 100, GridSizeY = 100;
    static CellData[,] gridData;
    static float timeSinceLastBenchmark;
    const int GenerationsUntilBenchmarkCheck = 100000;
    static Thread simulationThread;

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

        // for (int i = 0; i < GridSizeX; i++)
        // {
        //     gridData[i, 15].isAlive = true;
        // }

        #endregion
    }
    static public void StartSimulationThread()
    {
        simulationThread = new Thread(new ThreadStart(ProcessSimThread));
        simulationThread.Start();
    }
    static public void ProcessSimThread()
    {
        while (true)
        {
            if (displayAndApplicationManager.IsThreadPaused())
            {
                Thread.Sleep(10);
                continue;
            }

            generationNumber++;

            #region Process Generation On Model Data

            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeY; y++)
                {
                    gridData[x, y].isAliveNextGeneration = DetermineIfCellIsAliveNextGeneration(x, y);
                }
            }

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
    static public void AbortThread()
    {
        simulationThread.Abort();
    }
    static public int GetGenerationNumber()
    {
        return generationNumber;
    }
    static public void AddTimeToBenchmark(float deltaTime)
    {
        timeSinceLastBenchmark += deltaTime;
    }

}

public class CellData
{
    public bool isAlive;
    public bool isAliveNextGeneration;
}
