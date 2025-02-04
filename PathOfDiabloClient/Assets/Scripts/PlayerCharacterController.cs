using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
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
            Vector2 mousePos = Input.mousePosition;
            Vector3 moveTo = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));//Camera.main.nearClipPlane));

            transform.position = moveTo;
        }

    }
}
