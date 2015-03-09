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
        this.SetTargetTown(Town.GetTown(GameState.goblinTargetTown));
    }

    public void Cleanup() {
        GameState.goblinTargetTown = this.targetTown.townId;
        GameState.goblinsKilled = this.killed;
    }

    private void SetKilled(bool killed) {
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

        List<Town> availableTowns = new List<Town>(this.targetableTowns);
        availableTowns.Remove(this.targetTown);
        Town nextTown = availableTowns[Random.Range(0, availableTowns.Count)];
        print("Placing goblins at: " + nextTown.townName);
        this.SetTargetTown(nextTown);
        this.SetKilled(false);
    }

    public void Kill() {
        if (!this.killed) {
            this.targetTown.hasGoblins = false;
            //GameObject.Destroy(this.gameObject);
            this.SetKilled(true);
            this.StartCoroutine(Respawn());
        }
    }

    public void SetTargetTown(Town town) {
        if (this.targetTown != null) {
            this.targetTown.hasGoblins = false;
        }

        this.targetTown = town;
        this.transform.position = town.transform.position;
        town.hasGoblins = true;
    }
}
