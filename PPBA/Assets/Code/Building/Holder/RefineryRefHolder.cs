using TMPro;
using UnityEngine;
using PPBA;

public class RefineryRefHolder : MonoBehaviour, IRefHolder
{

	public AudioClip _BuildingSound;
	[SerializeField] private Sprite _ImageUI;
	[SerializeField] public GameObject _UIElement_;
	public GameObject _UIElement { get => _UIElement_; }
	[SerializeField] private TextMeshProUGUI _TextField;
	[SerializeField] private ObjectType _ObjectType;
	[SerializeField] private GameObject _GhostPrefab;
	[SerializeField] private GameObject LogicObject;

	public float _HarvestRadius;
	public float _HarvestIntensity;
	public Vector2 _Positions;

	public float _BuildingTime;
	public float _BuildingCosts;
	public float _CurrentResources;

	public float _LivePoints;
	public float _MaxLivePoints;

	[SerializeField] Material BaseMaterial;
	[SerializeField] private Renderer _myRenderer;
	private MaterialPropertyBlock _PropertyBlock;

	private void Awake()
	{
		if(null == _myRenderer)
		{
			Renderer temp = transform.GetChild(0).GetChild(0).GetComponent<Renderer>();
			if(null != temp)
				_myRenderer = temp;
		}
	}

	private int _teamBackingField;
	[HideInInspector] public int _team { get => _teamBackingField; set
		{
			if(_teamBackingField != value && null != _myRenderer && null != _PropertyBlock)
				BuildingColorSetter.SetMaterialColor(_myRenderer, _PropertyBlock, value);

			_teamBackingField = value;
		}
	}


	[HideInInspector] public Sprite _Image { get => _ImageUI; }
	[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
	[HideInInspector] public ObjectType _Type { get => _ObjectType; }
	[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
	[HideInInspector] public GameObject _LogicObj { get => LogicObject; }


	/// <summary>
	/// get the Properties for RessourceCalculate or set die Postion on die Textur
	/// </summary>
	public Vector4 GetShaderProperties
	{
		get { return new Vector4(_Positions.x, _Positions.y, _HarvestIntensity, _HarvestRadius); }
		set { _Positions = value; }
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



