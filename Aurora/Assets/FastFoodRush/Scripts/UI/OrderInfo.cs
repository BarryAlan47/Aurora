using UnityEngine;
using TMPro;

namespace CryingSnow.FastFoodRush
{
    public class OrderInfo : MonoBehaviour
    {
        [SerializeField, Tooltip("The icon image to be displayed for the order info.")]
        private GameObject iconImage;

        [SerializeField, Tooltip("The text component that shows the amount for the order.")]
        private TMP_Text amountText;

        [SerializeField, Tooltip("The offset from the displayer's position to place the order info on screen.")]
        private Vector3 displayOffset = new Vector3(0f, 2.5f, 0f);

        private Transform displayer; // Transform of the object (e.g., a customer) the order info is attached to
        private Camera mainCamera; // Main camera reference to convert world position to screen position

        void Start()
        {
            mainCamera = Camera.main; // Get the main camera reference
            HideInfo(); // Ensure the order info is hidden at the start
        }

        void LateUpdate()
        {
            if (displayer == null) return; // If no displayer is set, do nothing

            // Calculate the display position of the order info in screen space
            var displayPosition = displayer.position + displayOffset;
            transform.position = mainCamera.WorldToScreenPoint(displayPosition); // Set the position of the UI element on screen
        }

        /// <summary>
        /// Displays the order information UI at the specified displayer's position with the given amount.
        /// If the amount is greater than 0, the icon will be shown and the amount will be displayed. 
        /// If the amount is 0 or less, the icon is hidden and the text will display "NO SEAT!".
        /// </summary>
        /// <param name="displayer">The Transform of the object (e.g., a customer) to which the order info is attached.</param>
        /// <param name="amount">The amount of the order to display. If 0 or less, it shows "NO SEAT!".</param>
        public void ShowInfo(Transform displayer, int amount)
        {
            gameObject.SetActive(true); // Enable the order info UI element
            this.displayer = displayer; // Set the displayer's transform

            bool active = amount > 0; // Determine if the amount is greater than 0
            iconImage.SetActive(active); // Set the icon's active state based on the amount
            amountText.text = active ? amount.ToString() : "NO SEAT!"; // Display the amount or a message if no seat
        }

        /// <summary>
        /// Hides the order information UI and resets the displayer to null.
        /// This is used to make the order info UI disappear when it is no longer needed.
        /// </summary>
        public void HideInfo()
        {
            gameObject.SetActive(false); // Disable the order info UI element
            displayer = null; // Reset the displayer reference
        }
    }
}
