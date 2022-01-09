using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

/**
 * Loads in a MiDaS depth model and augments it for our use-case.
 * Also prepares textures and materials for working with the model.
 */
public class DepthModel : MonoBehaviour {
  public NNModel modelAsset;

  [Header("Min Max")]
  public ComputeShader minMaxShader;
  public Material normalizeMaterial;

  public const GraphicsFormat INPUT_TEXTURE_FORMAT = GraphicsFormat.R8G8B8A8_UNorm;
  public const GraphicsFormat OUTPUT_TEXTURE_FORMAT = GraphicsFormat.R32_SFloat;

  public delegate void DepthInferred(RenderTexture depthTexture);
  public event DepthInferred onDepthInferred;

  private Model model;
  private IWorker modelWorker;

  private Vector2Int dimens;

  private MinMaxer minMaxer;

  private Tensor modelInputTensor;

  // private RenderTexture srcTexture;
  private RenderTexture modelInputTexture;
  private RenderTexture modelOutputTexture;

  private void Start() {
    loadMidasModel();
    augmentMidasModel();

    modelWorker = WorkerFactory.CreateWorker(model);

    modelInputTexture = new RenderTexture(dimens.x, dimens.y, 0, INPUT_TEXTURE_FORMAT, 0);
    modelOutputTexture = new RenderTexture(dimens.x, dimens.y, 0, OUTPUT_TEXTURE_FORMAT, 0);

    minMaxer = new MinMaxer(modelOutputTexture, minMaxShader);
  }

  private void OnDestroy() {
    modelWorker.Dispose();
    minMaxer.Dispose();
    modelInputTexture.Release();
    modelOutputTexture.Release();
    modelInputTensor?.Dispose();
  }

  public void InferDepth(RenderTexture src, RenderTexture dest) {
    SetInputTexture(src);
    InferDepth(dest);
    modelInputTensor.Dispose();
    modelInputTensor = null;
  }

  public void InferDepth(RenderTexture dest) {
    // INFER FROM INPUT TEXTURE
    modelWorker.Execute(modelInputTensor);

    // OUTPUT FROM MODEL TO OUTPUT TEXTURE
    Tensor output = modelWorker.PeekOutput();
    output.ToRenderTexture(modelOutputTexture);

    // Determine the min/max value from the model output.
    minMaxer.ComputeMinMax(normalizeMaterial);

    // Use the computed min/max to normalize the values between 0 and 1.
    Graphics.Blit(modelOutputTexture, dest, normalizeMaterial);

    onDepthInferred?.Invoke(dest);
  }

  public void SetInputTexture(RenderTexture inRt) {
    // int inWidth = inRt.width;
    // int inHeight = inRt.height;
    // srcTexture = inRt;
    Graphics.Blit(inRt, modelInputTexture);
    modelInputTensor = new Tensor(modelInputTexture, 3);
  }

  public bool HasInputTexture() {
    return modelInputTensor != null;
  }

  private void loadMidasModel() {
    model = ModelLoader.Load(modelAsset);

    var input = model.inputs[0];
    var inputShape = input.shape;

    // A little confused why there are 8 input dimensions for example (1, 1, 1, 1, 1, 256, 256, 3)
    dimens = new Vector2Int(inputShape[5], inputShape[6]);

    Debug.Log($"Loaded model with input dimensions: {dimens}");
  }

  private void augmentMidasModel() {
    // Update the MIDAS model to reshape and transpose the final output properly.
    var builder = new ModelBuilder(model);
    var outputLayer = builder.Reshape("custom_reshape", model.outputs[0], new TensorShape(1, dimens.x, dimens.y, 1));
    outputLayer = builder.Transpose("custom_transpose", outputLayer, new[]{0, 2, 1, 3}); // Rotate layer 90deg
    builder.Output(outputLayer);
  }
}
