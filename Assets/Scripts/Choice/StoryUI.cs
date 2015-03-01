using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StoryUI : MonoBehaviour {
    public const int MAX_STORIES = 3;

    public StoryButton buttonPrefab;
    public Transform buttonParent;
    public GameObject nextButton;
    public GameObject speechBubble;
    public Text counterText;

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

    void Update() {
        this.UpdateCounter();
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

    public void UpdateCounter() {
        int stories = this.GetSelectedStories().Length;
        this.counterText.text = "Stoires: " + stories + "/" + MAX_STORIES;
        this.nextButton.SetActive(stories > 0);
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
    
    public void BackButtonPressed() {
        Application.LoadLevel("Map");
    }
}
