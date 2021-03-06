﻿using UnityEngine;
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
            Unit unit = (GameObject.Instantiate(this.unitPrefab, state.GetPosition(), Quaternion.identity) as GameObject).GetComponent<Unit>();
            unit.LoadState(state);
            units.Add(unit);
        }

        foreach (Unit unit in units) {
            if (unit.currentTown != null) {
                //unit.ArriveAtTown(unit.currentTown);
            }
        }

        //Initialize Goblins
        Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
        goblins.Initialize();

        //Initialize ring
        Ring ring = GameObject.FindObjectOfType(typeof(Ring)) as Ring;
        switch (GameState.ringState.locationType) {
            case GameState.RingState.LocationType.Town:
                Town town = Town.GetTown(GameState.ringState.location);
                ring.transform.position = town.transform.position;
                ring.PlaceInTown(town);
            break;

            case GameState.RingState.LocationType.Person:
                Unit unit = Unit.GetUnit(GameState.ringState.location);
                ring.transform.position = unit.transform.position;
                ring.GiveToPerson(unit);
            break;
        }
	}
}
