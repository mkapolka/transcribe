using UnityEngine;
using System.Collections;

public class Cursor : MonoBehaviour {

	void Update() {
        Vector3 np = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        np.z = 0;
        this.transform.position = np;
    }
}
