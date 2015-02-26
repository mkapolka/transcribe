using UnityEngine;
using System.Collections;

public class DialogBubble : MonoBehaviour {

    public string dialogId;
    public SpriteRenderer bubbleSprite;
    public Sprite newSprite;
    public Sprite oldSprite;

    public void Start() {
        this.UpdateSprite();
    }

    public void SetDialogId(string dialogId) {
        this.dialogId = dialogId;
        this.UpdateSprite();
        this.gameObject.SetActive(true);
    }

    private void UpdateSprite() {
        if (GameState.HasSeenDialog(this.dialogId)) {
            this.bubbleSprite.sprite = this.oldSprite;
        } else {
            this.bubbleSprite.sprite = this.newSprite;
        }
    }

	public void OnMouseUp() {
        this.ShowDialog();
        this.gameObject.SetActive(false);
    }

    public void ShowDialog() {
        GameState.DialogTemplate template = GameState.GetDialogTemplate(this.dialogId);
        Dialog.instance.ShowDialog(template.lines);
        GameState.SeeDialog(this.dialogId);
    }
}
