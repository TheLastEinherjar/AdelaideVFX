using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScaleToNote : MonoBehaviour
{
    public float notePrefabHight;
    public float ExtraLightEdge;

    [SerializeField]
    private light2DToScale[] lights;

    [SerializeField]
    private slicedRenderer[] slicedSprites;
    //private SlicedSpriteToScale[] SlicedSprites;

    private void Awake() {
        foreach (slicedRenderer sprite in slicedSprites)
        {
            sprite.startSize = sprite.spriteRenderer.size;
        }
        foreach (light2DToScale light in lights)
        {
            if (light.light.lightType == Light2D.LightType.Point) {
                light.startSize = light.light.pointLightOuterRadius;
            } else if (light.light.lightType == Light2D.LightType.Freeform) {
                light.startSize = light.light.transform.localScale.y;
            }
        }
    }

    public void setYScale(float _newSize) {
        Debug.Log(_newSize);
        foreach (light2DToScale light in lights)
        {
            if (light.light.lightType == Light2D.LightType.Point) {
                light.light.pointLightOuterRadius = (light.startSize/notePrefabHight * _newSize/2) + ExtraLightEdge;
            } else if (light.light.lightType == Light2D.LightType.Freeform) {
                light.light.transform.localScale = new Vector3(light.light.transform.localScale.x, light.startSize/notePrefabHight * _newSize, light.light.transform.localScale.z);
            }
        }
        foreach (slicedRenderer sprite in slicedSprites)
        {
            sprite.spriteRenderer.size = new Vector2(sprite.spriteRenderer.size.x, sprite.startSize.y/notePrefabHight * _newSize);
        }
    }

    [System.Serializable]
    private class slicedRenderer {
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public Vector2 startSize;
    }

    [System.Serializable]
    private class light2DToScale{
        public Light2D light;

        [HideInInspector]
        public float startSize;
    }

}
