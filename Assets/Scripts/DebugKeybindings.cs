using UnityEngine;
using System.Collections;

public class DebugKeybindings : MonoBehaviour {

    public static float unitBaseSpeed;

	void Update () {
	    if (Input.GetKeyDown("g")) {
            print("Moving goblins");
            Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
            Town town = goblins.PickTown();
            goblins.SetTargetTown(town);
            goblins.SetKilled(false);
        }

        if (Input.GetKeyDown("s")) {
            print("Pronto!");
            DebugKeybindings.unitBaseSpeed = Unit.BASE_SPEED;
            Unit.BASE_SPEED *= 3.0f;
        }

        if (Input.GetKeyUp("s")) {
            print("Langsamer!");
            Unit.BASE_SPEED = DebugKeybindings.unitBaseSpeed;
        }

        if (Input.GetKeyUp("r")) {
            print("Ring");
            Unit bearer = Unit.GetUnit("adventurer");
            Ring ring = GameObject.FindObjectOfType(typeof(Ring)) as Ring;
            ring.GiveToPerson(bearer);
        }
	}
}
