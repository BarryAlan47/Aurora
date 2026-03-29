using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using MyMiniMart.UI;

/// <summary>
/// 顾客头顶订单（A 方案）：<b>每位顾客</b>从预制体克隆一份 <see cref="CustomerOrderBubble"/>，互不抢占。
/// 配置：拖入「订单气泡预制体」+「Canvas 下的父节点」+ 食物名与图标映射表。
/// </summary>
public class CustomerOrderInfoService : MonoBehaviour
{
    public static CustomerOrderInfoService Instance { get; private set; }

    [System.Serializable]
    public class FoodIconEntry
    {
        [LabelText("食物名（与 Food.foodName / FoodPlaceManager.shelfFoodName 一致）")]
        public string foodName;

        [LabelText("对应图标")]
        public Sprite icon;
    }

    [FoldoutGroup("预制体与挂载")]
    [LabelText("订单气泡预制体")]
    [SerializeField]
    [Tooltip("从场景里做好的 Order UI 拖到 Project 生成 Prefab，再拖到这里；预制体上需挂 CustomerOrderBubble。")]
    private GameObject orderBubblePrefab;

    [FoldoutGroup("预制体与挂载")]
    [LabelText("UI 父节点（一般为 Canvas 或 Canvas 下空物体）")]
    [SerializeField]
    private RectTransform uiParent;

    [FoldoutGroup("食物图标映射")]
    [LabelText("食物名 → 图标")]
    [SerializeField]
    [Tooltip("键与货架/食物的 foodName 字符串完全一致时替换图标；未匹配则保留预制体默认图。")]
    private List<FoodIconEntry> foodIcons = new List<FoodIconEntry>();

    private readonly Dictionary<int, CustomerOrderBubble> _bubblesByCustomerId = new Dictionary<int, CustomerOrderBubble>();

    private readonly Dictionary<string, Sprite> _iconLookup = new Dictionary<string, Sprite>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildIconLookup();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnValidate()
    {
        RebuildIconLookup();
    }

    void RebuildIconLookup()
    {
        _iconLookup.Clear();
        if (foodIcons == null)
            return;
        foreach (var e in foodIcons)
        {
            if (e == null || string.IsNullOrEmpty(e.foodName) || e.icon == null)
                continue;
            _iconLookup[e.foodName.Trim()] = e.icon;
        }
    }

    public Sprite GetIconForFood(string foodName)
    {
        if (string.IsNullOrEmpty(foodName))
            return null;
        _iconLookup.TryGetValue(foodName.Trim(), out var s);
        return s;
    }

    /// <summary>
    /// 显示/更新该顾客的订单 UI（剩余件数 + 可选食物图标）。
    /// </summary>
    public void ShowForCustomer(Transform customer, int remainingAmount, string orderFoodName)
    {
        if (customer == null || orderBubblePrefab == null || uiParent == null)
            return;

        int id = customer.GetInstanceID();
        // 字典里可能残留「已 Destroy 的气泡」引用（Unity 假 null），需重建
        if (!_bubblesByCustomerId.TryGetValue(id, out var bubble) || bubble == null)
        {
            _bubblesByCustomerId.Remove(id);

            var go = Instantiate(orderBubblePrefab, uiParent);
            bubble = go.GetComponent<CustomerOrderBubble>();
            if (bubble == null)
            {
                Debug.LogError("[CustomerOrderInfoService] 预制体上缺少 CustomerOrderBubble 组件。");
                Destroy(go);
                return;
            }

            _bubblesByCustomerId[id] = bubble;
        }

        var icon = GetIconForFood(orderFoodName);

        if (remainingAmount > 0)
            bubble.ShowInfo(customer, remainingAmount, icon);
        else
            HideForCustomer(customer);
    }

    /// <summary>
    /// 隐藏并销毁该顾客对应的订单气泡。
    /// </summary>
    public void HideForCustomer(Transform customer)
    {
        if (customer == null)
            return;

        int id = customer.GetInstanceID();
        if (!_bubblesByCustomerId.TryGetValue(id, out var bubble))
            return;

        // 先从字典移除，避免在 Destroy 顺序中重复进入；已销毁的对象不能调用 HideInfo
        _bubblesByCustomerId.Remove(id);

        if (bubble == null)
            return;

        Destroy(bubble.gameObject);
    }
}
