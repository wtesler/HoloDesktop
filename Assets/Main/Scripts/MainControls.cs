using System.Linq;
using LookingGlass;
using UnityEngine;

/**
 * Receives input from hardware buttons and alters parameters (such as focus and depthiness)
 */
public class MainControls : MonoBehaviour {
  [SerializeField] public float focusSensitivity = .5f;
  [SerializeField] public float depthinessSensitivity = 1f;

  public Material frameMaterial;
  public Framer framer;
  public CornerMessage cornerMessage;
  public FrameTextures frameTextures;
  public Vector2 depthRange = new Vector2(0, 30000);

  private static readonly int SHADER_DEPTHINESS = Shader.PropertyToID("_Depthiness");

  private enum Parameter {
    FOCUS,
    DEPTHINESS
  }

  private readonly Parameter[] CYCLE_PARAMETERS = {
    Parameter.DEPTHINESS,
    Parameter.FOCUS
  };

  private int cycleParameterIndex;
  private int cycleParametersLength;

  private void Start() {
    cycleParametersLength = CYCLE_PARAMETERS.Count();

    var focus = PlayerPrefs.GetFloat(Parameter.FOCUS.ToString(), -.35f);
    framer.TranslateFrame(focus);

    var depthiness = PlayerPrefs.GetFloat(Parameter.DEPTHINESS.ToString(), 11000);
    frameMaterial.SetFloat(SHADER_DEPTHINESS, depthiness);
  }

  private void OnDestroy() {
    PlayerPrefs.Save();
  }

  private void Update() {
    if (InputManager.GetButtonUp(HardwareButton.PlayPause)) {
      CycleReceived();
    }

    if (Input.GetKeyUp(KeyCode.D)) {
      frameTextures.ToggleShowDepthAsMainTexture();
    }
  }

  private void FixedUpdate() {
    Parameter curParam = CYCLE_PARAMETERS[cycleParameterIndex];

    if (InputManager.GetButton(HardwareButton.Forward)) {
      DirectionReceived(curParam, true);
    }

    if (InputManager.GetButton(HardwareButton.Back)) {
      DirectionReceived(curParam, false);
    }

    if (Input.GetKey(KeyCode.RightArrow)) {
      DirectionReceived(Parameter.DEPTHINESS, true);
    }

    if (Input.GetKey(KeyCode.LeftArrow)) {
      DirectionReceived(Parameter.DEPTHINESS, false);
    }

    if (Input.GetKey(KeyCode.UpArrow)) {
      DirectionReceived(Parameter.FOCUS, true);
    }

    if (Input.GetKey(KeyCode.DownArrow)) {
      DirectionReceived(Parameter.FOCUS, false);
    }
  }

  private void CycleReceived() {
    cycleParameterIndex++;
    if (cycleParameterIndex >= cycleParametersLength) {
      cycleParameterIndex = 0;
    }
    ShowParameter(CYCLE_PARAMETERS[cycleParameterIndex], null);
  }

  private void DirectionReceived(Parameter parameter, bool forward) {
    switch (parameter) {
      case Parameter.FOCUS:
        ChangeFocus(forward);
        break;
      case Parameter.DEPTHINESS:
        ChangeDepthiness(forward);
        break;
    }
  }

  private void ChangeFocus(bool forward) {
    var amountToMove = focusSensitivity;
    amountToMove *= forward ? 1 : -1;

    float newPosZ = framer.TranslateFrame(amountToMove);

    PlayerPrefs.SetFloat(Parameter.FOCUS.ToString(), newPosZ);
    ShowParameter(Parameter.FOCUS, newPosZ);
  }

  private void ChangeDepthiness(bool forward) {
    var curDepthiness = frameMaterial.GetFloat(SHADER_DEPTHINESS);
    var amountToMove = depthinessSensitivity;
    amountToMove *= forward ? 1 : -1;
    var newDepthiness = curDepthiness + amountToMove;
    newDepthiness = Mathf.Clamp(newDepthiness, depthRange.x, depthRange.y);
    frameMaterial.SetFloat(SHADER_DEPTHINESS, newDepthiness);
    PlayerPrefs.SetFloat(Parameter.DEPTHINESS.ToString(), newDepthiness);
    ShowParameter(Parameter.DEPTHINESS, newDepthiness);
  }

  private void ShowParameter(Parameter parameter, float? value) {
    var paramName = parameter.ToString();
    var text = $"{paramName}";
    if (value.HasValue) {
      var valueStr = value.Value.ToString("0.00");
      text += $": {valueStr}";
    }
    cornerMessage.ShowText(text);
  }
}
