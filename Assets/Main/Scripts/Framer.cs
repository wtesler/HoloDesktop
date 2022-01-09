using LookingGlass;
using UnityEngine;

public class Framer : MonoBehaviour {
  [Header("Scaling")] public float aspectRatio = 3 / 4f;
  public Vector2 minMaxHeight;

  [Header("HoloPlay")] public Holoplay holoplay;

  private const float MARGIN = .01f;

  public float TranslateFrame(float amountToMove) {
    var t = transform;
    var p = t.position;

    var farExtent = holoplay.farClipFactor - MARGIN;
    var nearExtent = -holoplay.nearClipFactor + MARGIN;

    p = new Vector3(p.x, p.y, Mathf.Clamp(p.z + amountToMove, nearExtent, farExtent));

    t.position = p;

    Rescale();

    return p.z;
  }

  public void LerpFrame(float progress) {
    var t = transform;
    var p = t.position;

    var nearExtent = -holoplay.nearClipFactor + MARGIN;
    var farExtent = holoplay.farClipFactor - MARGIN;

    var a = -.4f;
    var b = .4f;

    p = new Vector3(p.x, p.y, Mathf.Lerp(a, b, progress));

    t.position = p;

    Rescale();
  }

  private void Rescale() {
    var t = transform;
    var p = t.position;

    var farExtent = holoplay.farClipFactor - MARGIN;
    var nearExtent = -holoplay.nearClipFactor + MARGIN;

    var progress = (p.z - nearExtent) / (farExtent - nearExtent);;

    var scaleY = Mathf.Lerp(minMaxHeight.x, minMaxHeight.y, progress);
    var scaleX = scaleY * aspectRatio;

    t.localScale = new Vector3(scaleX, scaleY, t.localScale.z);
  }
}
