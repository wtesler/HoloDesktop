using System.Linq;
using LookingGlass;
using UnityEngine;
using UnityRawInput;

/**
 * Receives input from hardware buttons and alters parameters (such as focus and depthiness)
 */
public class MainControls : MonoBehaviour {
  [SerializeField] public float focusSensitivity = .02f;
  [SerializeField] public float depthinessSensitivity = 100000f;
  [SerializeField] public float startDepth = 10000000f;
  [SerializeField] public Vector2 depthRange = new Vector2(0, 25000000);

  public Material frameMaterial;
  public Framer framer;
  public CornerMessage cornerMessage;
  public FrameTextures frameTextures;

  private static readonly int SHADER_DEPTHINESS = Shader.PropertyToID("_Depthiness");

  private readonly Parameter[] CYCLE_PARAMETERS = {
    Parameter.DEPTHINESS,
    Parameter.FOCUS
  };

  private int cycleParameterIndex;
  private int cycleParametersLength;

  private bool isHoldingControl;

  private void Start() {
    RawKeyInput.Start(true);

    RawKeyInput.OnKeyUp += onRawKeyUp;
    RawKeyInput.OnKeyDown += onRawKeyDown;

    cycleParametersLength = CYCLE_PARAMETERS.Count();

    var depthiness = PlayerPrefs.GetFloat(Parameter.DEPTHINESS.ToString(), startDepth);
    frameMaterial.SetFloat(SHADER_DEPTHINESS, depthiness);
  }

  private void OnDestroy() {
    RawKeyInput.OnKeyUp -= onRawKeyUp;
    RawKeyInput.OnKeyDown -= onRawKeyDown;
    PlayerPrefs.Save();
    RawKeyInput.Stop();
  }

  private void Update() {
    if (InputManager.GetButtonUp(HardwareButton.PlayPause)) {
      CycleReceived();
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

    isHoldingControl = RawKeyInput.IsKeyDown(RawKey.LeftControl) || RawKeyInput.IsKeyDown(RawKey.RightControl);

    if (!isHoldingControl) {
      return; // EARLY RETURN AT THIS POINT IF CONTROL NOT HELD DOWN.
    }

    if (RawKeyInput.IsKeyDown(RawKey.Right) || RawKeyInput.IsKeyDown(RawKey.Numpad6)) {
      DirectionReceived(Parameter.DEPTHINESS, true);
    }

    if (RawKeyInput.IsKeyDown(RawKey.Left) || RawKeyInput.IsKeyDown(RawKey.Numpad4)) {
      DirectionReceived(Parameter.DEPTHINESS, false);
    }

    if (RawKeyInput.IsKeyDown(RawKey.Up) || RawKeyInput.IsKeyDown(RawKey.Numpad8)) {
      DirectionReceived(Parameter.FOCUS, true);
    }

    if (RawKeyInput.IsKeyDown(RawKey.Down) || RawKeyInput.IsKeyDown(RawKey.Numpad2)) {
      DirectionReceived(Parameter.FOCUS, false);
    }
  }

  private void onRawKeyUp(RawKey key) {
    if (isHoldingControl) {
      switch (key) {
        case RawKey.D:
          frameTextures.ToggleShowDepthAsMainTexture();
          break;
        case RawKey.F:
          ShowParameter(Parameter.FPS, 1 / Time.deltaTime);
          break;
      }
    }

  }

  private void onRawKeyDown(RawKey key) {

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
    ShowParameter(Parameter.FOCUS, newPosZ * 100);
  }

  private void ChangeDepthiness(bool forward) {
    var curDepthiness = frameMaterial.GetFloat(SHADER_DEPTHINESS);
    var amountToMove = depthinessSensitivity;
    amountToMove *= forward ? 1 : -1;
    var newDepthiness = curDepthiness + amountToMove;
    newDepthiness = Mathf.Clamp(newDepthiness, depthRange.x, depthRange.y);
    frameMaterial.SetFloat(SHADER_DEPTHINESS, newDepthiness);
    PlayerPrefs.SetFloat(Parameter.DEPTHINESS.ToString(), newDepthiness);
    ShowParameter(Parameter.DEPTHINESS, newDepthiness / 100000);
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
