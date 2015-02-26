using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TownInfoBox : MonoBehaviour {

	public Text townName;
    public Text[] storyNames;
    public RectTransform bounds;

    public void Start() {
        this.SetVisibility(false);
    }

    public void SetTown(Town town) {
        this.townName.text = town.townName;
        this.UpdateStoryTexts(town.book);
    }

    public void SetVisibility(bool visible) {
        this.GetComponent<Canvas>().enabled = visible;
    }

    public void Update() {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = this.transform.position.z;
        this.SetPosition(mousePosition);
    }

    private void UpdateStoryTexts(Book book) {
        foreach (Text text in this.storyNames) {
            text.text = "";
        }
        int i = 0;
        foreach (Story story in book.stories) {
            this.storyNames[i].text = story.title;
            i++;
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
