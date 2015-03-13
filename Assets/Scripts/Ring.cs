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

    public static bool IsAtATown() {
        return GameState.ringState.locationType == GameState.RingState.LocationType.Town;
    }

    public static Town GetCurrentTown() {
        return Town.GetTown(GameState.ringState.location);
    }

    public static bool IsAtTown(Town town) {
        GameState.RingState state = GameState.ringState;
        return state.locationType == GameState.RingState.LocationType.Town && state.location == town.townId;
    }

    public static bool BelongsTo(Unit person) {
        GameState.RingState state = GameState.ringState;
        return state.locationType == GameState.RingState.LocationType.Person && state.location == person.GetId();
    }
}
