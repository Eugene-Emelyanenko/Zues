using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private Button chestButton;
    [SerializeField] private Sprite openedChestSprite;
    [SerializeField] private Sprite closedChestSprite;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Transform cooldownTimeContainer;

    public float shakeIntensity = 5f;
    public float shakeDuration = 1f;

    public int cooldownTime = 14400;

    private bool chestIsOpened = false;
    private bool isAnimating = false;  // Новый флаг для предотвращения повторных кликов
    private int currentCooldownTime = 0;
    private DateTime cooldownEndTime;

    private void Start()
    {
        // Получаем оставшееся время до конца отсчета
        chestIsOpened = PlayerPrefs.GetInt("ChestIsOpened", 0) == 1;
        long cooldownEndTicks = long.Parse(PlayerPrefs.GetString("CooldownEndTime", DateTime.Now.Ticks.ToString()));
        cooldownEndTime = new DateTime(cooldownEndTicks);

        currentCooldownTime = (int)(cooldownEndTime - DateTime.Now).TotalSeconds;

        if (currentCooldownTime <= 0)
        {
            chestIsOpened = false;
            currentCooldownTime = cooldownTime;
        }

        StartCoroutine(UpdateCooldownRoutine());

        EnablePanel(false);
    }

    public void EnablePanel(bool isOpen)
    {
        chestPanel.SetActive(isOpen);

        if (isOpen)
        {
            chestButton.image.sprite = chestIsOpened ? openedChestSprite : closedChestSprite;
            chestButton.onClick.RemoveAllListeners();

            if (!chestIsOpened && !isAnimating)  // Проверяем, что сундук закрыт и анимация не выполняется
            {
                chestButton.onClick.AddListener(() =>
                {
                    if (!isAnimating && !chestIsOpened) // Еще одна проверка для безопасности
                    {
                        StartCoroutine(ShakeAndOpenChest());
                    }
                });
            }
        }
    }

    IEnumerator ShakeAndOpenChest()
    {
        isAnimating = true;  // Устанавливаем флаг анимации

        // Тряска сундука
        yield return StartCoroutine(ShakeChest());

        int coins = Coins.GetCoins();
        coins += 100;
        Coins.SaveCoins(coins);
        UpdateCoinsText();
        SoundManager.Instance.PlayClip(SoundManager.Instance.getChestSound);

        chestIsOpened = true; // Устанавливаем флаг открытия сундука
        PlayerPrefs.SetInt("ChestIsOpened", 1);

        // Устанавливаем время окончания отсчета
        cooldownEndTime = DateTime.Now.AddSeconds(cooldownTime);
        PlayerPrefs.SetString("CooldownEndTime", cooldownEndTime.Ticks.ToString());
        PlayerPrefs.Save();

        chestButton.image.sprite = openedChestSprite;
        cooldownTimeContainer.gameObject.SetActive(true);

        isAnimating = false;  // Сбрасываем флаг анимации после завершения открытия
    }

    IEnumerator ShakeChest()
    {
        Vector3 originalPos = chestButton.transform.localPosition;
        float elapsedTime = 0;

        while (elapsedTime < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            float y = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            chestButton.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        chestButton.transform.localPosition = originalPos;
    }

    IEnumerator UpdateCooldownRoutine()
    {
        while (currentCooldownTime > 0)
        {
            yield return new WaitForSeconds(1);
            currentCooldownTime = (int)(cooldownEndTime - DateTime.Now).TotalSeconds;
            if (currentCooldownTime <= 0)
            {
                chestIsOpened = false;
                PlayerPrefs.SetInt("ChestIsOpened", 0);
                chestButton.image.sprite = closedChestSprite;
                currentCooldownTime = cooldownTime;
                cooldownTimeContainer.gameObject.SetActive(false);
            }
            UpdateCooldownTime();
        }
    }

    private void UpdateCoinsText()
    {
        coinsText.text = Coins.GetCoins().ToString();
    }

    private void UpdateCooldownTime()
    {
        foreach (Transform t in cooldownTimeContainer)
        {
            TextMeshProUGUI cooldownTimeText = t.GetComponent<TextMeshProUGUI>();
            if (currentCooldownTime > 0)
            {
                TimeSpan time = TimeSpan.FromSeconds(currentCooldownTime);
                cooldownTimeText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
            {
                cooldownTimeText.text = "00:00:00";
            }
        }
    }
}
