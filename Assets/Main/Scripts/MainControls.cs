using System.Linq;
using LookingGlass;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Receives input from hardware buttons and alters parameters (such as focus and depthiness)
 */
public class MainControls : MonoBehaviour {
  [SerializeField] public float focusSensitivity = .5f;
  [SerializeField] public float depthinessSensitivity = 1f;

  public Material frameMaterial;
  public Framer framer;
  public VideoControls videoControls;
  public CornerMessage cornerMessage;
  public FrameTextures frameTextures;

  private static readonly int SHADER_DEPTHINESS = Shader.PropertyToID("_Depthiness");

  private enum Parameter {
    FOCUS,
    DEPTHINESS,
    SEEK
  }

  private readonly Parameter[] CYCLE_PARAMETERS = {
    Parameter.DEPTHINESS,
    Parameter.FOCUS,
    Parameter.SEEK
  };

  private int cycleParameterIndex;
  private int cycleParametersLength;

  private void Start() {
    cycleParametersLength = CYCLE_PARAMETERS.Count();

    var focus = PlayerPrefs.GetFloat(Parameter.FOCUS.ToString(), 40);
    framer.TranslateFrame(focus);

    var depthiness = PlayerPrefs.GetFloat(Parameter.DEPTHINESS.ToString(), 100);
    frameMaterial.SetFloat(SHADER_DEPTHINESS, depthiness);
  }

  private void OnDestroy() {
    PlayerPrefs.Save();
  }

  private void Update() {
    if (InputManager.GetButtonUp(HardwareButton.PlayPause)) {
      CycleReceived();
    }

    if (Input.GetKeyUp(KeyCode.Return)) {
      CycleReceived();
    }

    if (Input.GetKeyUp(KeyCode.Space)) {
      TogglePlayPause();
    }

    if (Input.GetKeyUp(KeyCode.D)) {
      frameTextures.ToggleShowDepthAsMainTexture();
    }

    if (Input.GetKeyUp(KeyCode.Tab)) {
      if (videoControls == null) {
        return;
      }
      FilePicker.LoadVideoFile(videoControls);
    }

    if (Input.GetKeyUp(KeyCode.Escape)) {
      SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
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
      DirectionReceived(curParam, true);
    }

    if (Input.GetKey(KeyCode.LeftArrow)) {
      DirectionReceived(curParam, false);
    }

    if (Input.GetKey(KeyCode.Keypad6)) {
      DirectionReceived(Parameter.DEPTHINESS, true);
    }

    if (Input.GetKey(KeyCode.Keypad4)) {
      DirectionReceived(Parameter.DEPTHINESS, false);
    }

    if (Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.UpArrow)) {
      DirectionReceived(Parameter.FOCUS, true);
    }

    if (Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.DownArrow)) {
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
      case Parameter.SEEK:
        SeekVideo(forward);
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
    if (newDepthiness < 0) {
      newDepthiness = 0;
    }
    frameMaterial.SetFloat(SHADER_DEPTHINESS, newDepthiness);
    PlayerPrefs.SetFloat(Parameter.DEPTHINESS.ToString(), newDepthiness);
    ShowParameter(Parameter.DEPTHINESS, newDepthiness);
  }

  private void SeekVideo(bool forward) {
    if (videoControls == null) {
      return;
    }
    videoControls.Seek(forward);
  }

  private void TogglePlayPause() {
    if (videoControls == null) {
      return;
    }
    videoControls.TogglePlayPause();
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
