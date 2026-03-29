using RDG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// Helper 生成与升级面板控制：负责雇佣 Helper 以及容量/速度升级。
/// </summary>
public class HelperSpawner : MonoBehaviour
{
    [LabelText("容量/雇佣/速度按钮")]
    public Button helperCapacityBtn, helperBuybtn, helperSpeedBtn;

    [LabelText("容量/速度/雇佣价格文本")]
    public Text capacityBuyValText, speedBuyValText, helperBuyValText;

    [LabelText("价格递增值")]
    public int moneyIncreaseVal, increaseCapacityVal, increaseSpeedVal;

    [LabelText("当前容量/速度/雇佣价格")]
    private int capacityBuyVal, speedBuyValue, helperBuyValue;

    [LabelText("游戏管理器引用")]
    private GameManager _GameManager;

    [LabelText("容量/速度已满提示")]
    public GameObject capacityFullText, speedFullText;

    [LabelText("当前 Helper 引用")]
    public Helper helper;

    [LabelText("Helper 预制体")]
    public GameObject helperPrefab;

    [LabelText("Helper 生成点")]
    public Transform helperSpawnPoint;

    [LabelText("序号（用于存档 Key）")]
    public int srNo;

    /// <summary>
    /// 面板启用时初始化价格文本与按钮状态。
    /// </summary>
    private void OnEnable()
    {
        _GameManager = FindObjectOfType<GameManager>();
        UpdateBuyAmountsText();
        CheckButtonsActive();
    }

    /// <summary>
    /// 购买 Helper（播放广告、扣费并实例化 Helper）。
    /// </summary>
    public void BuyHelper()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");
        _GameManager.LessMoneyinBulk(helperBuyValue);

        helper = Instantiate(helperPrefab, helperSpawnPoint.position, helperSpawnPoint.rotation).GetComponent<Helper>();

        PlayerPrefs.SetString(srNo + "Helper", "");

        helperBuybtn.transform.parent.gameObject.SetActive(false);

        helperCapacityBtn.transform.parent.gameObject.SetActive(true);
        helperSpeedBtn.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// 购买 Helper 容量升级。
    /// </summary>
    public void BuyCapacity()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");

        _GameManager.LessMoneyinBulk(capacityBuyVal);

        capacityBuyVal += moneyIncreaseVal;
        PlayerPrefs.SetInt(srNo + "CapacityBuyVal", capacityBuyVal);

        UpdateBuyAmountsText();

        if (helper._PlayerManager.maxFoodPlayerCarry == 12)
        {
            capacityFullText.SetActive(true);
            PlayerPrefs.SetString(srNo + "CapacityFull", "True");
        }

        helper.IncreaseCapacity(increaseCapacityVal);

        CheckButtonsActive();
        helper.upgradeParticle.Play();
    }

    /// <summary>
    /// 购买 Helper 移动速度升级。
    /// </summary>
    public void BuySpeed()
    {
        MyAdManager.Instance.ShowInterstitialAd();

        AudioManager.Instance.Play("Upgrade");

        _GameManager.LessMoneyinBulk(speedBuyValue);

        speedBuyValue += moneyIncreaseVal;
        PlayerPrefs.SetInt(srNo + "SpeedBuyVal", speedBuyValue);

        UpdateBuyAmountsText();

        if (helper.gameObject.GetComponent<NavMeshAgent>().speed == 20)
        {
            speedFullText.SetActive(true);
            PlayerPrefs.SetString(srNo + "SpeedFull", "True");
        }
        helper.IncreaseSpeed(increaseSpeedVal);

        CheckButtonsActive();
        helper.upgradeParticle.Play();

    }

    /// <summary>
    /// 刷新所有价格文本，并根据存档状态更新交互。
    /// </summary>
    private void UpdateBuyAmountsText()
    {
        if (PlayerPrefs.HasKey(srNo + "CapacityFull"))
        {
            capacityFullText.SetActive(true);
            helperCapacityBtn.interactable = false;
        }

        if (PlayerPrefs.HasKey(srNo + "SpeedFull"))
        {
            speedFullText.SetActive(true);
            helperSpeedBtn.interactable = false;
        }

        helperBuyValue = moneyIncreaseVal;
        helperBuyValText.text = moneyIncreaseVal.ToString();

        capacityBuyVal = PlayerPrefs.GetInt(srNo + "CapacityBuyVal", moneyIncreaseVal);
        capacityBuyValText.text = capacityBuyVal.ToString();

        speedBuyValue = PlayerPrefs.GetInt(srNo + "SpeedBuyVal", moneyIncreaseVal);
        speedBuyValText.text = speedBuyValue.ToString();
    }

    /// <summary>
    /// 根据金币与存档状态检查按钮是否可用/显示。
    /// </summary>
    private void CheckButtonsActive()
    {
        if (PlayerPrefs.HasKey(srNo + "Helper"))
            helperBuybtn.transform.parent.gameObject.SetActive(false);
        else
        {
            if (helperBuyValue <= _GameManager.collectedMoney)
                helperBuybtn.interactable = true;
            else
                helperBuybtn.interactable = false;

            helperCapacityBtn.transform.parent.gameObject.SetActive(false);
            helperSpeedBtn.transform.parent.gameObject.SetActive(false);
        }

        if (capacityBuyVal <= _GameManager.collectedMoney)
            helperCapacityBtn.interactable = true;
        else
            helperCapacityBtn.interactable = false;


        if (speedBuyValue <= _GameManager.collectedMoney)
            helperSpeedBtn.interactable = true;
        else
            helperSpeedBtn.interactable = false;
    }
}
