using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dSpriteAnimationPreview 
{
	public enum GridTypes
	{
		LightChecked,
		MediumChecked,
		DarkChecked,
		BlackChecked,
		LightSolid,
		MediumSolid,
		DarkSolid,
		BlackSolid
	}

	public GridTypes GridType
	{
		get { return (GridTypes)tk2dPreferences.inst.animBackground; }
		set { if (tk2dPreferences.inst.animBackground != (int)value) { tk2dPreferences.inst.animBackground = (int)value; DestroyGridTexture(); } }
	}

	tk2dSpriteThumbnailCache spriteThumbnailRenderer = new tk2dSpriteThumbnailCache();

	private void Init()
	{
		if (gridTexture == null)
		{
			gridTexture = new Texture2D(16, 16);
			Color c0 = Color.white;
			Color c1 = new Color(0.8f, 0.8f, 0.8f, 1.0f);

			switch (GridType)
			{
				case GridTypes.LightChecked:  c0 = new Color32(255, 255, 255, 255); c1 = new Color32(204, 204, 204, 255); break;
				case GridTypes.MediumChecked: c0 = new Color32(153, 153, 153, 255); c1 = new Color32(102, 102, 102, 255); break;
				case GridTypes.DarkChecked:   c0 = new Color32( 51,  51,  51, 255); c1 = new Color32(102, 102, 102, 255); break;
				case GridTypes.BlackChecked:  c0 = new Color32(  0,   0,   0, 255); c1 = new Color32( 51,  51,  51, 255); break;
				case GridTypes.LightSolid:    c0 = new Color32(255, 255, 255, 255); c1 = c0; break;
				case GridTypes.MediumSolid:   c0 = new Color32(153, 153, 153, 255); c1 = c0; break;
				case GridTypes.DarkSolid:     c0 = new Color32( 51,  51,  51, 255); c1 = c0; break;
				case GridTypes.BlackSolid:    c0 = new Color32(  0,   0,   0, 255); c1 = c0; break;
			}

			for (int y = 0; y < gridTexture.height; ++y)
			{
				for (int x = 0; x < gridTexture.width; ++x)
				{
					bool xx = (x < gridTexture.width / 2);
					bool yy = (y < gridTexture.height / 2);
					gridTexture.SetPixel(x, y, (xx == yy)?c0:c1);
				}
			}
			gridTexture.Apply();
			gridTexture.filterMode = FilterMode.Point;
			gridTexture.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	void DestroyGridTexture()
	{
		if (gridTexture != null)
		{
			Object.DestroyImmediate(gridTexture);
			gridTexture = null;
		}
	}

	public void Destroy()
	{
		spriteThumbnailRenderer.Destroy();
		DestroyGridTexture();
	}

	void Repaint() { HandleUtility.Repaint(); }

	public int Frame { get; set; }
	Vector2 translate = Vector2.zero;
	float scale = 1.0f;
	bool dragging = false;

	public void ResetTransform()
	{
		scale = 1.0f;
		translate.Set(0, 0);
		Repaint();
	}

	// Background
	Texture2D gridTexture = null;

	public void Draw(Rect r, tk2dSpriteDefinition sprite)
	{
		Init();

		Event ev = Event.current;
		switch (ev.type)
		{
			case EventType.MouseDown:
				if (r.Contains(ev.mousePosition))
				{
					dragging = true;
					ev.Use();
				}
				break;
			case EventType.MouseDrag:
				if (dragging && r.Contains(ev.mousePosition)) 
				{
					translate += ev.delta;
					ev.Use();
					Repaint();
				}
				break;
			case EventType.MouseUp:
				dragging = false;
				break;
			case EventType.ScrollWheel:
				if (r.Contains(ev.mousePosition)) 
				{
					scale = Mathf.Clamp(scale + ev.delta.y * 0.1f, 0.1f, 10.0f);
					ev.Use();
					Repaint();
				}
				break;
		}

		// Draw grid	
		float scl = 16.0f;
		GUI.DrawTextureWithTexCoords(r, gridTexture, new Rect(-translate.x / scl, translate.y / scl, r.width / scl, r.height / scl), false);

		// Draw sprite
		if (sprite != null)
		{
			spriteThumbnailRenderer.DrawSpriteTextureCentered(r, sprite, translate, scale, Color.white);
		}
	}
}
