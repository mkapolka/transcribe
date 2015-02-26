using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoryUI : MonoBehaviour {

    public StoryButton buttonPrefab;
    public Transform buttonParent;
    public GameObject nextButton;
    public GameObject speechBubble;

	// Use this for initialization
	void Start () {
	    this.SetStories(GameState.GetKnownStories());
	}
	
	private void SetStories(Story[] stories) {
        foreach (Story story in stories) {
            StoryButton button = GameObject.Instantiate(this.buttonPrefab) as StoryButton;
            button.SetSpeechBubble(this.speechBubble);
            button.transform.SetParent(this.buttonParent, false);
            button.SetStory(story);
        }
        this.speechBubble.SetActive(false);
    }

    private Story[] GetSelectedStories() {
        StoryButton[] buttons = GameObject.FindObjectsOfType(typeof(StoryButton)) as StoryButton[];
        List<Story> stories = new List<Story>();
        foreach (StoryButton button in buttons) {
            if (button.IsSelected()) {
                stories.Add(button.GetStory());
            }
        }
        return stories.ToArray();
    }

    public void NextButtonPressed() {
        //Add the bard to the game
        if (GameState.availableBards.Count > 0) {
            string id = GameState.availableBards[0];
            GameState.availableBards.RemoveAt(0);
            GameState.PersonState bard = GameState.InstantiatePersonFromTemplate(id, id);
            bard.placeAtTown = "mission";
            bard.heardStories = this.GetSelectedStories();
            bard.targetTown = GameState.targetTown.id;
            GameState.StorePerson(bard);
            print("Bards remaining: " + string.Join(", ", GameState.availableBards.ToArray()));
        }
        Application.LoadLevel("Map");
    }
}
