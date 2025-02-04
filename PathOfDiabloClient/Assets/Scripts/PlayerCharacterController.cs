using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{

    Vector3 lerpMoveStart, lerpMoveEnd;
    float lerpMoveTimeUntilComplete, lerpMoveTimeElapsed;

    const float moveSpeed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool mouseClick = Input.GetMouseButton(0);

        if (mouseClick)
        {
            lerpMoveStart = transform.position;

            Vector2 mousePos = Input.mousePosition;
            lerpMoveEnd = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));//Camera.main.nearClipPlane));

            float xDif = Mathf.Abs(lerpMoveStart.x - lerpMoveEnd.x);
            float yDif = Mathf.Abs(lerpMoveStart.y - lerpMoveEnd.y);
            float dist = Mathf.Sqrt(xDif * xDif + yDif * yDif);

            lerpMoveTimeUntilComplete = dist / moveSpeed;

            //transform.position = moveTo;
            lerpMoveTimeElapsed = 0;
        }

        if(transform.position != lerpMoveEnd)
        {
            lerpMoveTimeElapsed += Time.deltaTime;
            float timeCompletePercent = lerpMoveTimeElapsed / lerpMoveTimeUntilComplete;
            //Mathf.Lerp(lerpMoveStart, lerpMoveEnd, lerpMoveTimer);
            transform.position = Vector3.Lerp(lerpMoveStart, lerpMoveEnd, timeCompletePercent);

        }


    }
}
