using UnityEngine;

public class MinMaxer {
  private static readonly int MIN_VALUE = Shader.PropertyToID("_Min");
  private static readonly int MAX_VALUE = Shader.PropertyToID("_Max");

  // These are based on numbers from the compute shader.
  private const int THREAD_GROUP_X = 8;
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

  private readonly int textureWidth;
  private readonly int textureHeight;

  public MinMaxer(RenderTexture textureToMinMax, ComputeShader shader) {
    minMaxShader = shader;

    textureWidth = textureToMinMax.width;
    textureHeight = textureToMinMax.height;

    minMaxBuffer = new ComputeBuffer(minMaxArr.Length, BUFFER_SIZE);
    minMaxBuffer.SetData(minMaxArr);

    minMaxShader.SetBuffer(0, "buffer", minMaxBuffer);
    minMaxShader.SetTexture(0, "DepthTexture", textureToMinMax);
  }

  public void Dispose() {
    minMaxBuffer.Dispose();
  }

  public void ComputeMinMax(Material minMaxMaterial) {
    minMaxArr[0].min = uint.MaxValue;
    minMaxArr[0].max = uint.MinValue;
    minMaxBuffer.SetData(minMaxArr);
    minMaxShader.Dispatch(0, textureWidth / THREAD_GROUP_X, textureHeight / THREAD_GROUP_Y, THREAD_GROUP_Z);
    minMaxBuffer.GetData(minMaxArr);

    minMaxMaterial.SetFloat(MIN_VALUE, minMaxArr[0].min / 1000f);
    minMaxMaterial.SetFloat(MAX_VALUE, minMaxArr[0].max / 1000f);
  }
}
