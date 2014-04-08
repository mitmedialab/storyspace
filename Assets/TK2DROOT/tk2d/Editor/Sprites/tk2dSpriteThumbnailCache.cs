using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dSpriteThumbnailCache
{
	public tk2dSpriteThumbnailCache()
	{
		System.Type guiClipType = System.Type.GetType("UnityEngine.GUIClip,UnityEngine");
		if (guiClipType != null)
			guiClipVisibleRectProperty = guiClipType.GetProperty("visibleRect");

		mat = new Material(Shader.Find("Hidden/tk2d/EditorUtility"));
		mat.hideFlags = HideFlags.DontSave;
	}

	public void Destroy() 
	{
		Object.DestroyImmediate(mat);
	}

	public Vector2 GetSpriteSizePixels(tk2dSpriteDefinition def)
	{
		return new Vector2(def.untrimmedBoundsData[1].x / def.texelSize.x, def.untrimmedBoundsData[1].y / def.texelSize.y);
	}

	public void DrawSpriteTexture(Rect rect, tk2dSpriteDefinition def)
	{
		DrawSpriteTexture(rect, def, Color.white);		
	}

	public void DrawSpriteTexture(Rect rect, tk2dSpriteDefinition def, Color tint)
	{
		Vector2 pixelSize = new Vector3( rect.width / def.untrimmedBoundsData[1].x, rect.height / def.untrimmedBoundsData[1].y);

		Rect visibleRect = VisibleRect;
		Vector4 clipRegion = new Vector4(visibleRect.x, visibleRect.y, visibleRect.x + visibleRect.width, visibleRect.y + visibleRect.height);

		bool visible = true;
		if (rect.xMin > visibleRect.xMax || rect.yMin > visibleRect.yMax ||
			rect.xMax < visibleRect.xMin || rect.yMax < visibleRect.yMin) 
			visible = false;

		if (Event.current.type == EventType.Repaint && visible)
		{
			Mesh tmpMesh = new Mesh();
			tmpMesh.vertices = def.positions;
			tmpMesh.uv = def.uvs;
			tmpMesh.triangles = def.indices;
			tmpMesh.RecalculateBounds();
			tmpMesh.RecalculateNormals();

			Vector3 t = def.untrimmedBoundsData[1] * 0.5f - def.untrimmedBoundsData[0];
			float tq = def.untrimmedBoundsData[1].y;

			mat.mainTexture = def.material.mainTexture;
			mat.SetColor("_Tint", tint);
			mat.SetVector("_Clip", clipRegion);

			Matrix4x4 m = new Matrix4x4();
			m.SetTRS(new Vector3(rect.x + t.x * pixelSize.x, rect.y + (tq - t.y) * pixelSize.y, 0), 
				Quaternion.identity, 
				new Vector3(pixelSize.x, -pixelSize.y, 1));

			mat.SetPass(0);
			Graphics.DrawMeshNow(tmpMesh, m * GUI.matrix);

			Object.DestroyImmediate(tmpMesh);
		}
	}

	public void DrawSpriteTextureCentered(Rect rect, tk2dSpriteDefinition def, Vector2 translate, float scale, Color tint)
	{
		Vector2 pixelSize = new Vector3( 1.0f / def.texelSize.x, 1.0f / def.texelSize.y);

		Rect visibleRect = VisibleRect;
		visibleRect = rect;
		Vector4 clipRegion = new Vector4(visibleRect.x, visibleRect.y, visibleRect.x + visibleRect.width, visibleRect.y + visibleRect.height);

		bool visible = true;
		// if (rect.xMin > visibleRect.xMax || rect.yMin > visibleRect.yMax ||
		// 	rect.xMax < visibleRect.xMin || rect.yMax < visibleRect.yMin) 
		// 	visible = false;

		if (Event.current.type == EventType.Repaint && visible)
		{
			Mesh tmpMesh = new Mesh();
			tmpMesh.vertices = def.positions;
			tmpMesh.uv = def.uvs;
			tmpMesh.triangles = def.indices;
			tmpMesh.RecalculateBounds();
			tmpMesh.RecalculateNormals();

			mat.mainTexture = def.material.mainTexture;
			mat.SetColor("_Tint", tint);
			mat.SetVector("_Clip", clipRegion);

			Matrix4x4 m = new Matrix4x4();
			m.SetTRS(new Vector3(rect.x + rect.width / 2.0f + translate.x, rect.y + rect.height / 2.0f + translate.y, 0), 
				Quaternion.identity, 
				new Vector3(pixelSize.x * scale, -pixelSize.y * scale, 1));

			mat.SetPass(0);
			Graphics.DrawMeshNow(tmpMesh, m * GUI.matrix);

			Object.DestroyImmediate(tmpMesh);
		}
	}





	Material mat;

	// Innards
	System.Reflection.PropertyInfo guiClipVisibleRectProperty = null;
	Rect VisibleRect 
	{
		get
		{
			if (guiClipVisibleRectProperty != null)
				return (Rect)guiClipVisibleRectProperty.GetValue(null, null);
			else
				return new Rect(-1.0e32f, -1.0e32f, 2.0e32f, 2.0e32f); // not so graceful fallback, but a fallback nonetheless
		}
	}
}

