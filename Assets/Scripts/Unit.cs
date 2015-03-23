using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
    // Units / second
    public static float BASE_SPEED = 1.0f;
    public const float INTERACT_DISTANCE = 0.2f;
    public const float CAMERA_PAN_SPEED = 10.0f;
    public const float FADE_SPEED = 2.0f;

    public const string STORY_SOLDIER_DEFEND = "defend_gaffer";
    public const string STORY_SOLDIER_DEMIURGE = "slay_demiurge";
    public const string STORY_ADVENTURER_QUEEN = "defend_gaffer";
    public const string STORY_ADVENTURER_RING = "location_hanging_tree";

	public enum Type {
        Bard, Soldier, Merchant, Wolf, Adventurer, FairyQueen
    }

    public enum Mode {
        Default, SoldierDefend, SoldierDemiurge, AdventurerRing, AdventurerDeliver
    }

    [System.Serializable]
    public class SpriteTemplate {
        public Type type;
        public Sprite sprite;
    }

    public SpriteTemplate[] spriteTemplates;

    private string id;
    public Type type;
    public Town nextTown;
    public Town targetTown;
    public Town currentTown;
    private float speed;
    public DialogBubble dialogBubble;
    private GameState.PersonState state;
    public Mode mode;
    public List<Story> heardStories = new List<Story>();
    public SpriteRenderer mainSprite;
    public Animator animator;

    // Move them a little off the center of the town for sprite overlapping purposes
    private Vector3 townOffset;

    private bool isKilled = false;

    public void Start() {
        this.townOffset = Random.onUnitSphere;
        this.townOffset.z = 0;

        if (this.state == null) {
            this.state = new GameState.PersonState();
            this.state.id = Random.Range(0, int.MaxValue).ToString();
            this.currentTown = this.GetNearestTown();
        }
    }

    public static Unit GetUnit(string personId) {
        Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
        foreach (Unit unit in units) {
            if (unit.id == personId) {
                return unit;
            }
        }
        return null;
    }

    public string GetName() {
        return this.state.name;
    }
    
    public string GetTitle() {
        return this.state.title;
    }

    private Town GetNearestTown() {
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        float nearestDistance = float.MaxValue;
        Town nearestTown = null;
        foreach (Town town in towns) {
            float distance = (this.transform.position - town.transform.position).magnitude;
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearestTown = town.GetComponent<Town>();
            }
        }
        return nearestTown;
    }

    void Kill() {
        this.isKilled = true;
        GameObject.Destroy(this.gameObject);
        this.MouseExit();
    }

    public void SetId(string id) {
        this.id = id;
        this.StoreState();
    }

    public string GetId() {
        return this.id;
    }

    public void SetMode(Mode mode) {
        this.mode = mode;
    }

    public bool IsAtTown(Town town) {
        return (town.transform.position - this.transform.position).magnitude < .1f;
    }

    public void Update() {
        if (!Dialog.InDialog()) {
            //TODO Refactor
            switch (this.mode) {
                case Mode.SoldierDefend:
                    this.UpdateSoldierDefend();
                break;

                case Mode.SoldierDemiurge:
                    this.UpdateSoldierDemiurge();
                break;

                case Mode.AdventurerRing:
                    this.UpdateAdventurerRing();
                break;

                case Mode.AdventurerDeliver:
                    this.UpdateAdventurerDeliver();
                break;
            }

            if (this.targetTown != null) {
                if (this.nextTown != this.targetTown && this.IsAtTown(this.nextTown)) {
                    this.ArriveAtTown(this.nextTown, false);
                    this.nextTown = this.nextTown.GetNextTown(this.targetTown);
                } else {
                    this.animator.SetBool("Walking", true);
                    this.MoveTowardsTown(this.nextTown);
                }

                if (this.IsAtTown(this.targetTown)) {
                    this.animator.SetBool("Walking", false);
                    this.MoveTowardsTown(this.nextTown);
                    this.currentTown = this.targetTown;
                    this.targetTown = null;
                    this.nextTown = null;
                    this.ArriveAtTown(this.currentTown, true);
                }
            }
        }
    }

    private void UpdateSoldierDefend() {
        if (this.targetTown == null) {
            Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
            if (goblins != null && !goblins.AreKilled() && goblins.targetTown != null) {
                Town town = goblins.targetTown;
                Dictionary<string, string> parameters = this.GetDialogParameters();
                parameters.Add("townName", town.townName);
                this.SetTargetTown(town);
            }
        }
    }

    private void UpdateSoldierDemiurge() {
        if (this.targetTown == null && this.currentTown.townId != "demiurge") {
            this.SetTargetTown(Town.GetTown("demiurge"));
        }
    }

    private void UpdateAdventurerRing() {
        if (this.targetTown == null && !Ring.BelongsTo(this) && Ring.IsAtATown()) {
            this.SetTargetTown(Ring.GetCurrentTown());
        }
    }

    private void UpdateAdventurerDeliver() {
        if (this.targetTown == null && this.currentTown.townId != "fairy_castle") {
            this.SetTargetTown(Town.GetTown("fairy_castle"));
        }
    }

    public void ArriveAtTown(Town town, bool isDestination) {
        if (this.type == Type.Bard && isDestination) {
            if (town.townId == "mission") {
                StartCoroutine("FadeOut");
                print("Available bards: " + string.Join(", ", GameState.availableBards.ToArray()));
            } else {
                town.ProcessStories(this.heardStories.ToArray());
                foreach (string storyId in town.folkSongs) {
                    if (!GameState.KnowsStory(storyId)) {
                        this.LearnStory(storyId);
                        this.ShowDialog("learn_folk_song");
                    }
                }
            }
        }

        if (this.type != Type.Bard && town.townId == "mission") {
            bool learned = false;
            List<Story> newStories = new List<Story>();
            foreach (Story story in this.heardStories) {
                if (!GameState.KnowsStory(story)) {
                    learned = true;
                    GameState.AddKnownStory(story);
                    newStories.Add(story);
                }
            }
            if (learned) {
                Dictionary<string, string> parameters = new Dictionary<string, string>() {
                    {"speakerId", this.state.id},
                    {"speakerName", this.state.name},
                    {"storyName", newStories[0].title}
                };
                GameState.ShowDialog("learn_from_unit", parameters);
            }
        }

        //TODO refactor
        Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
        if (goblins.targetTown == town && !goblins.AreKilled()) {
            if (this.mode == Mode.SoldierDefend) {
                goblins.Kill();
                this.animator.SetTrigger("WarriorFight");
            } else {
                this.BeScaredByGoblins(town);
            }
        }

        if (this.mode == Mode.AdventurerRing) {
            if (Ring.IsAtTown(town)) {
                Ring ring = GameObject.FindObjectOfType(typeof(Ring)) as Ring;
                this.ShowDialog("find_ring");
                ring.GiveToPerson(this);
            }
        }

        if (this.mode == Mode.AdventurerDeliver && Ring.BelongsTo(this) && town.townId == "fairy_castle") {
            this.ShowDialog("deliver_ring");
            Ring ring = GameObject.FindObjectOfType(typeof(Ring)) as Ring;
            ring.GiveToPerson(Unit.GetUnit("queen"));
            Door door = Town.GetTown("door_west") as Door;
            door.SetOpen(true);
        }
    }

    public void BeScaredByGoblins(Town atTown) {
        this.ShowDialog("flee_goblins");
        Town rTown = atTown.GetNearestBardableTown();
        print(rTown);
        this.SetTargetTown(rTown);

        if (this.mode == Mode.AdventurerRing) {
            this.SetMode(Mode.Default);
        }
    }

    IEnumerator FadeOut() {
        SpriteRenderer renderer = this.mainSprite;
        while (renderer.material.color.a > 0) {
            Color color = renderer.material.color;
            color.a -= Time.deltaTime * FADE_SPEED;
            renderer.material.color = color;
            yield return null;
        }
        GameState.RemovePerson(this.state);
        GameState.availableBards.Add(this.state.id);
        this.Kill();
    }

    public void LearnStory(string storyId) {
        this.LearnStory(GameState.GetStory(storyId));
    }

    public void LearnStory(Story story) {
        if (!this.heardStories.Contains(story)) {
            this.heardStories.Add(story);
        }
    }

    public void HearStory(Story story) {
        // Some stories should only be heard once
        if (this.heardStories.IndexOf(story) == -1) {
            this.heardStories.Add(story);

            switch (story.id) {
                case "non_existent":
                break;
            }
        }

        // Some stories can happen as many times as we want
        switch (story.id) {
            /*case "location_hanging_tree":
                this.HearTownStory("hanging_tree");
            break;

            case "location_smidge_ridge":
            case "class_warrior":
                this.HearTownStory("smidge_ridge");
            break;*/
        }

        //Type specific hear stories (TODO refactor this)
        switch (this.type) {
            case Type.Soldier:
                this.HearStorySoldier(story);
            break;

            case Type.Adventurer:
                this.HearStoryAdventurer(story);
            break;
        }
    }

    private void HearStorySoldier(Story story) {
        switch (story.id) {
            case STORY_SOLDIER_DEFEND:
                this.ShowDialog("inspire_defend");
                this.SetMode(Mode.SoldierDefend);
            break;

            case STORY_SOLDIER_DEMIURGE:
                this.ShowDialog("inspire_demiurge");
                this.SetMode(Mode.SoldierDemiurge);
            break;
        }
    }

    private void HearStoryAdventurer(Story story) {
        switch (story.id) {
            case STORY_ADVENTURER_RING:
                this.ShowDialog("go_to_hanging_tree");
                this.SetMode(Mode.AdventurerRing);
            break;

            case STORY_ADVENTURER_QUEEN:
                this.ShowDialog("seek_queen");
                this.SetMode(Mode.AdventurerDeliver);
            break;
        }
    }

    private void HearTownStory(string townId) {
        if (this.targetTown == null || this.targetTown.townId != townId) {
            this.SetTargetTown(townId);
            this.ShowDialog("go_to_" + townId);
        }
    }

    public void SetTargetTown(string townId) {
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        Town targetTown = null;
        foreach (Town town in towns) {
            if (town.townId == townId) {
                targetTown = town;
            }
        }
        if (targetTown == null) {
            throw new System.Exception("Couldn't find town with ID " + townId);
        }
        this.SetTargetTown(targetTown);
    }

    public void SetTargetTown(Town town) {
        this.currentTown = this.GetNearestTown();
        this.targetTown = town;
        if (town != null) {
            this.nextTown = this.currentTown.GetNextTown(town);
        }
    }

    public void MoveTowardsTown(Town town) {
        Vector3 delta = (town.transform.position + this.townOffset * 0.01f) - this.transform.position;
        if (delta.magnitude > Time.deltaTime * Unit.BASE_SPEED * this.speed) {
            delta = delta.normalized * Time.deltaTime * Unit.BASE_SPEED * this.speed;
        }
        this.transform.position += delta;
    }

    private Dictionary<string, string> GetDialogParameters() {
        return new Dictionary<string, string>() {
            {"speaker", this.state.id},
            {"speakerName", this.state.name},
            {"speakerPortrait", this.state.portraitId}
        };
    }

    public void ShowDialog(string dialogId, Dictionary<string, string> parameters = null) {
        if (parameters == null) {
            parameters = this.GetDialogParameters();
        }

        if (GameState.HasSeenDialog(dialogId)) {
            this.dialogBubble.SetDialogId(dialogId, parameters);
        } else {
            GameState.SeeDialog(dialogId);
            Camera.main.GetComponent<CameraFocuser>().EnqueueEvent(this.transform, dialogId, parameters);
        }
    }

    public Sprite GetSprite(Type type) {
        foreach (SpriteTemplate template in this.spriteTemplates) {
            if (template.type == type) {
                return template.sprite;
            }
        }

        return null;
    }

    void OnTriggerStay2D(Collider2D other) {
        Unit otherUnit = other.GetComponent<Unit>();
        if (otherUnit != null && otherUnit.type == Type.Bard && this.type != Type.Bard) {
            float distance = (this.transform.position - other.transform.position).magnitude;
            if (distance < INTERACT_DISTANCE) {
                otherUnit.animator.SetTrigger("BardSing");
                foreach (Story story in otherUnit.heardStories) {
                    this.HearStory(story);
                }
            }
        }
    }

    public void MouseUp() {
        if (this.type == Type.Bard) {
            this.SetTargetTown(Town.GetTown("mission"));
            this.ShowDialog("bard_return");
        }
    }

    public void CleanUp() {
        if (!this.isKilled) {
            this.StoreState();
        }
    }

    private void InitializeAnimations() {
        if (this.type == Type.Bard) {
            if (this.targetTown == null && this.GetNearestTown().townId != "mission") {
                this.animator.Play("BardDance");
            }
            animator.SetBool("IsBard", true);
        }
    }

    private void StoreState() {
        this.state.id = this.id;
        this.state.position = this.transform.position;
        this.state.type = this.type;
        this.state.heardStories = this.heardStories.ToArray();
        this.state.mode = this.mode;
        this.state.speed = this.speed;
        print("Next Town: " + this.nextTown + " Target Town: " + this.targetTown);
        if (this.nextTown != null) {
            this.state.nextTown = this.nextTown.townId;
        } else {
            this.state.nextTown = null;
        }
        if (this.targetTown != null) {
            this.state.targetTown = this.targetTown.townId;
        } else {
            this.state.targetTown = null;
        }
        GameState.StorePerson(this.state);
    }

    public void LoadState(GameState.PersonState state) {
        this.state = state;
        Vector3 position;
        if (state.placeAtTown != null) {
            print("Placing at town: " + state.placeAtTown);
            position = Town.GetTown(state.placeAtTown).transform.position;
            state.placeAtTown = null;
        } else {
            position = state.position;
        }
        this.transform.position = position;

        this.id = state.id;
        this.type = state.type;
        this.mode = state.mode;
        this.speed = state.speed;
        this.heardStories = new List<Story>(this.state.heardStories);
        if (this.state.nextTown != null) {
            this.nextTown = Town.GetTown(state.nextTown);
        }
        if (this.state.targetTown != null) {
            this.SetTargetTown(Town.GetTown(state.targetTown));
        } else {
            this.currentTown = this.GetNearestTown();
        }

        this.mainSprite.sprite = this.GetSprite(this.type);

        //For bard fadeout
        if (this.type == Type.Bard && this.currentTown == this.targetTown && this.currentTown.townId == "mission") {
            StartCoroutine(FadeOut());
        }
        this.InitializeAnimations();
    }

    private PersonInfoBox FindPersonInfoBox() {
        return GameObject.FindObjectOfType(typeof(PersonInfoBox)) as PersonInfoBox;
    }

    public void MouseEnter() {
        if (!Dialog.InDialog()) {
            PersonInfoBox tib = this.FindPersonInfoBox();
            tib.SetPerson(this);
            tib.SetVisibility(true);
        }
    }

    public void MouseExit() {
        PersonInfoBox tib = this.FindPersonInfoBox();
        tib.SetVisibility(false);
    }
}
