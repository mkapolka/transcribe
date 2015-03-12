using UnityEngine;
using System.Collections;

public class Ring : MonoBehaviour {
	private Transform owner;

	void Update () {
	    this.transform.position = this.owner.position;
	}

    public void PlaceInTown(Town town) {
        this.owner = town.transform;
        GameState.ringState.locationType = GameState.RingState.LocationType.Town;
        GameState.ringState.location = town.townId;
    }

    public void GiveToPerson(Unit unit) {
        this.owner = unit.transform;
        GameState.ringState.locationType = GameState.RingState.LocationType.Person;
        GameState.ringState.location = unit.GetId();
    }
}
