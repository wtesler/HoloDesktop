using System.Collections;
using TMPro;
using UnityEngine;

public class CornerMessage : MonoBehaviour {
  public TextMeshProUGUI textGui;

  private Coroutine showTextCoroutine;

  public void ShowText(string text) {
    if (showTextCoroutine != null) {
      StopCoroutine(showTextCoroutine);
      showTextCoroutine = null;
    }
    showTextCoroutine = StartCoroutine(ShowTextCoroutine(text));
  }

  private IEnumerator ShowTextCoroutine(string text) {
    textGui.gameObject.SetActive(true);
    textGui.text = text;
    yield return new WaitForSeconds(1);
    textGui.gameObject.SetActive(false);
    showTextCoroutine = null;
  }
}
