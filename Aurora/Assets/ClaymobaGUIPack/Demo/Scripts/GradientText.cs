// Copyright (C) 2025 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using TMPro;

namespace Ricimi
{
	// A gradient text effect.
	[ExecuteAlways]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TextMeshProGradient : MonoBehaviour
	{
		public enum GradientDirection
		{
			Vertical,
			Horizontal
		}

		public UnityEngine.Gradient gradient;
		public GradientDirection gradientDirection = GradientDirection.Horizontal;

		private TextMeshProUGUI textMeshProUGUI;

		private void Awake()
		{
			textMeshProUGUI = GetComponent<TextMeshProUGUI>();

			// Assign a default gradient if one is not assigned already.
			if (gradient == null)
			{
				gradient = new UnityEngine.Gradient();
				GradientColorKey[] colorKey = new GradientColorKey[2];
				GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

				colorKey[0].color = Color.white;
				colorKey[0].time = 0.0f;
				colorKey[1].color = Color.black;
				colorKey[1].time = 1.0f;

				alphaKey[0].alpha = 1.0f;
				alphaKey[0].time = 0.0f;
				alphaKey[1].alpha = 1.0f;
				alphaKey[1].time = 1.0f;

				gradient.SetKeys(colorKey, alphaKey);
			}
		}

		private void OnEnable()
		{
			UpdateGradient();
		}

		private void LateUpdate()
		{
			UpdateGradient();
		}

		private void UpdateGradient()
		{
			if (textMeshProUGUI == null)
			{
				return;
			}

			// Ensure the mesh is updated before applying the gradient.
				textMeshProUGUI.ForceMeshUpdate();

			TMP_TextInfo textInfo = textMeshProUGUI.textInfo;

			// Exit without applying the gradient if the text hasn't been rendered or updated yet.
			if (textInfo == null || textInfo.characterCount == 0)
			{
				return;
			}

			Color32[] newVertexColors;
			Vector3[] vertices;

			float minX = textMeshProUGUI.bounds.min.x;
			float maxX = textMeshProUGUI.bounds.max.x;
			float minY = textMeshProUGUI.bounds.min.y;
			float maxY = textMeshProUGUI.bounds.max.y;

			// Iterate over all the characters to apply a gradient based on the overall text width.
			for (int i = 0; i < textInfo.characterCount; i++)
			{
				TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
				if (!charInfo.isVisible)
					continue;

				int materialIndex = charInfo.materialReferenceIndex;
				newVertexColors = textInfo.meshInfo[materialIndex].colors32;
				vertices = textInfo.meshInfo[materialIndex].vertices;
				int vertexIndex = charInfo.vertexIndex;

				// Compute the gradient position for each vertex.
				float vertex0Offset = (gradientDirection == GradientDirection.Horizontal) ?
					Mathf.InverseLerp(minX, maxX, vertices[vertexIndex + 0].x) :
					Mathf.InverseLerp(minY, maxY, vertices[vertexIndex + 0].y);

				float vertex1Offset = (gradientDirection == GradientDirection.Horizontal) ?
					Mathf.InverseLerp(minX, maxX, vertices[vertexIndex + 1].x) :
					Mathf.InverseLerp(minY, maxY, vertices[vertexIndex + 1].y);

				float vertex2Offset = (gradientDirection == GradientDirection.Horizontal) ?
					Mathf.InverseLerp(minX, maxX, vertices[vertexIndex + 2].x) :
					Mathf.InverseLerp(minY, maxY, vertices[vertexIndex + 2].y);

				float vertex3Offset = (gradientDirection == GradientDirection.Horizontal) ?
					Mathf.InverseLerp(minX, maxX, vertices[vertexIndex + 3].x) :
					Mathf.InverseLerp(minY, maxY, vertices[vertexIndex + 3].y);

				// Apply the gradient color based on the calculated offsets.
				newVertexColors[vertexIndex + 0] = gradient.Evaluate(vertex0Offset);
				newVertexColors[vertexIndex + 1] = gradient.Evaluate(vertex1Offset);
				newVertexColors[vertexIndex + 2] = gradient.Evaluate(vertex2Offset);
				newVertexColors[vertexIndex + 3] = gradient.Evaluate(vertex3Offset);
			}

			// Update the mesh with the new color settings.
			textMeshProUGUI.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
		}
	}
}
