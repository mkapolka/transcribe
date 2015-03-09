using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Goblins : MonoBehaviour {
    public const float RESPAWN_TIMER_MIN = 10.0f;
    public const float RESPAWN_TIMER_MAX = 20.0f;
    
    public Town targetTown;
    public Town[] targetableTowns;
    private bool killed = false;

    void Start() {
        if (this.targetTown != null && GameState.goblinTargetTown == null) {
            this.SetTargetTown(this.targetTown);
        }
    }

    public void Initialize() {
        if (GameState.goblinTargetTown != null && !GameState.goblinsKilled) {
            this.SetTargetTown(Town.GetTown(GameState.goblinTargetTown));
        }
        this.killed = GameState.goblinsKilled;
    }

    public void Cleanup() {
        GameState.goblinTargetTown = this.targetTown.townId;
        GameState.goblinsKilled = this.killed;
    }

    public void SetKilled(bool killed) {
        this.killed = killed;
        this.GetComponent<Animator>().SetBool("killed", killed);
    }

    public bool AreKilled() {
        return this.killed;
    }

    private IEnumerator Respawn() {
        float timer = Random.Range(Goblins.RESPAWN_TIMER_MIN, Goblins.RESPAWN_TIMER_MAX);
        do {
            if (!Dialog.InDialog()) {
                timer -= Time.deltaTime;
            }
            yield return null;
        } while (timer > 0);
        Town nextTown = this.PickTown();
        print("Placing goblins at: " + nextTown.townName);
        this.SetTargetTown(nextTown);
        this.SetKilled(false);
    }

    public Town PickTown() {
        List<Town> availableTowns = new List<Town>(this.targetableTowns);
        availableTowns.Remove(this.targetTown);
        return availableTowns[Random.Range(0, availableTowns.Count)];
    }

    public void Kill() {
        if (!this.killed) {
            this.targetTown.SetHasGoblins(false);
            //GameObject.Destroy(this.gameObject);
            this.SetKilled(true);
            this.StartCoroutine(Respawn());
        }
    }

    public void SetTargetTown(Town town) {
        if (this.targetTown != null) {
            this.targetTown.SetHasGoblins(false);
        }

        this.targetTown = town;
        this.transform.position = town.transform.position;
        town.SetHasGoblins(true);
    }
}
