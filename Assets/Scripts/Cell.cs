using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellType
{
    Empty,
    Enemy,
    PowerUpHealth,
    PowerUpDamage,
    Player,
    Finish
}
public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject isPlayer;
    [SerializeField] private Image cellIcon;
    public CellType Type { get; private set; }
    public int EnemyHealth;
    public int EnemyDamage;

    public int EnemyIcon { get; private set; }

    public int PowerUpHealth { get; private set; }

    public int PowerUpDamage { get; private set; }

    public void SetUp(CellType type, int playerIcon = 0, int enemyHealth = 0, int enemyIcon = 0, int enemyDamage = 0, int powerUpHealth = 0, int powerUpDamage = 0)
    {
        Type = type;

        EnemyIcon = enemyIcon;

        isPlayer.SetActive(false);

        if (type == CellType.Player)
        {
            isPlayer.SetActive(true);
            cellIcon.sprite = Resources.Load<Sprite>($"Lighting/{playerIcon}");
        }
        else if (type == CellType.Enemy)
        {
            EnemyHealth = enemyHealth;
            EnemyDamage = enemyDamage;
            cellIcon.sprite = Resources.Load<Sprite>($"Enemy/{enemyIcon}");
        }
        else if(type == CellType.PowerUpHealth)
        {
            PowerUpHealth = powerUpHealth;
            cellIcon.sprite = Resources.Load<Sprite>($"Powerup/Health");
        }
        else if(type == CellType.PowerUpDamage)
        {
            PowerUpDamage = powerUpDamage;
            cellIcon.sprite = Resources.Load<Sprite>($"Powerup/Damage");
        }
        else if(type == CellType.Finish)
        {
            cellIcon.sprite = Resources.Load<Sprite>($"Finish");
        }
        else
        {
            isPlayer.SetActive(false);
            cellIcon.sprite = Resources.Load<Sprite>($"Transparent"); ;
        }
    }
}
