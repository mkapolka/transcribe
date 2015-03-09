using UnityEngine;
using System.Collections;

public class DebugKeybindings : MonoBehaviour {

	void Update () {
	    if (Input.GetKeyDown("g")) {
            print("Moving goblins");
            Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
            Town town = goblins.PickTown();
            goblins.SetTargetTown(town);
            goblins.SetKilled(false);
        }
	}
}
