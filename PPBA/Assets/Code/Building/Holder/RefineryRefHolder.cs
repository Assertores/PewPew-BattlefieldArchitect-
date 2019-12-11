using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PPBA;

public class RefineryRefHolder : MonoBehaviour, IRefHolder
{

	public AudioClip _BuildingSound;
	[SerializeField] private Sprite _ImageUI;
	[SerializeField] private TextMeshProUGUI _TextField;
	[SerializeField] private ObjectType _ObjectType;
	[SerializeField] private GameObject _GhostPrefab;
	[SerializeField] private GameObject _blueprintPrefab;

	public float _HarvestRadius;
	public float _HarvestIntensity;
	public Vector2 _Positions;

	public float _BuildingTime;
	public float _BuildingCosts;
	public float _CurrentResources;

	public float _LivePoints;
	public float _MaxLivePoints;

	[SerializeField] Material BaseMaterial;

	[HideInInspector] public Sprite _Image { get => _ImageUI; }
	[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
	[HideInInspector] public ObjectType _Type { get => _ObjectType; }
	[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }
	[HideInInspector] public GameObject _blueprintObj { get => _blueprintPrefab; }

	public Vector4 GetShaderProperties()
	{
		return new Vector4(_Positions.x, _Positions.y, _HarvestIntensity, _HarvestRadius);
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


	//private void OnValidate()
	//{
	//	if(null == _blueprintPrefab)
			
	//}
}



