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
		[SerializeField] private ObjectType _ObjectType;
		[SerializeField] private GameObject _GhostPrefab;

		public GameObject RefineryPrefab;
		public float _HarvestRadius;
		public float _HarvestIntensity;
		public Vector2 _Positions;
		

		[HideInInspector] public Sprite _Image { get => _ImageUI; }
		[HideInInspector] public TextMeshProUGUI _ToolTipFeld { get => _TextField; }
		[HideInInspector] public ObjectType _Type  { get => _ObjectType; }
		[HideInInspector] public GameObject _GhostPrefabObj  { get => _GhostPrefab; }
	
		public Vector4 GetShaderProperties()
		{
			return new Vector4(_Positions.x , _Positions.y , _HarvestIntensity , _HarvestRadius);
		}
 
 }
}


