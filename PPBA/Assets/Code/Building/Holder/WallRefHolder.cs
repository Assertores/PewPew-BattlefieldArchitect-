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

	[HideInInspector] public Sprite _Image { get => _ImageUI; }
	[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
	[HideInInspector] public ObjectType _Type { get => _ObjectType; }
	[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
	[HideInInspector] public GameObject _blueprintObj { get => _blueprintPrefab; }

	[SerializeField] Material BaseMaterial;
	[SerializeField] private Renderer _myRenderer;
	private MaterialPropertyBlock _PropertyBlock;

	private void Awake()
	{
		if(null == _myRenderer)
		{
			//Renderer temp = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
			Renderer temp = GetComponentInChildren<Renderer>();
			if(null != temp)
				_myRenderer = temp;
		}
	}

	private int _teamBackingField;
	[HideInInspector]
	public int _team
	{
		get => _teamBackingField; set
		{
			if(_teamBackingField != value && null != _myRenderer && null != _PropertyBlock)
				BuildingColorSetter.SetMaterialColor(_myRenderer, _PropertyBlock, value);

			_teamBackingField = value;
		}
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

	private void OnEnable()
	{
		_PropertyBlock = new MaterialPropertyBlock();
		BuildingColorSetter.SetMaterialColor(_myRenderer, _PropertyBlock, _team);
	}
}

