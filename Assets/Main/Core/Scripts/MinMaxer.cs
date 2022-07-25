using UnityEngine;

public class MinMaxer {
  private static readonly int MIN_VALUE = Shader.PropertyToID("_Min");
  private static readonly int DIFF_VALUE = Shader.PropertyToID("_Diff");

  // These are based on numbers from the compute shader.
  private const int THREAD_GROUP_X = 32;
  private const int THREAD_GROUP_Y = 8;
  private const int THREAD_GROUP_Z = 1;

  private struct MinMax {
    public uint min;
    public uint max;
  }

  private const int BUFFER_SIZE = sizeof(uint) + sizeof(uint);

  private readonly MinMax[] minMaxArr = {
    new MinMax()
  };

  private readonly ComputeBuffer minMaxBuffer;
  private readonly ComputeShader minMaxShader;

  private readonly RenderTexture textureToMinMax;
  // private readonly Texture2D textureToMinMaxCpu;
  private readonly int textureWidth;
  private readonly int textureHeight;
  private readonly Rect textureRect;

  public MinMaxer(RenderTexture textureToMinMax, ComputeShader shader) {
    minMaxShader = shader;

    this.textureToMinMax = textureToMinMax;

    textureWidth = textureToMinMax.width;
    textureHeight = textureToMinMax.height;

    // textureToMinMaxCpu = new Texture2D(textureWidth, textureHeight, textureToMinMax.graphicsFormat, 0);

    minMaxBuffer = new ComputeBuffer(minMaxArr.Length, BUFFER_SIZE);
    minMaxBuffer.SetData(minMaxArr);

    minMaxShader.SetBuffer(0, "buffer", minMaxBuffer);
    minMaxShader.SetTexture(0, "DepthTexture", textureToMinMax);

    textureRect = new Rect(0, 0, textureWidth, textureHeight);
  }

  public void Dispose() {
    minMaxBuffer.Dispose();
  }

  // public void ComputeMinMax(Material minMaxMaterial) {
  //   var hold = RenderTexture.active;
  //   RenderTexture.active = textureToMinMax;
  //   textureToMinMaxCpu.ReadPixels(textureRect, 0, 0);
  //   RenderTexture.active = hold;
  //   var pixels = textureToMinMaxCpu.GetPixels();
  //   var min = float.MaxValue;
  //   var max = float.MinValue;
  //   foreach (var pixel in pixels) {
  //     if (pixel.r < min) {
  //       min = pixel.r;
  //     }
  //     if (pixel.r > max) {
  //       max = pixel.r;
  //     }
  //   }
  //
  //   minMaxMaterial.SetFloat(MIN_VALUE, min);
  //   minMaxMaterial.SetFloat(DIFF_VALUE, max - min);
  // }

  public void ComputeMinMax(Material minMaxMaterial) {
    minMaxArr[0].min = uint.MaxValue;
    minMaxArr[0].max = uint.MinValue;
    minMaxBuffer.SetData(minMaxArr);
    minMaxShader.Dispatch(0, textureWidth / THREAD_GROUP_X, textureHeight / THREAD_GROUP_Y, THREAD_GROUP_Z);
    minMaxBuffer.GetData(minMaxArr);

    var data = minMaxArr[0];
    var min = data.min / 1000f;
    var max = data.max / 1000f;
    minMaxMaterial.SetFloat(MIN_VALUE, min);
    minMaxMaterial.SetFloat(DIFF_VALUE, max - min);
  }
}
