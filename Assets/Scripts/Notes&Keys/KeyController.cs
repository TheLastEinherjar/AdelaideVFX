using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using AdelaideVFX;

public class KeyController : MonoBehaviour
{
    public byte KeyID = 0;
    private int notesBeingPlayed = 0;
    public bool isBaseColor = true;
    public bool IsWhiteKey = true;
    public Color BaseColor;
    public Color HighlightedColor;
    [SerializeField]
    private float noteRenderSize;
    private float noteDistanceFromKey;
    private GameObject prefab;
    private Light2D keyLight;

    private void Start() {
        EventManager.keyEvent += CreateNote;
        keyLight = GetComponentInChildren<Light2D>();
        updateColor();
        updateSettings();
    }

    private void updateColor(){
        keyLight.color = HighlightedColor;
    }

    private void changeColor(){
        if (isBaseColor) {
            gameObject.GetComponent<SpriteRenderer>().color = HighlightedColor;
            keyLight.enabled = true;
        } else {
            gameObject.GetComponent<SpriteRenderer>().color = BaseColor;
            keyLight.enabled = false;
        }
        isBaseColor = !isBaseColor;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.GetComponent<Note>() != null) {
            
            notesBeingPlayed++;
        }
    }

    private void PlayKey() {

    }
    
    private void CreateNote(MidiEvent _data){
        if ((byte)_data.Parameters[0] == KeyID) {
            updateSettings();
            GameObject newNote;
            if (IsWhiteKey) {
                newNote = Instantiate(SettingsManager.Instance.WhiteNotePrefab(_data.channel), new Vector3(transform.position.x, transform.position.y + noteDistanceFromKey, transform.position.z), Quaternion.identity);
            } else {
                newNote = Instantiate(SettingsManager.Instance.BlackNotePrefab(_data.channel), new Vector3(transform.position.x, transform.position.y + noteDistanceFromKey, transform.position.z), Quaternion.identity);
            }
            newNote.SendMessage("setYScale", (float)_data.Parameters[1]);
            //when debuging do you ever notice that you are checking things that are easiest to fix first, not the ones that are most likely broken
        }
    }

    private void updateSettings() {
        SettingsManager s = SettingsManager.Instance;
        noteDistanceFromKey = s.StartDistanceFromKey;
    }
    
    private void OnDisable() {
        EventManager.keyEvent -= CreateNote;
    }
}
