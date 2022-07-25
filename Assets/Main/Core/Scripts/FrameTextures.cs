using LookingGlass;
using UnityEngine;

public class FrameTextures : MonoBehaviour {
  public Material frameMaterial;

  public Holoplay holoplay;

  // [SerializeField]
  // public RenderTexture frameTexture;
  //
  // [SerializeField]
  // public RenderTexture frameDepthTexture;

  private RenderTexture frameTexture;
  private RenderTexture frameDepthTexture;

  private static readonly int MAIN_TEX_SHADER = Shader.PropertyToID("_MainTex");
  private static readonly int DEPTH_TEX_SHADER = Shader.PropertyToID("_DepthTex");
  private static readonly int SHOW_DEPTH_SHADER = Shader.PropertyToID("_ShowDepth");

  private bool isShowingDepth;
  private bool isReady;

  private void Awake() {
    holoplay.onHoloplayReady.AddListener(_OnHoloplayReady);
  }

  private void OnDestroy() {
    frameTexture.Release();
    frameDepthTexture.Release();
    holoplay.onHoloplayReady.RemoveListener(_OnHoloplayReady);
  }

  public RenderTexture GetFrameTexture() {
    return frameTexture;
  }

  public RenderTexture GetFrameDepthTexture() {
    return frameDepthTexture;
  }

  public float GetAspectRatio() {
    return frameTexture.width / (float) frameTexture.height;
  }

  public void ToggleShowDepthAsMainTexture() {
    isShowingDepth = !isShowingDepth;
    frameMaterial.SetTexture(MAIN_TEX_SHADER, isShowingDepth ? frameDepthTexture : frameTexture);
    frameMaterial.SetInt(SHOW_DEPTH_SHADER, isShowingDepth ? 1 : 0);
  }

  public bool IsReady() {
    return isReady;
  }

  private void _OnHoloplayReady(LoadResults results) {
    int width = holoplay.cal.screenWidth;
    int height = holoplay.cal.screenHeight;

    frameTexture = new RenderTexture(width, height, 0, DepthModel.INPUT_TEXTURE_FORMAT, 0);
    frameDepthTexture = new RenderTexture(width, height, 0, DepthModel.OUTPUT_TEXTURE_FORMAT, 0);

    frameMaterial.SetTexture(MAIN_TEX_SHADER, frameTexture);
    frameMaterial.SetTexture(DEPTH_TEX_SHADER, frameDepthTexture);

    frameMaterial.SetInt(SHOW_DEPTH_SHADER, 0); // Don't show depth at start.

    isReady = true;
  }
}
