using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState : MonoBehaviour {

    public static bool isInitialized = false;
    public static TownState[] townStates;
    public static List<PersonState> personStates = new List<PersonState>();
    public static Story[] knownStories;
    public static Story[] allStories;
    public static PersonTemplate[] personTemplates;
    public static DialogTemplate[] dialogTemplates;
    public static List<string> seenDialogIds = new List<string>();
    public static List<string> availableBards = new List<string>(new string[]{"bard1", "bard2", "bard3"});

    public static bool hasSpawnedWarrior = false;

    public static TownState targetTown;

    public void Start() {
        if (!GameState.isInitialized) {
            GameState.InitializeState();
        }
    }

    public static void LoadScene(string sceneName) {
        Unit[] units = GameObject.FindObjectsOfType(typeof(Unit)) as Unit[];
        foreach (Unit unit in units) {
            unit.CleanUp();
        }
        Application.LoadLevel(sceneName);
    }

    public static void InitializeState() {
        string jsonString = (Resources.Load("data") as TextAsset).text;
        JSONObject dataJson = new JSONObject(jsonString);
        GameState.InitializeTowns(dataJson);
        GameState.InitializeStories(dataJson);
        GameState.InitializeKnownStories(dataJson);
        GameState.InitializePersonTemplates(dataJson);
        GameState.InitializeDialogTemplates(dataJson);

        GameState.isInitialized = true;
    }

    public static void InitializeTowns(JSONObject data) {
        List<TownState> townStates = new List<TownState>();
        foreach (JSONObject townJson in data["towns"].list) {
            TownState state = new TownState();
            state.id = townJson["id"].str;
            state.townName = townJson["name"].str;
            state.book = new Book(new Story[0]);
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
        GameState.knownStories = knownStories.ToArray();
    }

    public static void InitializePersonTemplates(JSONObject data) {
        List<PersonTemplate> people = new List<PersonTemplate>();
        foreach (JSONObject templateJson in data["people"].list) {
            PersonTemplate template = new PersonTemplate();
            template.id = templateJson["id"].str;
            template.name = templateJson["name"].str;
            template.portraitId = templateJson["portraitId"].str;
            template.type = (Unit.Type) Unit.Type.Parse(typeof(Unit.Type), templateJson["type"].str);
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

    public static void ShowDialog(string dialogId) {
        DialogTemplate template = GameState.GetDialogTemplate(dialogId);
        Dialog.instance.ShowDialog(template.lines);
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
        throw new System.Exception("Couldn't find TownState with townId " + townId);
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
        return GameState.knownStories;
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
        public Book book;
    }

    public class PersonState {
        public string id;
        public string name;
        public Unit.Type type;

        public Vector3 position;
        public string targetTown;
        public string nextTown;

        public string placeAtTown;

        public Story[] heardStories = new Story[0];
    }

    public class PersonTemplate {
        public string id;
        public string name;
        public string spriteId;
        public string portraitId;
        public Unit.Type type;
    }

    public class DialogTemplate {
        public string id;
        public string[] lines;
    }
}
