using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PPBA;


public class WallRefHolder : MonoBehaviour, IRefHolder
{
	public AudioClip _BuildingSound;
	[SerializeField] private Sprite _ImageUI;
	[SerializeField] private TextMeshProUGUI _TextField;
	[SerializeField] private ObjectType _ObjectType;
	[SerializeField] private GameObject _GhostPrefab;
	[SerializeField] private GameObject _blueprintPrefab;

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
	[HideInInspector] public GameObject _blueprintObj { get => _blueprintPrefab; }

	[SerializeField] Material BaseMaterial;

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


