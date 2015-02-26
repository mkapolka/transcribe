using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateInitializer : MonoBehaviour {

    public GameObject unitPrefab;
	// Use this for initialization
	void Start () {
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        foreach (Town town in towns) {
            town.InitializeState();
        }

        List<Unit> units = new List<Unit>();
	    // Initialize people
        foreach (GameState.PersonState state in GameState.personStates) {
            print("Loading " + state.id);
            Unit unit = (GameObject.Instantiate(this.unitPrefab, state.position, Quaternion.identity) as GameObject).GetComponent<Unit>();
            if (state.placeAtTown != null) {
                unit.transform.position = Town.GetTown(state.placeAtTown).transform.position;
                state.placeAtTown = null;
            }
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
