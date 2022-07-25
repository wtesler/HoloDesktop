using LookingGlass;
using UnityEngine;

public class Framer : MonoBehaviour {
  // [Header("Scaling")]
  // [SerializeField]
  // public float aspectRatio = 3 / 4f;
  public Vector2 minMaxHeight;

  [Header("Ranging")]
  [SerializeField] public Vector2 focusRange = new Vector2(-1.5f, 0);
  [SerializeField] public float startFocus = -.35f;

  [Header("HoloPlay")] public Holoplay holoplay;

  private const float MARGIN = .01f;

  private float aspectRatio;

  private void Awake() {
    holoplay.onHoloplayReady.AddListener(_OnHoloplayReady);
  }

  private void OnDestroy() {
    holoplay.onHoloplayReady.RemoveListener(_OnHoloplayReady);
  }

  public float TranslateFrame(float amountToMove) {
    var t = transform;
    var p = t.position;

    var farExtent = holoplay.farClipFactor - MARGIN;
    var nearExtent = -holoplay.nearClipFactor + MARGIN;

    var newZ = Mathf.Clamp(p.z + amountToMove, nearExtent, farExtent);
    newZ = Mathf.Clamp(newZ, focusRange.x, focusRange.y);

    p = new Vector3(p.x, p.y, newZ);

    t.position = p;

    Rescale();

    return p.z;
  }

  private void Rescale() {
    var t = transform;
    var p = t.position;

    var farExtent = holoplay.farClipFactor - MARGIN;
    var nearExtent = -holoplay.nearClipFactor + MARGIN;

    var progress = (p.z - nearExtent) / (farExtent - nearExtent);

    var scaleY = Mathf.Lerp(minMaxHeight.x, minMaxHeight.y, progress);
    var scaleX = scaleY * aspectRatio;

    t.localScale = new Vector3(scaleX, scaleY, t.localScale.z);
  }

  private void _OnHoloplayReady(LoadResults results) {
    int width = holoplay.cal.screenWidth;
    int height = holoplay.cal.screenHeight;

    aspectRatio = ((float) width) / height;

    var focus = PlayerPrefs.GetFloat(Parameter.FOCUS.ToString(), startFocus);
    TranslateFrame(focus);
  }
}
