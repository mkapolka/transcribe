using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Door : Town {

    public Door otherSide;
    public bool isOpen;

    public void Start() {
        this.SyncConnectedList();
    }

    public void SyncConnectedList() {
        List<Town> listConnected = new List<Town>(this.connected);
        if (isOpen) {
            if (!listConnected.Contains(this.otherSide)) {
                listConnected.Add(this.otherSide);
            }
        } else {
            if (listConnected.Contains(this.otherSide)) {
                listConnected.Remove(this.otherSide);
            }
        }
        this.connected = listConnected.ToArray();
    }

    public void SetOpen(bool isOpen, bool callback = true) {
        this.isOpen = isOpen;
        this.SyncConnectedList();
        if (callback) {
            this.otherSide.SetOpen(isOpen, false);
        }
    }

    override public void StoreState() {
        this.state.doorOpen = this.isOpen;
        base.StoreState();
    }

    override public void InitializeState() {
        base.InitializeState();
        this.SetOpen(this.state.doorOpen, false);
    }
}
