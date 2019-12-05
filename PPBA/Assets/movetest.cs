using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPBA;

public class movetest : MonoBehaviour
{
	public int team;
	public int index;
	public Transform pos1;
	public Transform pos2;
	private Transform target;
	public float speed = 2;

    // Start is called before the first frame update
    void Start()
    {
		target = pos1;
		Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint();
		TerritoriumMapCalculate.s_instance.AddSoldier( pos, team);

    }

    // Update is called once per frame
    void Update()
    {
		float dis = Vector3.Distance(target.position, transform.position);

		if(dis <= 0.2f)
		{
			if(target == pos1 )
			{
				target = pos2;
			}
			else
			{
				target = pos1;
			}
		}
		Vector2 pos = UserInputController.s_instance.GetTexturePixelPoint(transform);
		transform.position = Vector3.MoveTowards(transform.position, target.position, speed* Time.deltaTime);
		TerritoriumMapCalculate.s_instance.UpdateSoldiersPosition(index, pos);
		TerritoriumMapCalculate.s_instance.RefreshCalcTerritorium();
	}
}
