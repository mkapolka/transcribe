using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

public class Dialog : MonoBehaviour {
    private const float SPRITE_MOVE_SPEED = 7.5f;
    private const float FORCE_STAY_LENGTH = 0.25f;

    public static Dialog instance;

    public class DialogInfo {
        public string[] lines;
        public Dictionary<string, string> parameters = new Dictionary<string, string>();
    }

    [System.Serializable]
    public class HideySprite {
        public Transform hiddenPosition;
        public Vector3 unhiddenPosition;
        public Transform target;
    }

    [System.Serializable]
    public class SpriteKey {
        public string key;
        public Sprite sprite;
    }

    public Image leftPortrait;
    public Text textBox;
    
    private float forceStay;

    private DialogInfo dialog;
    private List<string> dialogQueue;
    public UnityEvent onComplete;
    private bool visible = false;

    public HideySprite[] hiddenSprites;
    public SpriteKey[] spriteKeys;

    public static bool InDialog() {
        return Dialog.instance.visible;
    }

    public void Start() {
        Dialog.instance = this;
        foreach (HideySprite sprite in this.hiddenSprites) {
            sprite.unhiddenPosition = sprite.target.position;
            sprite.target.position = sprite.hiddenPosition.position;
        }
    }

    public void ShowDialog(DialogInfo dialog) {
        this.dialog = dialog;
        this.dialogQueue = new List<string>(this.dialog.lines);
        this.PopDialog();
        this.SetVisible(true);
        this.forceStay = Dialog.FORCE_STAY_LENGTH;
    }

    public void ShowDialog(string[] dialogLines, Dictionary<string, string> parameters = null) {
        if (parameters == null) {
            parameters = new Dictionary<string, string>();
        }
        DialogInfo dialog = new DialogInfo();
        dialog.lines = dialogLines;
        dialog.parameters = parameters;
        this.ShowDialog(dialog);
    }

    private void PopDialog() {
        string nextValue = this.dialogQueue[0];
        this.dialogQueue.RemoveAt(0);
        this.SetDialog(nextValue);
    }

    private void SetDialog(string nextLine) {
        nextLine = this.FormatCommandLine(nextLine, this.dialog.parameters);
        nextLine = this.ParseCommands(nextLine);
        this.textBox.text = nextLine;
    }

    private string FormatCommandLine(string line, Dictionary<string, string> parameters) {
        foreach (KeyValuePair<string, string> tuple in parameters) {
            line = line.Replace("{" + tuple.Key + "}", tuple.Value);
        }
        return line;
    }

    private string ParseCommands(string line) {
        MatchCollection matches = Regex.Matches(line, @"\[(.+?)\]\s*");
        foreach (Match match in matches) {
            //print(match.Value + " " + match.Groups[1].Value + " " + match.Index + " " + match.Length);
            this.ProcessCommand(match.Groups[1].Value);
        }
        int lastIndex = 0;
        if (matches.Count > 0) {
            Match lastMatch = matches[matches.Count-1];
            lastIndex = lastMatch.Index + lastMatch.Length;
        }
        //print(line.Substring(lastIndex));
        return line.Substring(lastIndex);
    }

    private void ProcessCommand(string commandString) {
        string[] command = commandString.Split(' ');
        switch (command[0]) {
            case "portrait":
                this.SetSprite(command[1]);
            break;

            case "spoken":
                this.SetSprite(this.dialog.parameters["speakerPortrait"]);
            break;
        }
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

    private Sprite GetSprite(string key) {
        foreach (SpriteKey spriteKey in this.spriteKeys) {
            if (spriteKey.key == key) {
                return spriteKey.sprite;
            }
        }
        throw new System.Exception("Can't find sprite with key " + key);
    }

    private void SetSprite(string spriteKey) {
        this.leftPortrait.sprite = this.GetSprite(spriteKey);
        this.leftPortrait.color = this.leftPortrait.sprite == null ? Color.clear : Color.white;
    }

    public void SetVisible(bool isVisible) {
        this.visible = isVisible;
        this.GetComponent<CanvasGroup>().interactable = isVisible;
        this.GetComponent<CanvasGroup>().blocksRaycasts = isVisible;
        StartCoroutine("MoveSprites");
    }

    public void Update() {
        this.forceStay -= Time.deltaTime;
    }

    private IEnumerator MoveSprites() {
        float f = 0;
        while (f < 1.0f) {
            f = Mathf.Clamp(f + Time.deltaTime * SPRITE_MOVE_SPEED, 0, 1);
            foreach (HideySprite sprite in this.hiddenSprites) {
                Vector3 startPosition = this.visible ? sprite.hiddenPosition.position : sprite.unhiddenPosition;
                Vector3 targetPosition = this.visible ? sprite.unhiddenPosition : sprite.hiddenPosition.position;
                Vector3 position = startPosition + (targetPosition - startPosition) * f;
                sprite.target.transform.position = position;
            }
            this.GetComponent<CanvasGroup>().alpha = this.visible ? f : (1 - f);
            yield return null;
        }
    }
}
