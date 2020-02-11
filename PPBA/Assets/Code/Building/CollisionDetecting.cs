using UnityEngine;

namespace PPBA
{
	public class CollisionDetecting : MonoBehaviour
	{
		[SerializeField] private int _FaultBuildingLayer = 9;
		[SerializeField] private Material GhostMaterial;

		private Color GhostGreenColor;
		private Color GhostRedColor;
		private Material ground;
		private Texture2D groundTex;
		private int team;
		private bool canThisBuild = true;
		private bool canBuildColor = false;
		//public Texture terTex;
		private void Start()
		{
			GhostGreenColor = BuildingManager.s_instance._ghostGreenColor;
			GhostRedColor = BuildingManager.s_instance._ghostRedColor;
			GhostMaterial.SetColor("_Color", GhostGreenColor);
			ground = HeatMapCalcRoutine.s_instance._GroundMaterial;
			team = GlobalVariables.s_instance._clients[0]._id;
		}

		private void Update()
		{

			if(canThisBuild == true)
			{
				Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(this.transform);
				//groundTex = new Texture2D(1, 1, TextureFormat.RGB24, false);
				//Rect rectReadPicture = new Rect(0, 0, 1, 1);
				//RenderTexture.active = ground.GetTexture("_TerritorriumMap") as RenderTexture;
				//Color col = groundTex.GetPixel(0, 0);
				//groundTex.Apply();
				//RenderTexture.active = null; // added to avoid errors 
				//							 //	Color col = groundTex.GetPixel((int)pos.x, (int)pos.y);

				float rValue = (HeatMapHandler.s_instance.GetHMValue(1, (int)pos.x, (int)pos.y))-2;

				print("rValue : " + rValue + "team " + team);

				if(rValue == team)
				{
					BuildingManager.s_instance._canBuild = true;
					GhostMaterial.SetColor("_Color", GhostGreenColor);
				}
				else
				{
					//print("nixs");
					BuildingManager.s_instance._canBuild = false;
					GhostMaterial.SetColor("_Color", GhostRedColor);
				}
			}
			else
			{
				print("nixs");
				BuildingManager.s_instance._canBuild = false;
				GhostMaterial.SetColor("_Color", GhostRedColor);
			}
		}

		private void OnTriggerStay(Collider other)
		{
			if(other.gameObject.layer == _FaultBuildingLayer)
			{
				//isCollision = true;
				canThisBuild = false;
				//	BuildingManager.s_instance._canBuild = false;
				//	GhostMaterial.SetColor("_Color", GhostRedColor);


			}
		}

		private void OnTriggerExit(Collider other)
		{
			if(other.gameObject.layer == _FaultBuildingLayer)
			{
				//isCollision = false;
				canThisBuild = true;
				//	BuildingManager.s_instance._canBuild = true;
				//	GhostMaterial.SetColor("_Color", GhostGreenColor);
			}
		}

	}
}
