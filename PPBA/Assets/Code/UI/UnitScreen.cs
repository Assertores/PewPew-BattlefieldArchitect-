using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitScreen : MonoBehaviour
{
	public Transform UnitCamera;
	[SerializeField]private TextMeshProUGUI AmmoText;
	[SerializeField]private Image healthbar;
	
	[Range(0,1)]private float healthValue;  

	public void SetHealth(float value)
	{
		healthValue = value;
		healthbar.fillAmount = healthValue;
	}

	public void SetAmmo(float value)
	{
		AmmoText.text = value.ToString();
	}

	public void SetCamera(Vector3 TargetPosition)
	{
		UnitCamera.position = TargetPosition;
	}
}
