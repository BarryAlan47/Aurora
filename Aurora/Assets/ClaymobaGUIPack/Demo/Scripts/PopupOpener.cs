// Copyright (C) 2025 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace Ricimi
{
	// This class is responsible for creating and opening a popup of the
	// given prefab and adding it to the UI canvas of the current scene.
	public class PopupOpener : MonoBehaviour
	{
		public GameObject popupPrefab;

		protected Canvas canvas;
		protected GameObject popup;

		protected void Start()
		{
			canvas = GetComponentInParent<Canvas>();
		}

		public virtual void OpenPopup()
		{
			popup = Instantiate(popupPrefab, canvas.transform, false);
			popup.SetActive(true);
			popup.GetComponent<Popup>().Open();
		}
	}
}
