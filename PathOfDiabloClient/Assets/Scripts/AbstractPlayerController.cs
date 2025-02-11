using UnityEngine;

public class AbstractPlayerController : MonoBehaviour
{

    protected Vector3 lerpMoveStart, lerpMoveEnd;
    protected float lerpMoveTimeUntilComplete, lerpMoveTimeElapsed;

    void Start()
    {

    }

    protected void UpdateLerpMovement()
    {
        if (transform.position != lerpMoveEnd)
        {
            lerpMoveTimeElapsed += Time.deltaTime;
            float timeCompletePercent = lerpMoveTimeElapsed / lerpMoveTimeUntilComplete;
            transform.position = Vector3.Lerp(lerpMoveStart, lerpMoveEnd, timeCompletePercent);
        }
    }

    public void SetLerpMoveEndToCurrentPosition()
    {
        lerpMoveEnd = transform.position;
    }
}
