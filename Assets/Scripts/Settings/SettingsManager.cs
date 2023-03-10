using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public SettingsObject Settings;

    private float noteSpeed;

    private void Awake() {
        Instance = this;
        EventManager.updateBPMEvent += UpdateNoteSpeed;
    }

    public float GetNoteSpeed{
        get {return noteSpeed;}
    }

    public float GetNoteHight{
        get {return Settings.QuarterNoteHight;}
    }

    public float StartDistanceFromKey {
        get {return Settings.startDistanceFromKey;}
        set {Settings.startDistanceFromKey = value;}
    }

    //note prefabs
    public GameObject WhiteNotePrefab(byte _channel) {
        return Settings.channels[_channel].whiteNotePrefab;
    }
    public void SetWhiteNotePrefab(byte _channel, GameObject _newPrefab) {
        Settings.channels[_channel].whiteNotePrefab = _newPrefab;
    }

    public GameObject BlackNotePrefab(byte _channel) {
        return Settings.channels[_channel].blackNotePrefab;
    }
    public void SetBlackNotePrefab(byte _channel, GameObject _newPrefab) {
        Settings.channels[_channel].blackNotePrefab = _newPrefab;
    }


    public void UpdateNoteSpeed(float _newSpeed) {
        noteSpeed = _newSpeed;
    }
}
