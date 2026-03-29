using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家逻辑：与货架、收银台、购买点等进行交互。
/// </summary>
public class Player : MonoBehaviour
{
    [LabelText("游戏管理器引用")]
    private GameManager _GameManager;

    [LabelText("收银台引用")]
    private BillingDesk _BillingDesk;

    [LabelText("是否移除过任意食物标记")]
    private bool removedAnyFood;

    [LabelText("玩家物品管理器")]
    public PlayerManager _PlayerManager;

    [LabelText("玩家容量升级价格")]
    public int playerCapacityBuyAmount;

    [LabelText("玩家容量价格文本")]
    public Text playerCapaciyTest;

    /// <summary>
    /// 初始化玩家容量、价格和管理器引用。
    /// </summary>
    private void Start()
    { 
        _PlayerManager.maxFoodPlayerCarry = PlayerPrefs.GetInt("PlayerCapacity", _PlayerManager.maxFoodPlayerCarry);
        playerCapacityBuyAmount = PlayerPrefs.GetInt("PlayerCapacityBuyAmount", playerCapacityBuyAmount);
        playerCapaciyTest.text = playerCapacityBuyAmount.ToString();

        _GameManager = FindObjectOfType<GameManager>();
        _BillingDesk = FindObjectOfType<BillingDesk>();
    }

    /// <summary>
    /// 持续触发：处理与货架和收银台的交互。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<FoodPlaceManager>())
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();

            if (shelf.collectedFoods.Count < shelf.collectFoodCapacity)
            {
                int collectedFoodCount = _PlayerManager.collectedFood.Count - 1;

                if (collectedFoodCount >= 0)
                {
                    for (int i = _PlayerManager.collectedFood.Count - 1; i >= 0; i--)
                    {
                        if (_PlayerManager.collectedFood[i].foodName == shelf.shelfFoodName)
                        {
                            removedAnyFood = true;
                            _PlayerManager.collectedFood[i].PlaceFood(shelf.shelfTopTransform);
                            AudioManager.Instance.Play("FoodPlace");

                            shelf.collectedFoods.Add(_PlayerManager.collectedFood[i]);
                            _PlayerManager.collectedFood[i].transform.parent = shelf.transform;
                            shelf.MoveShelfTopTransform();

                            _PlayerManager.collectedFood[i].goToCustomer = true;
                            _PlayerManager.collectedFood.Remove(_PlayerManager.collectedFood[i]);
                            break;
                        }
                    }

                    if (removedAnyFood)
                    {
                        Transform foodCollectPos = _PlayerManager.foodCollectPos;

                        foodCollectPos.localPosition = _PlayerManager.initialFoodCollectPos;

                        foreach (Food food in _PlayerManager.collectedFood)
                        {
                            food.transform.localPosition = foodCollectPos.localPosition;
                            foodCollectPos.localPosition = new Vector3(foodCollectPos.transform.localPosition.x, foodCollectPos.transform.localPosition.y + 1, foodCollectPos.transform.localPosition.z);
                        }

                        removedAnyFood = false;
                    }
                }
            }
        }

        if (other.CompareTag("BillingDeskCollider"))
        {
            if (_BillingDesk.money.Count > 0)
            {
                foreach (GameObject money in _BillingDesk.money)
                {
                    money.transform.DOJump(transform.position, 4, 1, .4f)
                    .OnComplete(delegate ()
                    {
                        _GameManager.AddMoney(5);
                        AudioManager.Instance.Play("MoneyCollect");
                        Destroy(money);
                    });
                }

                _BillingDesk.money = new List<GameObject>();
                _BillingDesk.moneyPosCount = 0;

                Vector3 vec = _BillingDesk.moneyPosParent.position;
                vec.y = 2;

                _BillingDesk.moneyPosParent.position = vec;
            }
        }
    }

    /// <summary>
    /// 进入触发区域：开始在购买点花钱或打开 Helper 升级窗口。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
        {
            other.GetComponent<BuyPoint>().StartSpend();
        }

        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().OpenWindow();
        }
    }

    /// <summary>
    /// 离开触发区域：停止在购买点花钱或关闭 Helper 升级窗口。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BuyPoint"))
            other.GetComponent<BuyPoint>().StopSpend();

        if (other.gameObject.CompareTag("HelperSpawner"))
        {
            other.GetComponent<HelperBuy_UpgradePoint>().CloseWindow();
        }
    }

    /// <summary>
    /// 提升玩家携带容量（消耗金币并提高价格）。
    /// </summary>
    public void IncreasePlayerCapacity()
    {
        if (_GameManager.collectedMoney >= playerCapacityBuyAmount)
        {
            AudioManager.Instance.Play("Upgrade");

            _PlayerManager.maxFoodPlayerCarry++;
            PlayerPrefs.SetInt("PlayerCapacity", _PlayerManager.maxFoodPlayerCarry);

            playerCapacityBuyAmount += 100;
            PlayerPrefs.SetInt("PlayerCapacityBuyAmount", playerCapacityBuyAmount);

            playerCapaciyTest.text = playerCapacityBuyAmount.ToString();
        }
    }
}
