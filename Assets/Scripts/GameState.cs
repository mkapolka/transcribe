using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public static bool isInitialized = false;

    public static Story[] allStories;
    public static PersonTemplate[] personTemplates;
    public static DialogTemplate[] dialogTemplates;

    public static TownState[] townStates;
    public static List<PersonState> personStates = new List<PersonState>();
    public static List<Story> knownStories;
    public static List<string> seenDialogIds = new List<string>();
    public static List<string> availableBards = new List<string>(new string[]{"bard1", "bard2", "bard3"});
    public static string goblinTargetTown = null;
    public static bool goblinsKilled = false;
    public static bool hasSpawnedWarrior = false;
    public static bool hasSpawnedAdventurer = false;
    public static RingState ringState;

    public static TownState targetTown;

    public void Start() {
        GameState.InitializeState();
    }

    public static void LoadScene(string sceneName) {
        GameState.Cleanup();
        Application.LoadLevel(sceneName);
    }

    public static void Cleanup() {
        Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
        foreach (Unit unit in units) {
            unit.CleanUp();
        }
        Town[] towns = GameObject.FindObjectsOfType(typeof(Town)) as Town[];
        foreach (Town town in towns) {
            town.Cleanup();
        }
        Goblins goblins = GameObject.FindObjectOfType(typeof(Goblins)) as Goblins;
        goblins.Cleanup();
    }

    public static void InitializeState() {
        if (!GameState.isInitialized) {
            string jsonString = (Resources.Load("data") as TextAsset).text;
            JSONObject dataJson = new JSONObject(jsonString);
            GameState.InitializeTowns(dataJson);
            GameState.InitializeStories(dataJson);
            GameState.InitializeKnownStories(dataJson);
            GameState.InitializePersonTemplates(dataJson);
            GameState.InitializeDialogTemplates(dataJson);
            GameState.InitializePeople(GameState.personTemplates);
            GameState.InitializeRing(dataJson);

            GameState.isInitialized = true;
        }
    }

    public static void InitializeTowns(JSONObject data) {
        List<TownState> townStates = new List<TownState>();
        foreach (JSONObject townJson in data["towns"].list) {
            TownState state = new TownState();
            state.id = townJson["id"].str;
            state.townName = townJson["name"].str;
            townStates.Add(state);
        }
        GameState.townStates = townStates.ToArray();
    }

    public static void InitializeStories(JSONObject data) {
        List<Story> stories = new List<Story>();
        foreach (JSONObject obj in data["stories"].list) {
            string id = obj["id"].str;
            string title = obj["title"].str;
            string description = obj["description"].str;
            Story story = new Story(id, title, description);
            stories.Add(story);
        }
        GameState.allStories = stories.ToArray();
    }

    public static void InitializeKnownStories(JSONObject data) {
        List<Story> knownStories = new List<Story>();
        foreach (JSONObject obj in data["known_stories"].list) {
            Story story = GameState.GetStory(obj.str);
            knownStories.Add(story);
        }
        GameState.knownStories = knownStories;
    }

    public static void InitializePersonTemplates(JSONObject data) {
        List<PersonTemplate> people = new List<PersonTemplate>();
        foreach (JSONObject templateJson in data["people"].list) {
            PersonTemplate template = new PersonTemplate();
            template.id = templateJson["id"].str;
            template.name = templateJson["name"].str;
            template.title = templateJson["title"].str;
            template.portraitId = templateJson["portraitId"].str;
            template.type = (Unit.Type) Unit.Type.Parse(typeof(Unit.Type), templateJson["type"].str);
            template.speed = templateJson["speed"].f;
            if (templateJson["initial_town"] != null) {
                template.initialTown = templateJson["initial_town"].str;
            }
            people.Add(template);
        }
        GameState.personTemplates = people.ToArray();
    }

    public static void InitializeDialogTemplates(JSONObject data) {
        List<DialogTemplate> dialogs = new List<DialogTemplate>();
        foreach (JSONObject templateJson in data["dialogs"].list) {
            DialogTemplate template = new DialogTemplate();
            template.id = templateJson["id"].str;
            List<string> lines = new List<string>();
            foreach (JSONObject lineJson in templateJson["texts"].list) {
                lines.Add(lineJson.str);
            }
            template.lines = lines.ToArray();
            dialogs.Add(template);
        }
        GameState.dialogTemplates = dialogs.ToArray();
    }

    public static void InitializePeople(PersonTemplate[] people) {
        foreach (PersonTemplate template in people) {
            if (template.initialTown != null && template.initialTown != "") {
                PersonState person = GameState.InstantiatePersonFromTemplate(template, template.id);
                person.placeAtTown = template.initialTown;
                person.targetTown = template.initialTown;
                GameState.StorePerson(person);
            }
        }
    }

    public static void InitializeRing(JSONObject data) {
        GameState.ringState = new RingState();
        GameState.ringState.locationType = RingState.LocationType.Town;
        GameState.ringState.location = "hanging_tree";
    }

    public static DialogTemplate GetDialogTemplate(string dialogId) {
        if (!GameState.isInitialized) {
            GameState.InitializeState();
        }

        foreach (DialogTemplate template in GameState.dialogTemplates) {
            if (template.id == dialogId) {
                return template;
            }
        }
        throw new System.Exception("Couldn't find DialogTemplate with dialogId " + dialogId);
    }

    public static void ShowDialog(string dialogId, Dictionary<string, string> parameters = null) {
        DialogTemplate template = GameState.GetDialogTemplate(dialogId);
        Dialog.instance.ShowDialog(template.lines, parameters);
    }

    public static TownState GetTownState(string townId) {
        if (!GameState.isInitialized) {
            GameState.InitializeState();
        }

        foreach (TownState state in GameState.townStates) {
            if (state.id == townId) {
                return state;
            }
        }
        //throw new System.Exception("Couldn't find TownState with townId " + townId);
        TownState output = new TownState();
        output.id = townId;
        return output;
    }

    public static void SetTownState(TownState townState) {
        for (int i = 0; i < GameState.townStates.Length; i++) {
            if (GameState.townStates[i].id == townState.id) {
                GameState.townStates[i] = townState;
            }
        }
    }

    public static Story GetStory(string storyId) {
        foreach (Story story in GameState.allStories) {
            if (story.id == storyId) {
                return story;
            }
        }
        throw new System.Exception("Couldn't find Story with storyId " + storyId);
    }

    public static Story[] GetKnownStories() {
        if (!GameState.isInitialized) {
            GameState.InitializeState();
        }
        return GameState.knownStories.ToArray();
    }

    public static void AddKnownStory(Story story) {
        GameState.knownStories.Add(story);
    }

    public static bool KnowsStory(Story story) {
        return GameState.KnowsStory(story.id);
    }

    public static bool KnowsStory(string storyId) {
        foreach (Story story in GameState.knownStories) {
            if (story.id == storyId) {
                return true;
            }
        }
        return false;
    }

    public static bool HasPerson(string personId) {
        foreach (PersonState person in GameState.personStates) {
            if (person.id == personId) {
                return true;
            }
        }
        return false;
    }

    public static void StorePerson(PersonState state) {
        for (int i = 0; i < GameState.personStates.Count; i++) {
            if (GameState.personStates[i].id == state.id) {
                GameState.personStates[i] = state;
                return;
            }
        }
        GameState.personStates.Add(state);
    }

    public static void RemovePerson(PersonState state) {
        print("Trying to remove " + state.id);
        PersonState toRemove = null;
        foreach (PersonState pState in GameState.personStates) {
            if (pState.id == state.id) {
                toRemove = pState;
            }
        }
        if (toRemove != null) {
            print("Removing " + toRemove.id);
            GameState.personStates.Remove(toRemove);
        }
    }

    public static PersonTemplate GetPersonTemplate(string templateId) {
        foreach (PersonTemplate template in GameState.personTemplates) {
            if (template.id == templateId) {
                return template;
            }
        }
        throw new System.Exception("Can't find person template with ID " + templateId);
    }

    public static PersonState InstantiatePersonFromTemplate(string templateId, string id = null) {
        PersonTemplate template = GameState.GetPersonTemplate(templateId);
        return GameState.InstantiatePersonFromTemplate(template, id);
    }

    public static PersonState InstantiatePersonFromTemplate(PersonTemplate template, string id = null) {
        PersonState state = new PersonState();
        state.id = (id == null) ? Random.Range(0, int.MaxValue).ToString() : id;
        state.type = template.type;
        state.name = template.name;
        state.title = template.title;
        state.speed = template.speed;
        state.portraitId = template.portraitId;
        return state;
    }

    public static bool HasSeenDialog(string dialogId) {
        return GameState.seenDialogIds.Contains(dialogId);
    }

    public static void SeeDialog(string dialogId) {
        if (!GameState.seenDialogIds.Contains(dialogId)) {
            GameState.seenDialogIds.Add(dialogId);
        }
    }

	public class TownState {
        public string id;
        public string townName;
        public bool doorOpen = false;
    }

    public class PersonState {
        public string id;
        public string title;
        public string name;
        public string portraitId;
        public Unit.Type type;
        public Unit.Mode mode;
        public float speed;

        public Vector3 position;
        public string targetTown;
        public string nextTown;

        public string placeAtTown;

        public Story[] heardStories = new Story[0];

        public Vector3 GetPosition() {
            if (this.placeAtTown != null) {
                return Town.GetTown(this.placeAtTown).transform.position;
            } else {
                return this.position;
            }
        }
    }

    public class PersonTemplate {
        public string id;
        public string name;
        public string title;
        public string spriteId;
        public string portraitId;
        public Unit.Type type;
        public string initialTown;
        public float speed;
    }

    public class DialogTemplate {
        public string id;
        public string[] lines;
    }

    public class RingState {
        public enum LocationType {
            Person, Town
        };
        public LocationType locationType;
        public string location;
    }
}
