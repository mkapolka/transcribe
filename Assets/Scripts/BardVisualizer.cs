using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BardVisualizer : MonoBehaviour {
    public const float BARD_SPEED = 1000.0f;

    [System.Serializable]
    public class Bard {
        public string bardId;
        public Transform transform;
        [System.NonSerialized]
        public Vector3 startPosition;
        [System.NonSerialized]
        public bool available;
    }

    public Bard[] bards;
    public Transform hidePosition;

	// Use this for initialization
	void Start () {
	    foreach (Bard bard in this.bards) {
            bard.startPosition = bard.transform.position;
            if (GameState.availableBards.IndexOf(bard.bardId) == -1) {
                bard.transform.position = this.hidePosition.position;
            }
        }
	}

    void Update() {
        foreach (Bard bard in this.bards) {
            bool actuallyAvailable = GameState.availableBards.Contains(bard.bardId);
            if (bard.available != actuallyAvailable) {
                StartCoroutine(MoveToTarget(bard));
                bard.available = actuallyAvailable;
            }
        }
    }

    IEnumerator MoveToTarget(Bard bard) {
        bool atTarget = false;
        do {
            Vector3 targetPosition = new Vector3();
            if (GameState.availableBards.Contains(bard.bardId)) {
                targetPosition = bard.startPosition;
            } else {
                targetPosition = this.hidePosition.position;
            }
            Vector3 delta = targetPosition - bard.transform.position;
            atTarget = delta.magnitude < Time.deltaTime * BARD_SPEED;
            if (delta.magnitude > Time.deltaTime * BARD_SPEED) {
                delta = delta.normalized * Time.deltaTime * BARD_SPEED;
            }
            bard.transform.position += delta;
            yield return null;
        } while (!atTarget);
    }
}
