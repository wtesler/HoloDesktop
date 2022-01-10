using uDesktopDuplication;
using UnityEngine;

/**
 * Sets the Desktop Duplication texture to our holo material.
 */
public class HolifyDesktop : MonoBehaviour {
  public Transform frameHolder;
  public FrameTextures frameTextures;
  public DepthModel depthModel;

  private Monitor monitor;
  private int lastMonitorId;

  private Vector3 lastFrameScale;

  private void OnEnable() {
    var localScale = frameHolder.localScale;
    lastFrameScale = localScale;
    localScale = new Vector3(localScale.x, -localScale.y, localScale.z);
    frameHolder.localScale = localScale;

    if (monitor == null) {
      monitor = Manager.primary;
    }

    Manager.onReinitialized += ReInitialize;
  }

  private void OnDisable() {
    if (frameHolder) {
      frameHolder.localScale = lastFrameScale;
    }
    Manager.onReinitialized -= ReInitialize;
  }

  private void Update() {
    if (monitor == null) {
      ReInitialize();
    }

    if (monitor.texture && monitor.hasBeenUpdated) {
      OnRender(monitor.texture);
    }

    lastMonitorId = monitor.id;
    monitor.shouldBeUpdated = true;
  }

  private void OnRender(Texture2D inputTexture) {
    var inputWidth = inputTexture.width;
    var inputHeight = inputTexture.height;
    var desiredWidth = frameTextures.GetAspectRatio() * inputHeight;

    var frameTexture = frameTextures.GetFrameTexture();
    var frameDepthTexture = frameTextures.GetFrameDepthTexture();

    var widthRatio = desiredWidth / inputWidth;

    var offsetX = ((inputWidth - desiredWidth) / 2) / inputWidth;

    // Get the center of the desktop texture.
    Graphics.Blit(inputTexture, frameTexture, new Vector2(widthRatio, 1), new Vector2(offsetX, 0f));

    depthModel.InferDepth(frameTexture, frameDepthTexture);
  }

  private void ReInitialize() {
    monitor = Manager.GetMonitor(lastMonitorId);
  }
}
