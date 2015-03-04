using UnityEngine;
using System.Collections;

public class CameraDrag : MonoBehaviour {
    private Vector3 lastPosition;
    public RectTransform bounds;

	void Start() {
        this.lastPosition = Input.mousePosition;
    }

	void Update () {
	    if (Input.GetButton("Fire1") && !Dialog.InDialog()) {
            Camera camera = this.GetComponent<Camera>();
            Vector3 newPosition = this.transform.position + (camera.ScreenToWorldPoint(this.lastPosition) - camera.ScreenToWorldPoint(Input.mousePosition));

            Vector3[] corners = new Vector3[4];
            this.bounds.GetWorldCorners(corners);
            Vector3 upperLeft = corners[1];
            Vector3 lowerRight = corners[3];
            newPosition.x = Mathf.Clamp(newPosition.x, upperLeft.x, lowerRight.x);
            newPosition.y = Mathf.Clamp(newPosition.y, lowerRight.y, upperLeft.y);
            this.transform.position = newPosition;
        }
        this.lastPosition = Input.mousePosition;
	}
}
