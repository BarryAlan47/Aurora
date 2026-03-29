using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

/// <summary>
/// 游戏管理器：负责全局金币数量与基础调试功能。
/// </summary>
public class GameManager : MonoBehaviour
{
    [HideInInspector]
    [LabelText("当前金币数量")]
    public int collectedMoney;

    [LabelText("画布 UI 管理器")]
    private CanvasUiManager _CanvasUiManager;

    [LabelText("顾客颜色列表")]
    public Color[] customerColors;

    [LabelText("顾客帽子模型列表")]
    public Mesh[] customerHats;

    /// <summary>
    /// 初始化游戏管理器，读取金币并刷新 UI。
    /// </summary>
    private void Start()
    {
        _CanvasUiManager = FindObjectOfType<CanvasUiManager>();
        collectedMoney = PlayerPrefs.GetInt("MoneyAmount", 0);
        _CanvasUiManager.SetMoneyText(collectedMoney);
    }

    /// <summary>
    /// 调试按键：P 清空存档，R 重载场景，M 增加测试金币。
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayerPrefs.DeleteAll();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMoney(500);
        }
    }

    /// <summary>
    /// 增加金币。
    /// </summary>
    /// <param name="amount">增加的金币数量。</param>
    public void AddMoney(int amount)
    {
        collectedMoney += amount;
        ShowAndSave();
    }

    /// <summary>
    /// 减少 1 点金币。
    /// </summary>
    public void LessMoney()
    {
        collectedMoney--;
        ShowAndSave();
    }

    /// <summary>
    /// 按指定数量批量减少金币。
    /// </summary>
    /// <param name="amount">要扣除的金币数量。</param>
    public void LessMoneyinBulk(int amount)
    {
        collectedMoney -= amount;
        ShowAndSave();
    }

    /// <summary>
    /// 刷新金币 UI 并保存到 PlayerPrefs。
    /// </summary>
    public void ShowAndSave()
    {
        _CanvasUiManager.SetMoneyText(collectedMoney);
        PlayerPrefs.SetInt("MoneyAmount", collectedMoney);
    }
}
