using UnityEngine;

public class PlayerCharacterController : AbstractPlayerController
{
    const float moveSpeed = 10;

    Vector2 lastMouseDownPosition;
    bool mouseWasDownDuringLastUpdate;

    void Start()
    {

    }

    void Update()
    {
        UpdateLerpMovement();

        #region On Mouse Click, Setup Lerp Movement

        bool mouseDown = Input.GetMouseButton(0);

        if (mouseDown && !mouseWasDownDuringLastUpdate)
        {
            Vector2 mousePos = Input.mousePosition;

            if (lastMouseDownPosition != mousePos)
            {
                lerpMoveStart = transform.position;

                float cameraDistanceInZ = Mathf.Abs(Camera.main.transform.position.z);
                lerpMoveEnd = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cameraDistanceInZ));

                float xDif = Mathf.Abs(lerpMoveStart.x - lerpMoveEnd.x);
                float yDif = Mathf.Abs(lerpMoveStart.y - lerpMoveEnd.y);
                float dist = Mathf.Sqrt(xDif * xDif + yDif * yDif);

                lerpMoveTimeUntilComplete = dist / moveSpeed;

                lerpMoveTimeElapsed = 0;

                string netMsg = Utilities.Concatenate((int)ClientToServerSignifiers.LocalPlayerLerpMove,
                    lerpMoveStart.x.ToString(),
                    lerpMoveStart.y.ToString(),
                    lerpMoveEnd.x.ToString(),
                    lerpMoveEnd.y.ToString(),
                    lerpMoveTimeUntilComplete.ToString());
                NetworkClientProcessing.SendMessageToServer(netMsg);

                lastMouseDownPosition = mousePos;
            }

            mouseWasDownDuringLastUpdate = true;
        }
        else
        {
            mouseWasDownDuringLastUpdate = false;
        }



        #endregion
    }
}
