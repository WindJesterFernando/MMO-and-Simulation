using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{

    Vector3 lerpMoveStart, lerpMoveEnd;
    float lerpMoveTimeUntilComplete, lerpMoveTimeElapsed;

    const float moveSpeed = 10;

    void Start()
    {

    }

    void Update()
    {

        #region On Mouse Click, Setup Lerp Movement

        bool mouseClick = Input.GetMouseButton(0);

        if (mouseClick)
        {
            lerpMoveStart = transform.position;

            Vector2 mousePos = Input.mousePosition;
            float cameraDistanceInZ = Mathf.Abs(Camera.main.transform.position.z);
            lerpMoveEnd = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cameraDistanceInZ));

            float xDif = Mathf.Abs(lerpMoveStart.x - lerpMoveEnd.x);
            float yDif = Mathf.Abs(lerpMoveStart.y - lerpMoveEnd.y);
            float dist = Mathf.Sqrt(xDif * xDif + yDif * yDif);

            lerpMoveTimeUntilComplete = dist / moveSpeed;

            lerpMoveTimeElapsed = 0;

            string netMsg = Utilities.Concatenate((int)ClientToServerSignifiers.LocalPlayerLerpMove, 
                mousePos.x.ToString(), 
                mousePos.y.ToString(), 
                lerpMoveEnd.x.ToString(), 
                lerpMoveEnd.y.ToString(), 
                lerpMoveTimeUntilComplete.ToString());
            NetworkClientProcessing.SendMessageToServer(netMsg);
        }

        #endregion

        #region Lerp Move Character

        if(transform.position != lerpMoveEnd)
        {
            lerpMoveTimeElapsed += Time.deltaTime;
            float timeCompletePercent = lerpMoveTimeElapsed / lerpMoveTimeUntilComplete;
            transform.position = Vector3.Lerp(lerpMoveStart, lerpMoveEnd, timeCompletePercent);

        }

        #endregion

    }
}
