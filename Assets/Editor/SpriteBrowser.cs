using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SpriteBrowser : EditorWindow
{
    private GameObject selectedPrefab;
    [SerializeField]
    private List<LineData> spriteList = new List<LineData>();
    private Vector2 scrollPos;

    [MenuItem("Window/Sprite Browser")]
    public static void ShowWindow()
    {
        GetWindow<SpriteBrowser>("Sprite Browser");
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        using (new EditorGUILayout.HorizontalScope())
        {
            selectedPrefab = EditorGUILayout.ObjectField("Prefab", selectedPrefab, typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("refresh"))
            {
                GetSpriteList();
            }

            if (selectedPrefab != null && GUILayout.Button("replace toggled"))
            {
                ReplaceToggled();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            GetSpriteList();
        }
        if (selectedPrefab != null)
        {
            ListSprites();
        }
    }

    private bool IsContainSprite(Sprite sp)
    {
        foreach (var data in spriteList)
        {
            if (data.sprite == sp)
                return true;
        }
        return false;
    }

    private void AddSprite(Sprite sp, Image image)
    {
        foreach (var data in spriteList)
        {
            if (data.sprite == sp)
            {
                data.refImages.Add(image);
            }
        }
    }

    private void GetSpriteList()
    {
        if (selectedPrefab == null)
        {
            return;
        }
        spriteList.Clear();
        var images = selectedPrefab.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            var sprite = image.sprite;
            if (sprite == null)
                continue;
            if (!IsContainSprite(sprite))
            {
                var lineData = new LineData();
                lineData.sprite = sprite;
                lineData.refImages = new List<Image>();
                lineData.toggled = false;
                spriteList.Add(lineData);
            }
            AddSprite(sprite, image);
        }
    }



    private void ListSprites()
    {
        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
        {
            GUIStyle rightAlignedStyle = new GUIStyle(GUI.skin.label);
            GUIStyle leftAlignedStyle = new GUIStyle(GUI.skin.label);

            rightAlignedStyle.alignment = TextAnchor.MiddleRight;
            rightAlignedStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("origional sprite", rightAlignedStyle, GUILayout.MaxWidth(100));

            EditorGUILayout.Space();
            leftAlignedStyle.alignment = TextAnchor.MiddleLeft;
            leftAlignedStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label("replace texture", leftAlignedStyle, GUILayout.MaxWidth(100));
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < spriteList.Count; i++)
        {
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(130)))
            {
                var data = spriteList[i];
                var sprite = data.sprite;
                var assetPath = AssetDatabase.GetAssetPath(sprite);
                var texture = sprite.texture;

                var texRect = GUILayoutUtility.GetRect(new GUIContent(texture), GUI.skin.box, GUILayout.Width(128), GUILayout.Height(128));
                GUI.Box(texRect, texture, GUI.skin.box);
                GUILayout.Label(assetPath);
                EditorGUILayout.Space();
                EditorGUI.BeginChangeCheck();
                data.repalceTex = (Texture)EditorGUILayout.ObjectField(data.repalceTex, typeof(Texture), true, GUILayout.Width(128), GUILayout.Height(128));
                if (EditorGUI.EndChangeCheck())
                {
                    data.toggled = true;
                }

                data.toggled = EditorGUILayout.Toggle(data.toggled, GUILayout.Width(20));

                Event e = Event.current;
                if (e.type == EventType.MouseDown && texRect.Contains(e.mousePosition))
                {
                    e.Use();

                    EditorGUIUtility.PingObject(sprite.texture);
                }
            }

        }

        EditorGUILayout.EndScrollView();
    }

    private void ReplaceToggled()
    {
        foreach (var data in spriteList)
        {
            if (data.toggled)
            {
                data.toggled = false;
                foreach (var image in data.refImages)
                {
                    image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(data.repalceTex));
                }
            }
        }
    }

    [Serializable]
    private class LineData
    {
        public Sprite sprite;
        public List<Image> refImages;
        public bool toggled;
        public Texture repalceTex;
    }
}
