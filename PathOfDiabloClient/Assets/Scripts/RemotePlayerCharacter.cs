using UnityEngine;

public class RemotePlayerCharacter : MonoBehaviour
{

    Vector3 lerpMoveStart, lerpMoveEnd;
    float lerpMoveTimeUntilComplete, lerpMoveTimeElapsed;

    void Start()
    {

    }

    void Update()
    {
        if (transform.position != lerpMoveEnd)
        {
            lerpMoveTimeElapsed += Time.deltaTime;
            float timeCompletePercent = lerpMoveTimeElapsed / lerpMoveTimeUntilComplete;
            transform.position = Vector3.Lerp(lerpMoveStart, lerpMoveEnd, timeCompletePercent);
        }
    }

    public void ReceiveLerpMoveData(float lerpMoveStartX, float lerpMoveStartY, float lerpMoveEndX, float lerpMoveEndY, float lerpMoveTimeUntilComplete)
    {
        float cameraDistanceInZ = Mathf.Abs(Camera.main.transform.position.z);
        lerpMoveStart = new Vector3(lerpMoveStartX, lerpMoveStartY, cameraDistanceInZ);
        lerpMoveEnd = new Vector3(lerpMoveEndX, lerpMoveEndY, cameraDistanceInZ);
        this.lerpMoveTimeUntilComplete = lerpMoveTimeUntilComplete;
        lerpMoveTimeElapsed = 0;
    }
}
