using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiftingSprite : MonoBehaviour {

    public const float MAX_SPEED = 10.0f;

    private List<ShiftingSprite> otherSprites = new List<ShiftingSprite>();
    public Transform sprite;
    private Vector3 offsetDirection;

    void Start() {
        this.offsetDirection = Random.onUnitSphere;
    }

	void Update () {
        Vector3 targetPosition = new Vector3();
        if (this.otherSprites.Count > 0) {
            List<ShiftingSprite> nulled = new List<ShiftingSprite>();
            Vector3 otherCenter = new Vector3();
            foreach (ShiftingSprite otherSprite in this.otherSprites) {
                if (otherSprite == null) {
                    nulled.Add(otherSprite);
                } else {
                    otherCenter += otherSprite.transform.position - this.transform.position;
                }
            }
            otherCenter /= this.otherSprites.Count;

            if (otherCenter.magnitude == 0) {
                otherCenter = this.offsetDirection;
            }

            // Move towards target position
            targetPosition = (-otherCenter.normalized * Mathf.Max(1 - otherCenter.magnitude, 0));

            foreach (ShiftingSprite sprite in nulled) {
                this.otherSprites.Remove(sprite);
            }
        }
        Vector3 delta = targetPosition - this.sprite.localPosition;
        this.sprite.localPosition += delta * Time.deltaTime * MAX_SPEED;
	}

    void OnTriggerEnter2D(Collider2D other) {
        ShiftingSprite otherSprite = other.GetComponent<ShiftingSprite>();
        if (otherSprite != null) {
            this.otherSprites.Add(otherSprite);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        ShiftingSprite otherSprite = other.GetComponent<ShiftingSprite>();
        if (otherSprite != null) {
            this.otherSprites.Remove(otherSprite);
        }
    }
}
