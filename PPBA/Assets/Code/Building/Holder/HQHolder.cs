﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PPBA
{
	public class HQHolder : MonoBehaviour, IRefHolder
	{
		public AudioClip _BuildingSound;
		[SerializeField] private Sprite _ImageUI;
		[SerializeField] private TextMeshProUGUI _TextField;
		[SerializeField] private ObjectType _ObjectType;
		[SerializeField] private GameObject _GhostPrefab;
		[SerializeField] private GameObject _blueprintPrefab;
		[SerializeField] private GameObject _logicObject;

		[HideInInspector] public int _team { get; set; }
		[HideInInspector] public Sprite _Image { get => _ImageUI; }
		[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
		[HideInInspector] public ObjectType _Type { get => _ObjectType; }
		[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
		[HideInInspector] public GameObject _blueprintObj { get => _blueprintPrefab; }

		[SerializeField] Material BaseMaterial;

		public Vector2 _Positions;

		public Vector4 GetShaderProperties
		{
			get { return Vector4.zero; }
			set { _Positions = value; }
		}

		public ObjectType GetObjectType()
		{
			return _ObjectType;
		}

		public Material GetMaterial() => BaseMaterial;

		private void OnEnable()
		{
			_logicObject.SetActive(true);
		}

		public void HQisBuiding()
		{
			UiInventory.s_instance.AddLastBuildings();
			UiInventory.s_instance.RemoveStartBuildings();
		}

	}
}
