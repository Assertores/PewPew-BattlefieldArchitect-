using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace PPBA
{
	public class RefineryRefHolder : MonoBehaviour, IUIElement
	{
		public AudioClip _BuildingSound;
		[SerializeField] private Sprite _ImageUI;
		[SerializeField] private TextMeshProUGUI _TextField;
		[SerializeField] ObjectType _ObjectType;
		[SerializeField] GameObject _GhostPrefab;

		public float _harvestRadius;
		public float _harvestIntensity;

		[HideInInspector] public Sprite _Image { get => _ImageUI; }
		[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
		[HideInInspector] public ObjectType _Type  { get => _ObjectType; }
		[HideInInspector] public GameObject _GhostPrefabObj  { get => _GhostPrefab; }
	}
}


