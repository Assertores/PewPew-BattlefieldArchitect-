using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{

	public class DepotHolder : MonoBehaviour, IRefHolder
	{
		public AudioClip _BuildingSound;
		[SerializeField] private Sprite _ImageUI;
		[SerializeField] public GameObject _UIElement_;
		public GameObject _UIElement { get => _UIElement_; }
		[SerializeField] private TextMeshProUGUI _TextField;
		[SerializeField] private ObjectType _ObjectType;
		[SerializeField] private GameObject _GhostPrefab;
		[SerializeField] private GameObject LogicObject;

		public float _BuildingTime;
		public float _BuildingCosts;
		public float _CurrentResources;

		public float _LivePoints;
		public float _MaxLivePoints;

		[HideInInspector] public int _team { get; set; }
		[HideInInspector] public Sprite _Image { get => _ImageUI; }
		[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
		[HideInInspector] public ObjectType _Type { get => _ObjectType; }
		[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
		[HideInInspector] public GameObject _LogicObj { get => LogicObject; }

		[SerializeField] Material BaseMaterial;

		public int AddResources(int value)
		{
			return _LogicObj.GetComponent<ResourceDepot>().GiveResources(value);
		}

		public Vector4 GetShaderProperties
		{
			get { return Vector4.zero; }
			set { }
		}

		public float GetBuildingCost()
		{
			return _BuildingCosts;
		}

		public float GetBuildingCurrentResources()
		{
			return _CurrentResources;
		}

		public float GetBuildingTime()
		{
			return _BuildingTime;
		}

		public ObjectType GetObjectType()
		{
			return _ObjectType;
		}
		public Material GetMaterial() => BaseMaterial;
	}
}
