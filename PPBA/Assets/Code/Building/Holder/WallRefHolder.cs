using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PPBA;


public class WallRefHolder : MonoBehaviour, IUIElement
{
	public AudioClip _BuildingSound;
	[SerializeField] private Sprite _ImageUI;
	[SerializeField] private TextMeshProUGUI _TextField;
	[SerializeField] private ObjectType _ObjectType;
	[SerializeField] private GameObject _GhostPrefab;


	public GameObject GhostWallMiddlePrefab;
	public GameObject WallEndPrefab;
	public GameObject WallMiddlePrefab;

	public float _BuildingTime;
	public float _BuildingCosts;

	public float _LivePoints;
	public float _MaxLivePoints;

	[HideInInspector] public Sprite _Image { get => _ImageUI; }
	[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
	[HideInInspector] public ObjectType _Type { get => _ObjectType; }
	[HideInInspector] public GameObject _GhostPrefabObj { get => _GhostPrefab; }

}


