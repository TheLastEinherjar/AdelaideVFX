using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Note : MonoBehaviour
{
    public bool isBlackNote = false;
    public bool changesToKeyColor = false;

    private float _noteSpeed;

    private Light2D noteLight;
    private void Awake() {
        noteLight = GetComponent<Light2D>();
    }

    private void Update() {
        transform.position -= new Vector3(0, SettingsManager.Instance.GetNoteSpeed * Time.deltaTime, 0);
    }

}
