using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PersonInfoBox : MonoBehaviour {

	public Text personName;
    public Text personTitle;
    public Text[] storyNames;
    public RectTransform bounds;

    public void Start() {
        this.SetVisibility(false);
    }

    public void SetPerson(Unit unit) {
        this.personName.text = unit.GetName();
        this.personTitle.text = unit.GetTitle();
        this.UpdateStoryTexts(unit);
    }

    public void SetVisibility(bool visible) {
        this.GetComponent<Canvas>().enabled = visible;
    }

    public void Update() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = this.transform.position.z;
        this.SetPosition(mousePosition);
    }

    private void UpdateStoryTexts(Unit unit) {
        foreach (Text text in this.storyNames) {
            text.text = "";
        }
        int i = 0;
        foreach (Story story in unit.heardStories) {
            if (this.storyNames.Length > i) {
                this.storyNames[i].text = story.title;
                i++;
            }
        }
    }

    public void SetPosition(Vector3 position) {
        RectTransform transform = this.GetComponent<RectTransform>();
        //Initial pivot
        transform.pivot = new Vector2(.5f, 0f);

        Vector3[] vectors = new Vector3[4];
        transform.GetWorldCorners(vectors);
        Vector3 myULCorner = vectors[1];
        bounds.GetWorldCorners(vectors);
        Vector3 theirULCorner = vectors[1];
        if (myULCorner.y > theirULCorner.y) {
            transform.pivot = new Vector2(.5f, 1f);
        } else {
            transform.pivot = new Vector2(.5f, 0f);
        }
        this.transform.position = position;
    }
}
