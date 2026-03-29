using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 画布 UI 管理器：负责金币文本、设置面板和新手引导等界面。
/// </summary>
public class CanvasUiManager : MonoBehaviour
{
    [LabelText("金币文本组件")]
    public Text collectedMoney;

    [LabelText("拖拽移动提示窗口")]
    public GameObject dragToMoveWindow;

    [LabelText("设置面板")]
    public GameObject settingsPanel;

    /// <summary>
    /// 处理拖拽提示窗口的关闭逻辑。
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButton(0) && dragToMoveWindow)
        {
            PlayerPrefs.SetString("DragWindow","");
            Destroy(dragToMoveWindow);
        }
    }

    /// <summary>
    /// 初始化拖拽提示窗口显示状态。
    /// </summary>
    private void Start()
    {
        if(PlayerPrefs.HasKey("DragWindow"))
            Destroy(dragToMoveWindow);
    }

    /// <summary>
    /// 设置金币文本显示。
    /// </summary>
    /// <param name="amount">金币数量。</param>
    public void SetMoneyText(int amount)
    {
        collectedMoney.text = "$" + amount.ToString();
    }

    /// <summary>
    /// 重新加载当前场景：与调试键 P（清空存档）+ R（重载场景）一致；会先展示插屏广告。
    /// </summary>
    public void Reload()
    {
        MyAdManager.Instance.ShowInterstitialAd();
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 播放激励视频广告以获取奖励金币。
    /// </summary>
    public void GetRewardCash()
    {
        MyAdManager.Instance.ShowRewardVideo();
    }

    /// <summary>
    /// 打开设置窗口（会先展示插屏广告）。
    /// </summary>
    public void OpenSettingsWindow()
    {
        MyAdManager.Instance.ShowInterstitialAd();
        settingsPanel.SetActive(true);
    }
}
