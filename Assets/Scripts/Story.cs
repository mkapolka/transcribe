using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Story {
    public string id;
    public string title;
    public string description;

    public Story(string id, string title, string description) {
        this.id = id;
        this.title = title;
        this.description = description;
    }

    public static Story[] loadStories() {
        List<Story> stories = new List<Story>();
        string jsonString = (Resources.Load("data") as TextAsset).text;
        JSONObject storiesJson = new JSONObject(jsonString);
        foreach(JSONObject storyJson in storiesJson["stories"].list) {
            Story story = new Story(storyJson["id"].str, storyJson["title"].str, storyJson["description"].str);
            stories.Add(story);
        }
        return stories.ToArray();
    }
}
