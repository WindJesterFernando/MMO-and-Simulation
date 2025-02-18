using UnityEngine;
using TMPro;
using System.Threading;

public class ConwaySimulationManager : MonoBehaviour
{
    int generationNumber = 1;
    const float TimeToWaitForNextGeneration = 0;//2.5f;
    float elapsedTimeSinceLastGeneration = 0;
    const int GridSizeX = 20, GridSizeY = 20;
    CellData[,] gridData;

    [SerializeField] TMP_Text generationNumberText;
    GameObject[,] gridVisuals;

    float timeSinceLastBenchmark;

    Thread sampleThread;

    void Start()
    {
        sampleThread = new Thread(new ThreadStart(ThreadProcess));

        sampleThread.Start();

        #region Instantiate Grid Visuals

        GameObject gridVisualsParent = new GameObject("Grid Cells");
        gridVisuals = new GameObject[GridSizeX, GridSizeY];

        Texture2D spriteTex = Resources.Load<Texture2D>("Square");

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeX; y++)
            {
                #region Instantiate Cell

                GameObject cell = new GameObject("Cell " + x + "," + y);
                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(spriteTex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 256);
                cell.transform.position = new Vector3(x - GridSizeX / 2, y - GridSizeY / 2, 0);
                sr.color = Color.black;
                gridVisuals[x, y] = cell;
                cell.transform.parent = gridVisualsParent.transform;

                #endregion
            }
        }

        #endregion

        #region Instantiate Grid Model Data

        gridData = new CellData[GridSizeX, GridSizeY];

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeX; y++)
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

        UpdateVisualsFromModelData();

    }

    void Update()
    {
        elapsedTimeSinceLastGeneration += Time.deltaTime;

        if (elapsedTimeSinceLastGeneration >= TimeToWaitForNextGeneration)
        {
            generationNumber++;
            elapsedTimeSinceLastGeneration -= TimeToWaitForNextGeneration;

            #region Process Generation On Model Data

            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeX; y++)
                {
                    gridData[x, y].isAliveNextGeneration = DetermineIfCellIsAliveNextGeneration(x, y);
                }
            }

            for (int x = 0; x < GridSizeX; x++)
            {
                for (int y = 0; y < GridSizeX; y++)
                {
                    gridData[x, y].isAlive = gridData[x, y].isAliveNextGeneration;
                    gridData[x, y].isAliveNextGeneration = false;
                }
            }

            #endregion

            UpdateVisualsFromModelData();

            #region Benchmark Check

            timeSinceLastBenchmark += Time.deltaTime;

            if (generationNumber % 10000 == 0)
            {
                Debug.Log("Benchmark #" + generationNumber / 10000 + ", time taken == " + timeSinceLastBenchmark);
                timeSinceLastBenchmark = 0;
            }

            #endregion
        }

    }

    public bool DetermineIfCellIsAliveNextGeneration(int x, int y)
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

    public void UpdateVisualsFromModelData()
    {
        generationNumberText.text = "Generation #" + generationNumber;

        for (int x = 0; x < GridSizeX; x++)
        {
            for (int y = 0; y < GridSizeX; y++)
            {
                if (gridData[x, y].isAlive)
                    gridVisuals[x, y].GetComponent<SpriteRenderer>().color = Color.yellow;
                else
                    gridVisuals[x, y].GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
    }

    public static void ThreadProcess()
    {
        for (int i = 0; i < 500; i++)
            Debug.Log("Threaded Process... " + i);

        Debug.Log("Competed!");
    }

    void OnApplicationQuit()
    {
        sampleThread.Abort();
    }

}

public class CellData
{
    public bool isAlive;
    public bool isAliveNextGeneration;
}
