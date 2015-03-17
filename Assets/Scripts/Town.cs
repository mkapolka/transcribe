using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Town : MonoBehaviour {
    public string townName;
    public string townId;
	public Town[] connected;
    public GameObject unitPrefab;
    public string[] folkSongs;
    public bool canSendBards;

    public bool innatelyDangerous;
    [System.NonSerialized]
    private bool hasGoblins;

    protected GameState.TownState state;

    public static Town GetTown(string townId) {
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        foreach (Town town in towns) {
            if (town.townId == townId) {
                return town;
            }
        }
        return null;
    }

    public void Start() {}

    virtual public void InitializeState() {
        GameState.TownState townState = GameState.GetTownState(this.townId);
        this.townName = townState.townName;
        this.state = townState;
    }

    virtual public void StoreState() {
        this.state.townName = this.townName;
        this.state.id = this.townId;
        GameState.SetTownState(this.state);
    }

    public void Cleanup() {
        this.StoreState();
    }

    private Dictionary<string, string> GetDialogParameters() {
        return new Dictionary<string, string>() {
            {"townName", this.townName}
        };
    }

    public bool IsDangerous() {
        return this.innatelyDangerous || this.hasGoblins;
    }

    public void SetHasGoblins(bool val) {
        this.hasGoblins = val;
        if (val) {
            Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
            foreach (Unit unit in units) {
                if (unit.currentTown == this || unit.nextTown == this && !(unit.mode == Unit.Mode.SoldierDefend)) {
                    unit.BeScaredByGoblins(this);
                }
            }
        }
    }

    public void MouseUp() {
        if (this.canSendBards) {
            if (this.IsDangerous()) {
                GameState.ShowDialog("dangerous_town", this.GetDialogParameters());
            } else {
                bool bardGoingToTown = false;
                Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
                foreach (Unit unit in units) {
                    if (unit.type == Unit.Type.Bard && (unit.currentTown == this || unit.targetTown == this)) {
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
    }

    public void ProcessStories(Story[] stories) {
        foreach (Story story in stories) {
            this.ProcessStory(story);
        }
    }

    public void ProcessStory(Story story) {
        //TODO Temporarily disabled, remove permanently?
        /*
        switch (story.id) {
            case "class_warrior":
                if (!GameState.hasSpawnedWarrior) {
                    GameState.hasSpawnedWarrior = true;
                    Unit warrior = this.SpawnPerson("warrior");
                    warrior.ShowDialog("inspire_soldier");
                }
            break;
            case "location_hanging_tree":
                if (!GameState.hasSpawnedAdventurer) {
                    GameState.hasSpawnedAdventurer = true;
                    Unit adventurer = this.SpawnPerson("adventurer");
                    adventurer.ShowDialog("inspire_adventurer");
                    adventurer.SetTargetTown("hanging_tree");
                    adventurer.HearStory(story);
                    adventurer.LearnStory(GameState.GetStory("location_smidge_ridge"));
                }
            break;
        }*/
    }

    Unit SpawnPerson(string id) {
        GameState.PersonTemplate template = GameState.GetPersonTemplate(id);
        Unit unit = (GameObject.Instantiate(this.unitPrefab, this.transform.position, Quaternion.identity) as GameObject).GetComponent<Unit>();
        GameState.PersonState state = GameState.InstantiatePersonFromTemplate(template, id);
        state.position = this.transform.position;
        unit.LoadState(state);
        unit.ArriveAtTown(this, true);
        return unit;
    }

    /* Returns all the towns connected to this one in order of their distance from this town */
    private Town[] GetDistanceOrderedTowns() {
        List<Town> orderedTowns = new List<Town>();
        Queue<Town> remainingTowns = new Queue<Town>();
        remainingTowns.Enqueue(this);
        Town nextTown;
        do {
            nextTown = remainingTowns.Dequeue();
            orderedTowns.Add(nextTown);
            foreach (Town town in nextTown.connected) {
                if (!orderedTowns.Contains(town)) {
                    remainingTowns.Enqueue(town);
                }
            }
        } while (remainingTowns.Count > 0);
        return orderedTowns.ToArray();
    }

    public Town GetNearestBardableTown() {
        Town[] towns = this.GetDistanceOrderedTowns();
        foreach (Town town in towns) {
            if (town.canSendBards && !town.IsDangerous()) {
                return town;
            }
        }
        return null;
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
                if (town == null) {
                    print(this + " " + targetTown);
                }
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
