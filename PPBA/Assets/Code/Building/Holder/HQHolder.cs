using System.Collections;
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
		[SerializeField] private GameObject LogicObject;
		[SerializeField] private GameObject _logicObject;

		[HideInInspector] public Sprite _Image { get => _ImageUI; }
		[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
		[HideInInspector] public ObjectType _Type { get => _ObjectType; }
		[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
		[HideInInspector] public GameObject _LogicObj { get => LogicObject; }

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
			_PropertyBlock = new MaterialPropertyBlock();
			BuildingColorSetter.SetMaterialColor(_myRenderer, _PropertyBlock, _team);
		}

		public void HQisBuiding()
		{
			//if(_team != GlobalVariables.s_instance._clients[0]._id)
			//	return;

			//UiInventory.s_instance.AddLastBuildings();
			//UiInventory.s_instance.RemoveStartBuildings();
		}
	}
}
