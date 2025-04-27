using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapControl : MonoBehaviour
{
    [SerializeField]
    private string mapReference = "Default";

    [SerializeField]
    private float zoomSpeed = 0.02f;

    private Map map;
    private bool regenerate;

    private void Update()
    {
        if (map == null) map = MapUtils.Maps[mapReference];

        if (!map.CanControl) return;

        if (Input.touchCount >= 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            map.ZoomLevel -= deltaMagnitudeDiff * (zoomSpeed / 10);

            regenerate = true;
        }
        else
        {
            if(Input.GetAxis("Mouse ScrollWheel") > 0.0f || Input.GetAxis("Mouse ScrollWheel") < 0.0f)
            {
                map.ZoomLevel += Input.GetAxis("Mouse ScrollWheel") * (zoomSpeed * 100);

                regenerate = true;
            }
            else
            {
                regenerate = false;
            }
        }

        if(regenerate)
        {
            if (map.ZoomLevel >= map.MinZoom && map.ZoomLevel <= map.MaxZoom)
            {
                map.Regenerate();
            }
            else
            {
                if (map.ZoomLevel < map.MinZoom) map.ZoomLevel = map.MinZoom;

                if (map.ZoomLevel > map.MaxZoom) map.ZoomLevel = map.MaxZoom;
            }
        }
    }
}
