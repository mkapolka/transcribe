using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFocuser : MonoBehaviour {
    public const float CAMERA_PAN_SPEED = 25.0f;

    public class CameraEvent {
        public Transform target;
        public string dialogId;
    }

    private Queue<CameraEvent> eventQueue = new Queue<CameraEvent>();
    private CameraEvent currentEvent;

	public void DequeueEvent() {
        if (this.eventQueue.Count > 0) {
            this.currentEvent = this.eventQueue.Dequeue();
            StartCoroutine("CenterCamera");
            GameState.ShowDialog(this.currentEvent.dialogId);
        } else {
            this.currentEvent = null;
        }
    }

    public void EnqueueEvent(Transform target, string dialogId) {
        CameraEvent e = new CameraEvent();
        e.target = target;
        e.dialogId = dialogId;
        this.EnqueueEvent(e);
    }

    public void EnqueueEvent(CameraEvent e) {
        this.eventQueue.Enqueue(e);
        if (this.currentEvent == null) {
            this.DequeueEvent();
        }
    }
	
    IEnumerator CenterCamera() {
        Vector3 delta;
        Transform currentTarget = this.currentEvent.target;
        do {
            delta = currentTarget.position - this.transform.position;
            delta.z = 0;
            if (delta.magnitude > Time.deltaTime * CAMERA_PAN_SPEED) {
                this.transform.position += delta.normalized * Time.deltaTime * CAMERA_PAN_SPEED;
            } else {
                this.transform.position += delta;
                delta = new Vector3(0, 0, 0);
            }
            yield return null;
        } while (delta.magnitude > .01);
    }

}
