using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Town : MonoBehaviour {
    public string townName;
    public string townId;
	public Town[] connected;
    public Book book;
    public GameObject unitPrefab;

    public static Town GetTown(string townId) {
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        foreach (Town town in towns) {
            if (town.townId == townId) {
                return town;
            }
        }
        return null;
    }

    public void Start() {
        // this.InitializeState();
    }

    public void InitializeState() {
        GameState.TownState townState = GameState.GetTownState(this.townId);
        this.townName = townState.townName;
        this.book = townState.book;
    }

    private Dictionary<string, string> GetDialogParameters() {
        return new Dictionary<string, string>() {
            {"townName", this.townName}
        };
    }

    public void MouseUp() {
        if (this.townId != "mission") {
            bool bardGoingToTown = false;
            Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
            foreach (Unit unit in units) {
                if (unit.type == Unit.Type.Bard && unit.targetTown == this) {
                    bardGoingToTown = true;
                }
            }

            if (GameState.availableBards.Count <= 0) {
                GameState.ShowDialog("no_bards", this.GetDialogParameters());
            } else if (bardGoingToTown) {
                GameState.ShowDialog("bard_already_at_town", this.GetDialogParameters());
            } else {
                GameState.targetTown = GameState.GetTownState(this.townId);
                GameState.LoadScene("Writing");
            }
        }
    }

    void Update() {
        if (this.book != null) {
            this.ProcessStories(this.book.stories);
        }
    }

    public void ProcessStories(Story[] stories) {
        foreach (Story story in stories) {
            this.ProcessStory(story);
        }
    }

    public void ProcessStory(Story story) {
        switch (story.id) {
            case "class_warrior":
                if (!GameState.hasSpawnedWarrior) {
                    GameState.hasSpawnedWarrior = true;
                    Unit warrior = this.SpawnPerson("warrior");
                    warrior.ShowDialog("inspire_soldier");
                }
            break;
        }
    }

    Unit SpawnPerson(string id) {
        GameState.PersonTemplate template = GameState.GetPersonTemplate(id);
        Unit unit = (GameObject.Instantiate(this.unitPrefab, this.transform.position, Quaternion.identity) as GameObject).GetComponent<Unit>();
        GameState.PersonState state = GameState.InstantiatePersonFromTemplate(template, id);
        state.position = this.transform.position;
        unit.LoadState(state);
        unit.ArriveAtTown(this);
        return unit;
    }

    /*
     * Return the next town that you have to go through to get to the target town
     */
    public Town GetNextTown(Town targetTown) {
        if (targetTown == this) {
            return this;
        }

        Stack<Town> remainingTowns = new Stack<Town>();
        Dictionary<Town, int> townDistances = new Dictionary<Town, int>();
        remainingTowns.Push(targetTown);
        townDistances.Add(targetTown, 0);
        while (remainingTowns.Count != 0) {
            Town nextTown = remainingTowns.Pop();
            int nextDistance = townDistances[nextTown];
            foreach (Town town in nextTown.connected) {
                if (!townDistances.ContainsKey(town)) {
                    townDistances.Add(town, nextDistance + 1);
                    remainingTowns.Push(town);
                }
            }
        }

        int minDistance = int.MaxValue;
        Town minTown = null;
        foreach (Town town in this.connected) {
            if (townDistances[town] < minDistance) {
                minDistance = townDistances[town];
                minTown = town;
            }
        }
        return minTown;
    }
}
