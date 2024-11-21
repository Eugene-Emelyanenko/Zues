using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LightingUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject priceObject;
    [SerializeField] private GameObject availableObject;
    [SerializeField] private GameObject lockObject;
    public Button button;
    public LightingData lightingData;

    public void SetUp(LightingData data)
    {
        lightingData = data;
        icon.sprite = Resources.Load<Sprite>($"Lighting/{lightingData.index}");
        priceText.text = lightingData.price.ToString();
        priceObject.SetActive(true);
        lockObject.SetActive(true);
        availableObject.gameObject.SetActive(false);

        if(lightingData.isUnlocked)
        {
            priceObject.SetActive(false);
            lockObject.SetActive(false);
            availableObject.gameObject.SetActive(true);
        }
    }
}
