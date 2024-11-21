using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public float loadingTime = 3f;
    public string sceneToLoad = "Menu";
    public Transform rotatingImage;
    public float rotationSpeed = 100f;
    public TextMeshProUGUI progressText;

    private void Start()
    {
        StartCoroutine(AnimateLoading());
    }

    private IEnumerator AnimateLoading()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;

            // �������� �����������
            rotatingImage.Rotate(-Vector3.forward * rotationSpeed * Time.deltaTime);

            // ���������� ������ ���������
            float progress = Mathf.Clamp01(elapsedTime / loadingTime) * 100f;
            progressText.text = Mathf.RoundToInt(progress) + "%";

            yield return null;
        }

        // �������� ����� ����� ����� ���������� ������� ��������
        SceneManager.LoadScene(sceneToLoad);
    }
}
