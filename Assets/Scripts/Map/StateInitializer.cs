using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateInitializer : MonoBehaviour {

    public GameObject unitPrefab;
	// Use this for initialization
	void Start () {
        GameState.InitializeState();

        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        foreach (Town town in towns) {
            try {
                town.InitializeState();
            } catch {
                print("Couldn't find town with id: " + town.townId);
            }
        }

        List<Unit> units = new List<Unit>();
	    // Initialize people
        foreach (GameState.PersonState state in GameState.personStates) {
            print("Loading " + state.id);
            Vector3 position;
            if (state.placeAtTown != null) {
                position = Town.GetTown(state.placeAtTown).transform.position;
                state.placeAtTown = null;
            } else {
                position = state.position;
            }

            Unit unit = (GameObject.Instantiate(this.unitPrefab, position, Quaternion.identity) as GameObject).GetComponent<Unit>();
            unit.LoadState(state);
            units.Add(unit);
        }

        foreach (Unit unit in units) {
            if (unit.currentTown != null) {
                unit.ArriveAtTown(unit.currentTown);
            }
        }
	}
}
