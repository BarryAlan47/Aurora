using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class Door : Interactable
    {
        [SerializeField, Tooltip("The transform of the door to be animated.")]
        private Transform doorTransform;

        [SerializeField, Tooltip("Duration for the door to open.")]
        private float openDuration = 0.4f;

        [SerializeField, Tooltip("Duration for the door to close.")]
        private float closeDuration = 0.5f;

        private Vector3 openAngle = new Vector3(0f, 90f, 0f);  // The angle the door rotates to when opening.

        /// <summary>
        /// Called when the player enters the trigger area. It opens the door towards the player.
        /// </summary>
        protected override void OnPlayerEnter()
        {
            OpenDoor(player.transform); // Opens the door by calling the OpenDoor method, passing the player's transform.
        }

        /// <summary>
        /// Called when the player exits the trigger area. It closes the door.
        /// </summary>
        protected override void OnPlayerExit()
        {
            CloseDoor(); // Closes the door by calling the CloseDoor method.
        }

        /// <summary>
        /// Opens the door towards the player by rotating it to the open angle.
        /// </summary>
        /// <param name="interactor">The transform of the object interacting with the door (typically the player).</param>
        public void OpenDoor(Transform interactor)
        {
            Vector3 direction = (interactor.position - transform.position).normalized; // Direction from door to player.
            float dotProduct = Vector3.Dot(direction, transform.forward); // Dot product to determine direction.
            Vector3 targetAngle = openAngle * Mathf.Sign(dotProduct); // Calculate the correct rotation angle for opening.

            doorTransform.DOLocalRotate(targetAngle, openDuration, RotateMode.LocalAxisAdd); // Animate the door rotation using DOTween.
        }

        /// <summary>
        /// Closes the door by rotating it back to the original position.
        /// </summary>
        public void CloseDoor()
        {
            doorTransform.DOLocalRotate(Vector3.zero, closeDuration).SetEase(Ease.OutBounce); // Animate the door closing with bounce effect using DOTween.
        }
    }
}
