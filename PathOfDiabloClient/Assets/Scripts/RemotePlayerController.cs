using UnityEngine;

public class RemotePlayerController : AbstractPlayerController
{

    void Start()
    {

    }

    void Update()
    {
        UpdateLerpMovement();
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
