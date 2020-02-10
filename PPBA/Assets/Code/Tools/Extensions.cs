using UnityEngine;


public static class ColorExtensions
{
	public static Vector4 ColorToVector4(this Color col )
	{
		Vector4 vec = new Vector4(col.r, col.g, col.b, col.a);
		return vec;
	}

	public static Vector4[] ColorToVector4(this Color[] col)
	{
		Vector4[] vecs = new Vector4[col.Length];

		for(int i = 0; i < col.Length; i++)
		{
			vecs[i] = new Vector4(col[i].r, col[i].g, col[i].b, col[i].a);
		}
		return vecs;
	}

}

