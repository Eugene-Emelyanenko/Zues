using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    public int maxHealth = 3;
    public int currentHealth;
    public int startPlayerDamage = 1;
    public int currentPlayerDamage;
    public Vector2Int playerPosition;

    [Header("Grid")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellContainer;
    public int gridWidth;
    public int gridHeight;
    private Cell[][] grid;

    [Header("Levels")]
    [SerializeField] private Transform levelsContainer;
    public int bonusLevel;
    public int finishLevel;
    public int coinsForLevel = 50;
    public int bonusLevelCoinsIncreaser = 2;

    [Header("Animation")]
    public float timeAppear = 1f;

    private LightingData selectedLighting = null;
    private Vector2Int previousPlayerPosition;
    private int currentLevel = 1;
    private bool canInput = false;

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        foreach (Transform t in levelsContainer)
        {
            t.gameObject.SetActive(false);
        }

        for (int i = 0; i < currentLevel; i++)
        {
            levelsContainer.GetChild(i).gameObject.SetActive(true);
        }

        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        currentPlayerDamage = startPlayerDamage;
        UpdateHealth();

        InitializeGrid();
        PlacePlayerAndFinish();
    }

    private void InitializeGrid()
    {
        foreach (Transform t in cellContainer)
        {
            Destroy(t.gameObject);
        }

        grid = new Cell[gridHeight][];
        int[] rowDamageBoosts = new int[gridHeight]; // ƒл€ каждого р€да следим за собранными усилени€ми урона

        for (int y = 0; y < gridHeight; y++)
        {
            grid[y] = new Cell[gridWidth];

            // ѕодсчитываем общее количество усилений урона до текущего р€да включительно
            int totalDamageBoostsUpToRow = 0;
            for (int i = 0; i <= y; i++) // ¬ключаем текущий р€д
            {
                totalDamageBoostsUpToRow += rowDamageBoosts[i];
            }

            bool isHealth = false;

            for (int x = 0; x < gridWidth; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, cellContainer);
                cellObj.name = $"Cell[y:{y}][x:{x}]";
                Cell cell = cellObj.GetComponent<Cell>();

                float randomValue = Random.value;

                if(isHealth)
                {
                    cell.SetUp(CellType.PowerUpHealth, powerUpHealth: 1);
                    isHealth = false;
                }
                else
                {
                    if (randomValue < 0.4f) // 40% шанс на врага
                    {
                        int enemyHealth = 0;
                        int enemyDamage = 0;

                        if (Random.value < 0.5)
                        {
                            enemyHealth = (int)Mathf.Max(1, startPlayerDamage * Mathf.Pow(2, totalDamageBoostsUpToRow) + 1);

                            isHealth = true;
                        }
                        else
                        {
                            enemyHealth = (int)Mathf.Max(1, startPlayerDamage * Mathf.Pow(2, totalDamageBoostsUpToRow));
                        }

                        enemyDamage = Mathf.Min(maxHealth / 3, 2); // ќграничим урон врага

                        cell.SetUp(CellType.Enemy, enemyHealth: enemyHealth, enemyDamage: enemyDamage, enemyIcon: Random.Range(1, 4));
                    }
                    else if (randomValue < 0.55f) // 15% шанс на усиление здоровь€
                    {
                        cell.SetUp(CellType.PowerUpHealth, powerUpHealth: 1);
                    }
                    else if (randomValue < 0.7f) // 15% шанс на усиление урона
                    {
                        cell.SetUp(CellType.PowerUpDamage, powerUpDamage: 2);
                        rowDamageBoosts[y]++; // ”величиваем счетчик усилений урона дл€ текущего р€да
                    }
                    else
                    {
                        cell.SetUp(CellType.Empty); // 30% пустые клетки
                    }
                }
                
                grid[y][x] = cell;
            }
        }

        AppearCells(true, CanInput);
    }

    private void AppearCells(bool isIn, UnityAction onEndAction)
    {
        int cellCount = cellContainer.childCount;
        int completedAnimations = 0; // ƒл€ отслеживани€ завершенных анимаций

        foreach (Transform cell in cellContainer)
        {
            Image cellImage = cell.GetComponent<Image>();
            Image isPlayerImage = cell.Find("IsPlayer").GetComponent<Image>();
            Image iconImage = cell.Find("Icon").GetComponent<Image>();

            // ќпредел€ем финальное значение прозрачности (1 = полностью видно, 0 = невидимо)
            float endValue = isIn ? 1f : 0f;

            // ѕлавное изменение прозрачности дл€ каждого изображени€
            cellImage.DOFade(endValue, timeAppear).OnComplete(() => CheckAnimationComplete(ref completedAnimations, cellCount, onEndAction));
            isPlayerImage.DOFade(endValue, timeAppear).OnComplete(() => CheckAnimationComplete(ref completedAnimations, cellCount, onEndAction));
            iconImage.DOFade(endValue, timeAppear).OnComplete(() => CheckAnimationComplete(ref completedAnimations, cellCount, onEndAction));
        }
    }

    private void CheckAnimationComplete(ref int completedAnimations, int totalAnimations, UnityAction onEndAction)
    {
        completedAnimations++;
        // ≈сли все анимации завершены, вызываем переданный метод
        if (completedAnimations >= totalAnimations * 3) // умножаем на 3, т.к. на каждом объекте 3 изображени€
        {
            onEndAction?.Invoke();
        }
    }

    private void CanInput()
    {
        canInput = true;
    }


    private void PlacePlayerAndFinish()
    {
        playerPosition = new Vector2Int(0, 0);
        List<LightingData> lightingDataList = LightingDataManager.LoadLightingData();
        selectedLighting = lightingDataList.Find(data => data.isSelected);
        grid[playerPosition.y][playerPosition.x].SetUp(CellType.Player, playerIcon: selectedLighting.index);

        grid[5][2].SetUp(CellType.Finish);
    }

    public void MoveUp()
    {
        if(canInput)
            MovePlayer(Vector2Int.up);
    }
    public void MoveLeft()
    {
        if (canInput)
            MovePlayer(Vector2Int.left);
    }
    public void MoveRight()
    {
        if (canInput)
            MovePlayer(Vector2Int.right);
    }
    private void MovePlayer(Vector2Int direction)
    {
        if(direction == Vector2Int.up && !CanMoveUp())
            return;

        Vector2Int newPosition = playerPosition + direction;

        if (newPosition.x < 0 || newPosition.x >= gridWidth || newPosition.y < 0 || newPosition.y >= gridHeight)
        {
            Debug.Log("New position is out of bounds.");
            return;
        }

        previousPlayerPosition = playerPosition; // —охран€ем текущую позицию перед движением
        HandleCellInteraction(newPosition, direction);
    }

    private bool CanMoveUp()
    {
        // ѕровер€ем весь текущий р€д (по оси X) на наличие врагов и усилений
        for (int x = 0; x < gridWidth; x++)
        {
            Cell cell = grid[playerPosition.y][x];

            if (cell.Type == CellType.Enemy || cell.Type == CellType.PowerUpHealth || cell.Type == CellType.PowerUpDamage)
            {
                Debug.Log($"Cannot move up. Cell at [y:{playerPosition.y}][x:{x}] contains {cell.Type}.");
                return false; // ≈сли найден враг или усиление, игрок не может двигатьс€ вверх
            }
        }

        return true; // ¬се клетки в р€ду пустые
    }

    private void HandleCellInteraction(Vector2Int position, Vector2Int direction)
    {
        Cell cell = grid[position.y][position.x];
        Debug.Log($"Handling interaction with cell of type: {cell.Type}");

        void Move()
        {
            grid[playerPosition.y][playerPosition.x].SetUp(CellType.Empty);
            playerPosition = position;
            grid[playerPosition.y][playerPosition.x].SetUp(CellType.Player, playerIcon: selectedLighting.index);
        }

        if (cell.Type == CellType.Enemy)
        {
            if (currentPlayerDamage >= cell.EnemyHealth)
            {
                Debug.Log("Enemy defeated!");
                Move();
            }
            else
            {
                cell.EnemyHealth -= currentPlayerDamage;
                Debug.Log("Player hit by enemy! Returning to previous position.");
                TakeDamage(cell.EnemyDamage);
            }
        }
        else if (cell.Type == CellType.PowerUpHealth)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + cell.PowerUpHealth);
            UpdateHealth();
            Debug.Log("Health restored!");
            Move();
        }
        else if (cell.Type == CellType.PowerUpDamage)
        {
            currentPlayerDamage *= cell.PowerUpDamage;
            Debug.Log("Damage increased!");
            Move();
        }
        else if(cell.Type == CellType.Finish)
        {
            if (CanFinish())
            {
                Debug.Log("All enemies and power-ups collected. Player reached the finish!");
                Move(); // ѕередвинуть игрока на финиш
                Finish(); // «авершить игру
            }
            else
            {
                Debug.Log("Cannot finish. There are still enemies or power-ups remaining.");
                if(direction == Vector2.right || direction == Vector2.left)
                {
                    Vector2Int newPosition = playerPosition + direction * 2;
                    previousPlayerPosition = playerPosition;
                    HandleCellInteraction(newPosition, direction);
                }
            }
        }
        else
        {
            Move();
        }
    }

    private void TakeDamage(int value)
    {
        currentHealth -= value;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }

        UpdateHealth();
    }

    private void UpdateHealth()
    {
        healthSlider.value = currentHealth;
        healthText.text = currentHealth.ToString();
    }

    private void GameOver()
    {
        SoundManager.Instance.PlayClip(SoundManager.Instance.gameoverSound);
        Debug.Log("Game Over!");
        StartGame();
    }

    private void Finish()
    {
        Debug.Log("Finish");

        SoundManager.Instance.PlayClip(SoundManager.Instance.winSound);

        int coins = Coins.GetCoins();

        if (currentLevel == bonusLevel)
        {          
            coins += coinsForLevel * bonusLevelCoinsIncreaser;
        }
        else
        {
            coins += coinsForLevel;
        }

        Coins.SaveCoins(coins);

        if (currentLevel == finishLevel)
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
        }
        else
        {
            currentLevel++;
            PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        }

        PlayerPrefs.Save();
        AppearCells(false, StartGame);
    }

    private bool CanFinish()
    {
        // ѕроходим по всем клеткам и провер€ем наличие врагов и усилений
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Cell cell = grid[y][x];
                if (cell.Type == CellType.Enemy || cell.Type == CellType.PowerUpHealth || cell.Type == CellType.PowerUpDamage)
                {
                    return false; // ≈сли найдены враги или усилени€, финишировать нельз€
                    
                }
            }
        }

        return true; // ¬се враги и усилени€ собраны
    }
}
