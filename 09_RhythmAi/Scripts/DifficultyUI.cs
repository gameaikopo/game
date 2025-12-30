using UnityEngine;
using TMPro;

public class DifficultyUI : MonoBehaviour
{
    public static DifficultyUI Instance;

    [SerializeField] 
    private TextMeshProUGUI difficultyText;

    private float showTime = 1.0f;   // 표시 시간
    private float timer = 0f;

    void Awake()
    {
        Instance = this;
        difficultyText.text = "";
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
                difficultyText.text = "";
        }
    }

    public void ShowDifficulty(string state)
    {
        if (state == "Increase")
        {
            difficultyText.text = "Difficulty ↑";
            difficultyText.color = Color.yellow;
        }
        else if (state == "Decrease")
        {
            difficultyText.text = "Difficulty ↓";
            difficultyText.color = Color.cyan;
        }
        else
        {
            difficultyText.text = "Difficulty →";
            difficultyText.color = Color.gray;
        }

        timer = showTime;
    }
}