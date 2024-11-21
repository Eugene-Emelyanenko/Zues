using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LightingData
{
    public int index;
    public int price;
    public bool isUnlocked;
    public bool isSelected;

    public LightingData(int index, int price, bool isUnlocked, bool isSelected)
    {
        this.index = index;
        this.price = price;
        this.isUnlocked = isUnlocked;
        this.isSelected = isSelected;
    }
}

public static class LightingDataManager
{
    public readonly static string lightingDataKey = "LightingData";

    public static List<LightingData> LoadLightingData()
    {
        string json = PlayerPrefs.GetString(lightingDataKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            LightingDataListWrapper wrapper = JsonUtility.FromJson<LightingDataListWrapper>(json);
            return wrapper.lightingDataList;
        }
        return new List<LightingData>();
    }

    public static void SaveLightingData(List<LightingData> lightingDataList)
    {
        LightingDataListWrapper wrapper = new LightingDataListWrapper(lightingDataList);
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(lightingDataKey, json);
        PlayerPrefs.Save();
    }
}

[Serializable]
public class LightingDataListWrapper
{
    public List<LightingData> lightingDataList;

    public LightingDataListWrapper(List<LightingData> lightingDataList)
    {
        this.lightingDataList = lightingDataList;
    }
}
public class Market : MonoBehaviour
{
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private GameObject lightingPrefab;
    [SerializeField] private Transform lightingContainer;
    [SerializeField] private RectTransform coinsTransform;
    [SerializeField] private float xDefaultCoinsPos;
    [SerializeField] private float xShopCoinsPos;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Image selectedLightingImage;

    private List<LightingData> lightingDataList = new List<LightingData>();
    private LightingData selectedLighting = null;
    private int selectedLightingIndex = 0;

    private void Start()
    {
        EnableMenu(false);
        selectedLighting = lightingDataList.Find(data => data.isSelected);
        selectedLightingImage.sprite = Resources.Load<Sprite>($"Lighting/{selectedLighting.index}");
        selectedLightingIndex = selectedLighting.index - 1;
    }

    public void EnableMenu(bool isOpen)
    {
        marketPanel.SetActive(isOpen);

        lightingDataList = LightingDataManager.LoadLightingData();
        if (lightingDataList.Count == 0)
            CreateDefaultLightingData();

        if (isOpen)
        {
            coinsTransform.anchorMin = new Vector2(1, 1);
            coinsTransform.anchorMax = new Vector2(1, 1);
            coinsTransform.anchoredPosition = new Vector2(xShopCoinsPos, coinsTransform.anchoredPosition.y);

            DisplayLightings();
        }
        else
        {
            coinsTransform.anchorMin = new Vector2(0, 1);
            coinsTransform.anchorMax = new Vector2(0, 1);
            coinsTransform.anchoredPosition = new Vector2(xDefaultCoinsPos, coinsTransform.anchoredPosition.y);
        }

        UpdateCoinsText();
    }

    private void CreateDefaultLightingData()
    {
        lightingDataList = new List<LightingData>();
        for (int i = 0; i < 12; i++)
        {
            lightingDataList.Add(new LightingData(i + 1, 10, i == 0, i == 0));
        }
        LightingDataManager.SaveLightingData(lightingDataList);
    }

    private void DisplayLightings()
    {
        foreach (Transform t in lightingContainer)
        {
            Destroy(t.gameObject);
        }

        foreach (LightingData data in lightingDataList)
        {
            GameObject lightingObject = Instantiate(lightingPrefab, lightingContainer);
            lightingObject.name = $"Lighting_{data.index}";
            LightingUI lightingUI = lightingObject.GetComponent<LightingUI>();
            lightingUI.SetUp(data);
            lightingUI.button.onClick.RemoveAllListeners();
            lightingUI.button.onClick.AddListener(() => 
            {
                if(!data.isUnlocked)
                {
                    int coins = Coins.GetCoins();
                    if(coins >= data.price)
                    {
                        coins -= data.price;
                        Coins.SaveCoins(coins);
                        UpdateCoinsText();
                        SoundManager.Instance.PlayClip(SoundManager.Instance.buySound);
                        data.isUnlocked = true;
                        foreach (LightingData lightingData in lightingDataList)
                        {
                            lightingData.isSelected = false;
                        }
                        data.isSelected = true;
                        LightingDataManager.SaveLightingData(lightingDataList);
                        DisplayLightings();
                    }
                }
            });
        }
    }

    private void UpdateCoinsText()
    {
        coinsText.text = Coins.GetCoins().ToString();
    }

    private void UpdateSelectedLightingImage()
    {
        // Обновляем изображение выбранной молнии
        selectedLighting = lightingDataList[selectedLightingIndex];
        selectedLightingImage.sprite = Resources.Load<Sprite>($"Lighting/{selectedLighting.index}");
    }

    public void SelectNextLighting()
    {
        // Находим индекс следующей купленной молнии
        do
        {
            selectedLightingIndex = (selectedLightingIndex + 1) % lightingDataList.Count;
        } while (!lightingDataList[selectedLightingIndex].isUnlocked);

        SetSelectedLighting();
    }

    public void SelectPreviousLighting()
    {
        // Находим индекс предыдущей купленной молнии
        do
        {
            selectedLightingIndex = (selectedLightingIndex - 1 + lightingDataList.Count) % lightingDataList.Count;
        } while (!lightingDataList[selectedLightingIndex].isUnlocked);

        SetSelectedLighting();
    }

    private void SetSelectedLighting()
    {
        // Снимаем выбор с предыдущей молнии
        foreach (LightingData lightingData in lightingDataList)
        {
            lightingData.isSelected = false;
        }

        // Устанавливаем выбранной новую молнию
        lightingDataList[selectedLightingIndex].isSelected = true;

        // Сохраняем изменения
        LightingDataManager.SaveLightingData(lightingDataList);

        // Обновляем изображение выбранной молнии
        UpdateSelectedLightingImage();
    }
}
