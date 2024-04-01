using UnityEngine;

public class ZoomHandler : MonoBehaviour
{
    [SerializeField] RectTransform rt;

    Vector3 touchStart;
    float zoomOutMin = 1;
    public float zoomOutMax = 5;

    float xMin = 0;
    float xMax = 0;
    float yMin = 0;
    float yMax = 0;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
        }
        if(Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * 0.01f);
        }
        else if(Input.GetMouseButton(0))
        {
            Vector3 direction = (touchStart - Input.mousePosition);
            rt.anchoredPosition -= new Vector2(direction.x, direction.y);
            touchStart -= direction;
        }

        float x = Mathf.Clamp(rt.anchoredPosition.x, xMin, xMax);
        float y = Mathf.Clamp(rt.anchoredPosition.y, yMin, yMax);
        rt.anchoredPosition = new Vector2(x, y);
    }

    void Zoom(float increment)
    {
        float zoomAmount = Mathf.Clamp(rt.localScale.x + increment, zoomOutMin, zoomOutMax);
        rt.localScale = new Vector3(zoomAmount, zoomAmount, 1);

        xMax = (rt.localScale.x - 1) * (1920 / 2);
        yMax = (rt.localScale.y - 1) * (1080 / 2);

        xMin = -xMax;
        yMin = -yMax;
    }
}
