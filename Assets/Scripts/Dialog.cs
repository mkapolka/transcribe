using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Dialog : MonoBehaviour {
    private const float FORCE_STAY_LENGTH = 0.25f;

    public static Dialog instance;

    public Image leftPortrait;
    public Text textBox;
    
    private float forceStay;

    private List<string> dialogQueue;
    public UnityEvent onComplete;

    public void Start() {
        Dialog.instance = this;
    }

    public void ShowDialog(string[] dialogLines) {
        this.dialogQueue = new List<string>(dialogLines);
        this.PopDialog();
        this.SetVisible(true);
        this.forceStay = Dialog.FORCE_STAY_LENGTH;
    }

    private void PopDialog() {
        string nextValue = this.dialogQueue[0];
        this.dialogQueue.RemoveAt(0);
        this.SetDialog(nextValue);
    }

    private void SetDialog(string nextLine) {
        this.textBox.text = nextLine;
    }

    public void ClickNext() {
        if (this.forceStay < 0) {
            if (this.dialogQueue.Count > 0) {
                this.PopDialog();
            } else {
                this.SetVisible(false);
                this.onComplete.Invoke();
            }
        }
    }

    public void SetVisible(bool isVisible) {
        this.GetComponent<CanvasGroup>().alpha = isVisible ? 1 : 0;
        this.GetComponent<CanvasGroup>().interactable = isVisible;
        this.GetComponent<CanvasGroup>().blocksRaycasts = isVisible;
    }

    public void Update() {
        this.forceStay -= Time.deltaTime;
    }
}
