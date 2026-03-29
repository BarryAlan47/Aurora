using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MyMiniMart.UI
{
    /// <summary>
    /// 屏幕空间跟点订单气泡（由 FastFood OrderInfo 改造）：每位顾客单独实例一份。
    /// 请在预制体上绑定 Icon 的 <see cref="Image"/> 用于替换食物图标。
    /// </summary>
    public class CustomerOrderBubble : MonoBehaviour
    {
        [SerializeField]
        private GameObject iconImageRoot;

        [SerializeField]
        [Tooltip("用于显示食物图标的 Image；与 iconImageRoot 可同时存在（根物体开关 + 子图换图）。")]
        private Image iconImage;

        [SerializeField]
        private TMP_Text amountText;

        [SerializeField]
        private Vector3 displayOffset = new Vector3(0f, 2.5f, 0f);

        private Transform _displayer;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_displayer == null || _mainCamera == null)
                return;

            var displayPosition = _displayer.position + displayOffset;
            transform.position = _mainCamera.WorldToScreenPoint(displayPosition);
        }

        /// <summary>
        /// 显示订单数量；可选传入食物图标。
        /// </summary>
        public void ShowInfo(Transform displayer, int amount, Sprite foodIcon = null)
        {
            gameObject.SetActive(true);
            _displayer = displayer;

            bool active = amount > 0;
            if (iconImageRoot != null)
                iconImageRoot.SetActive(active);

            if (amountText != null)
                amountText.text = active ? amount.ToString() : "NO SEAT!";

            if (iconImage != null)
            {
                if (foodIcon != null)
                {
                    iconImage.sprite = foodIcon;
                    iconImage.enabled = true;
                }
                // 未配置映射时保留预制体上原有 Sprite，不强制清空
            }
        }

        public void HideInfo()
        {
            // 气泡或物体已被 Destroy 时勿再访问（Unity 假 null）
            if (!this || !gameObject)
                return;
            gameObject.SetActive(false);
            _displayer = null;
        }
    }
}
