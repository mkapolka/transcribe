using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoryButton : MonoBehaviour {

    private Story story;
    private CanvasGroup speechBubble;
    private Text speechBubbleText;
    public Text buttonText;
    private bool inBook = false;

    public void SetSpeechBubble(GameObject bubble) {
        this.speechBubble = bubble.GetComponent<CanvasGroup>();
        this.speechBubbleText = bubble.GetComponentInChildren<Text>();
    }

	public void SetStory(Story story) {
        this.buttonText.text = story.title;
        this.story = story;
    }

    public Story GetStory() {
        return this.story;
    }

    public bool IsSelected() {
        return this.inBook;
    }

    public void OnPressed() {
        if (this.inBook) {
            Transform storyPanel = GameObject.Find("ThoughtPanel").transform;
            this.transform.SetParent(storyPanel);
            this.inBook = false;
        } else {
            Transform storyPanel = GameObject.Find("StoryPanel").transform;
            this.transform.SetParent(storyPanel);
            this.inBook = true;
        }
    }

    public void SetSpeechVisibility(bool isVisible) {
        // this.speechBubble.alpha = isVisible ? 1.0f : 0.0f;
        this.speechBubble.gameObject.SetActive(isVisible);
    }

    public void OnMouseEnter() {
        this.SetSpeechVisibility(true);
        this.speechBubbleText.text = story.description;
    }

    public void OnMouseExit() {
        this.SetSpeechVisibility(false);
    }
}
