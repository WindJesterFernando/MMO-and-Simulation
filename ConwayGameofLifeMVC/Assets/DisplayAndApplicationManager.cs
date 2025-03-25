using UnityEngine;
using TMPro;
using UnityEditor;
using System.Collections.Generic;

public class DisplayAndApplicationManager : MonoBehaviour
{
    GameObject[,] gridVisuals;
    [SerializeField] TMP_Text generationNumberText;

    bool threadIsPaused;
    Queue<bool[,]> bufferQueueOfModelDataForVisuals;

    void Start()
    {
        bufferQueueOfModelDataForVisuals = new Queue<bool[,]>();

        #region Instantiate Grid Visuals

        GameObject gridVisualsParent = new GameObject("Grid Cells");
        gridVisuals = new GameObject[ConwaySimulation.GridSizeX, ConwaySimulation.GridSizeY];

        Texture2D spriteTex = Resources.Load<Texture2D>("Square");

        for (int x = 0; x < ConwaySimulation.GridSizeX; x++)
        {
            for (int y = 0; y < ConwaySimulation.GridSizeY; y++)
            {
                #region Instantiate Cell

                GameObject cell = new GameObject("Cell " + x + "," + y);
                SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                sr.sprite = Sprite.Create(spriteTex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f), 256);
                cell.transform.position = new Vector3(x - ConwaySimulation.GridSizeX / 2, y - ConwaySimulation.GridSizeY / 2, 0);
                sr.color = Color.black;
                gridVisuals[x, y] = cell;
                cell.transform.parent = gridVisualsParent.transform;

                #endregion
            }
        }

        #endregion

        ConwaySimulation.Init(this);

        UpdateVisualsFromModelData();

        EditorApplication.pauseStateChanged += LogPauseState;

        ConwaySimulation.StartSimulationThreads();

    }
    void Update()
    {
        UpdateVisualsFromModelData();
    }
    public void UpdateVisualsFromModelData()
    {
        generationNumberText.text = "Generation #" + ConwaySimulation.GetGenerationNumber();

        bool[,] fromBuffer = null;

        lock (bufferQueueOfModelDataForVisuals)
        {
            if (bufferQueueOfModelDataForVisuals.Count > 0)
            {
                if (bufferQueueOfModelDataForVisuals.Count > 1)
                    Debug.Log(bufferQueueOfModelDataForVisuals.Count);

                while (bufferQueueOfModelDataForVisuals.Count > 1)
                    bufferQueueOfModelDataForVisuals.Dequeue();

                fromBuffer = bufferQueueOfModelDataForVisuals.Dequeue();
            }
        }

        if(fromBuffer != null)
        {
            for (int x = 0; x < ConwaySimulation.GridSizeX; x++)
            {
                for (int y = 0; y < ConwaySimulation.GridSizeY; y++)
                {
                    if (fromBuffer[x, y])
                        gridVisuals[x, y].GetComponent<SpriteRenderer>().color = Color.yellow;
                    else
                        gridVisuals[x, y].GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
        }
    }
    void OnApplicationQuit()
    {
        ConwaySimulation.AbortThreads();
    }
    private void LogPauseState(PauseState state)
    {
        threadIsPaused = (state == PauseState.Paused);
    }
    public bool IsBufferQueueOfModelDataForVisualsEmpty()
    {
        return bufferQueueOfModelDataForVisuals.Count == 0;
    }
    public void EnqueueBufferOfModelDataForVisuals(bool[,] dataToEnqueue)
    {
        lock (bufferQueueOfModelDataForVisuals)
            bufferQueueOfModelDataForVisuals.Enqueue(dataToEnqueue);
    }
    public bool IsThreadPaused()
    {
        return threadIsPaused;
    }

}

// private bool[,] CreateDeepCopyOfGrid(bool[,] toCopy)
// {
//     bool[,] newCopy = new bool[GridSizeX, GridSizeY];

//     for (int x = 0; x < GridSizeX; x++)
//     {
//         for (int y = 0; y < GridSizeX; y++)
//         {
//             newCopy[x, y] = toCopy[x, y];
//         }
//     }

//     //newCopy = (bool[,])toCopy.Clone();
//     //https://stackoverflow.com/questions/15725840/copy-one-2d-array-to-another-2d-array

//     return newCopy;
// }