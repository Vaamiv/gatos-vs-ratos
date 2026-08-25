using System.Collections.Generic;
using UnityEngine;

namespace GatosVsRatos
{
    public static class ArtFactory
    {
        private static Sprite circleSprite;
        private static Sprite squareSprite;
        private static Sprite triangleSprite;
        private static Material spriteMaterial;
        private static Font runtimeFont;

        public static Sprite Circle => circleSprite ??= MakeShape("Circle", 64, (x, y) => x * x + y * y <= 1f);
        public static Sprite Square => squareSprite ??= MakeShape("Square", 8, (x, y) => true);
        public static Sprite Triangle => triangleSprite ??= MakeShape("Triangle", 64, (x, y) => y >= -1f && y <= 1f - Mathf.Abs(x) * 2f);
        public static Material SpriteMaterial => spriteMaterial ??= new Material(Shader.Find("Sprites/Default"));
        public static Font RuntimeFont => runtimeFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private static Sprite MakeShape(string name, int size, System.Func<float, float, bool> inside)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name + "Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = ((x + 0.5f) / size) * 2f - 1f;
                    float ny = ((y + 0.5f) / size) * 2f - 1f;
                    pixels[y * size + x] = inside(nx, ny) ? Color.white : Color.clear;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            sprite.name = name + "Sprite";
            return sprite;
        }

        public static GameObject SpriteObject(string name, Transform parent, Sprite sprite, Color color,
            Vector2 size, Vector3 localPosition, int order = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return go;
        }

        public static LineRenderer Line(string name, Transform parent, IReadOnlyList<Vector3> points,
            float width, Color color, int order = 0, bool loop = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++) line.SetPosition(i, points[i]);
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = 12;
            line.numCornerVertices = 12;
            line.loop = loop;
            line.material = SpriteMaterial;
            line.sortingOrder = order;
            return line;
        }

        public static TextMesh WorldText(string name, Transform parent, string text, Vector3 localPosition,
            int fontSize = 44, float characterSize = 0.06f, int order = 30)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = RuntimeFont;
            mesh.fontSize = fontSize;
            mesh.characterSize = characterSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            mesh.GetComponent<MeshRenderer>().material = RuntimeFont.material;
            mesh.GetComponent<MeshRenderer>().sortingOrder = order;
            return mesh;
        }

        public static Transform CreateCat(Transform parent, TowerKind kind)
        {
            var cat = new GameObject("CatVisual").transform;
            cat.SetParent(parent, false);

            Color fur = Balance.TowerColor(kind);
            Color dark = Color.Lerp(fur, Color.black, 0.32f);
            Color cream = new Color32(255, 239, 205, 255);

            SpriteObject("Shadow", cat, Circle, new Color(0.12f, 0.18f, 0.12f, 0.25f), new Vector2(1.25f, 0.42f), new Vector3(0, -0.52f, 0), 2);
            SpriteObject("Body", cat, Circle, fur, new Vector2(0.88f, 0.85f), new Vector3(-0.08f, -0.13f, 0), 8);
            SpriteObject("Belly", cat, Circle, cream, new Vector2(0.45f, 0.5f), new Vector3(0.02f, -0.17f, 0), 9);
            SpriteObject("Tail", cat, Circle, dark, new Vector2(0.22f, 0.68f), new Vector3(-0.52f, -0.08f, 0), 7).transform.localRotation = Quaternion.Euler(0, 0, 34);
            SpriteObject("Head", cat, Circle, fur, new Vector2(0.78f, 0.7f), new Vector3(0, 0.35f, 0), 12);
            var earL = SpriteObject("EarL", cat, Triangle, fur, new Vector2(0.34f, 0.39f), new Vector3(-0.25f, 0.69f, 0), 11);
            var earR = SpriteObject("EarR", cat, Triangle, fur, new Vector2(0.34f, 0.39f), new Vector3(0.25f, 0.69f, 0), 11);
            SpriteObject("EarInnerL", earL.transform, Triangle, new Color32(245, 164, 166, 255), new Vector2(0.45f, 0.52f), new Vector3(0, -0.05f, 0), 12);
            SpriteObject("EarInnerR", earR.transform, Triangle, new Color32(245, 164, 166, 255), new Vector2(0.45f, 0.52f), new Vector3(0, -0.05f, 0), 12);
            SpriteObject("Muzzle", cat, Circle, cream, new Vector2(0.42f, 0.28f), new Vector3(0.08f, 0.23f, 0), 13);
            SpriteObject("EyeL", cat, Circle, Color.white, new Vector2(0.14f, 0.18f), new Vector3(-0.14f, 0.45f, 0), 14);
            SpriteObject("EyeR", cat, Circle, Color.white, new Vector2(0.14f, 0.18f), new Vector3(0.18f, 0.45f, 0), 14);
            SpriteObject("PupilL", cat, Circle, new Color32(30, 35, 42, 255), new Vector2(0.065f, 0.1f), new Vector3(-0.11f, 0.44f, 0), 15);
            SpriteObject("PupilR", cat, Circle, new Color32(30, 35, 42, 255), new Vector2(0.065f, 0.1f), new Vector3(0.21f, 0.44f, 0), 15);
            SpriteObject("Nose", cat, Triangle, new Color32(80, 45, 52, 255), new Vector2(0.12f, 0.1f), new Vector3(0.07f, 0.28f, 0), 16).transform.localRotation = Quaternion.Euler(0, 0, 180);

            var weapon = new GameObject("WeaponPivot").transform;
            weapon.SetParent(cat, false);
            weapon.localPosition = new Vector3(0.18f, -0.05f, 0);

            if (kind == TowerKind.Metralhadora)
            {
                SpriteObject("Grip", weapon, Square, new Color32(68, 74, 80, 255), new Vector2(0.18f, 0.42f), new Vector3(0.18f, -0.19f, 0), 18).transform.localRotation = Quaternion.Euler(0, 0, -18);
                SpriteObject("Receiver", weapon, Square, new Color32(60, 70, 82, 255), new Vector2(0.64f, 0.27f), new Vector3(0.32f, 0.03f, 0), 18);
                for (int i = -1; i <= 1; i++)
                    SpriteObject("Barrel", weapon, Square, new Color32(38, 42, 48, 255), new Vector2(0.55f, 0.055f), new Vector3(0.82f, 0.03f + i * 0.085f, 0), 17);
                SpriteObject("Muzzle", weapon, Circle, new Color32(25, 28, 33, 255), new Vector2(0.18f, 0.31f), new Vector3(1.08f, 0.03f, 0), 19);
            }
            else if (kind == TowerKind.Bazuca)
            {
                SpriteObject("Tube", weapon, Square, new Color32(53, 117, 92, 255), new Vector2(1.22f, 0.36f), new Vector3(0.46f, 0.13f, 0), 18);
                SpriteObject("Rear", weapon, Circle, new Color32(45, 55, 65, 255), new Vector2(0.22f, 0.52f), new Vector3(-0.17f, 0.13f, 0), 17);
                SpriteObject("Ring", weapon, Circle, new Color32(228, 178, 55, 255), new Vector2(0.2f, 0.48f), new Vector3(0.84f, 0.13f, 0), 19);
                var tip = SpriteObject("RocketTip", weapon, Triangle, new Color32(220, 70, 51, 255), new Vector2(0.37f, 0.38f), new Vector3(1.09f, 0.13f, 0), 20);
                tip.transform.localRotation = Quaternion.Euler(0, 0, -90);
            }
            else
            {
                SpriteObject("BeamL", weapon, Square, new Color32(104, 63, 37, 255), new Vector2(0.12f, 0.82f), new Vector3(0.15f, -0.18f, 0), 16).transform.localRotation = Quaternion.Euler(0, 0, -32);
                SpriteObject("BeamR", weapon, Square, new Color32(104, 63, 37, 255), new Vector2(0.12f, 0.82f), new Vector3(0.72f, -0.18f, 0), 16).transform.localRotation = Quaternion.Euler(0, 0, 32);
                SpriteObject("Crossbar", weapon, Square, new Color32(128, 79, 42, 255), new Vector2(0.82f, 0.11f), new Vector3(0.43f, -0.43f, 0), 17);
                SpriteObject("Arm", weapon, Square, new Color32(151, 93, 45, 255), new Vector2(0.12f, 0.95f), new Vector3(0.48f, 0.04f, 0), 18).transform.localRotation = Quaternion.Euler(0, 0, -38);
                SpriteObject("Stone", weapon, Circle, new Color32(83, 91, 98, 255), new Vector2(0.32f, 0.32f), new Vector3(0.83f, 0.42f, 0), 20);
            }

            return weapon;
        }

        public static void CreateRat(Transform parent, EnemyKind kind)
        {
            Color fur = kind switch
            {
                EnemyKind.Corredor => new Color32(180, 140, 96, 255),
                EnemyKind.Grandao => new Color32(92, 101, 116, 255),
                _ => new Color32(137, 145, 151, 255)
            };
            float scale = kind == EnemyKind.Grandao ? 1.18f : kind == EnemyKind.Corredor ? 0.88f : 1f;
            var visual = new GameObject("RatVisual").transform;
            visual.SetParent(parent, false);
            visual.localScale = Vector3.one * scale;

            Line("Tail", visual, new[] { new Vector3(-0.33f, -0.05f), new Vector3(-0.62f, 0.04f), new Vector3(-0.76f, 0.22f) }, 0.055f, new Color32(206, 132, 140, 255), 7);
            SpriteObject("Shadow", visual, Circle, new Color(0.1f, 0.1f, 0.1f, 0.22f), new Vector2(0.9f, 0.27f), new Vector3(0, -0.28f, 0), 6);
            SpriteObject("Body", visual, Circle, fur, new Vector2(0.72f, 0.52f), new Vector3(-0.08f, -0.02f, 0), 9);
            SpriteObject("Head", visual, Circle, Color.Lerp(fur, Color.white, 0.08f), new Vector2(0.47f, 0.43f), new Vector3(0.28f, 0.11f, 0), 10);
            SpriteObject("EarL", visual, Circle, new Color32(226, 153, 164, 255), new Vector2(0.23f, 0.23f), new Vector3(0.13f, 0.33f, 0), 11);
            SpriteObject("EarR", visual, Circle, new Color32(226, 153, 164, 255), new Vector2(0.21f, 0.21f), new Vector3(0.36f, 0.31f, 0), 11);
            SpriteObject("Eye", visual, Circle, Color.white, new Vector2(0.12f, 0.14f), new Vector3(0.36f, 0.15f, 0), 12);
            SpriteObject("Pupil", visual, Circle, new Color32(28, 31, 35, 255), new Vector2(0.055f, 0.075f), new Vector3(0.39f, 0.15f, 0), 13);
            SpriteObject("Nose", visual, Circle, new Color32(72, 41, 48, 255), new Vector2(0.11f, 0.11f), new Vector3(0.52f, 0.04f, 0), 13);
            if (kind == EnemyKind.Grandao)
            {
                SpriteObject("Helmet", visual, Circle, new Color32(75, 84, 96, 255), new Vector2(0.48f, 0.28f), new Vector3(0.22f, 0.28f, 0), 14);
            }
        }

        public static void Burst(Vector3 position, Color color, int count = 7)
        {
            for (int i = 0; i < count; i++)
            {
                var dot = SpriteObject("FxDot", null, Circle, color, Vector2.one * Random.Range(0.08f, 0.16f), position, 40);
                var fx = dot.AddComponent<FxDot>();
                float angle = (Mathf.PI * 2f * i / count) + Random.Range(-0.25f, 0.25f);
                fx.Velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(0.8f, 1.6f);
            }
        }
    }

    public sealed class FxDot : MonoBehaviour
    {
        public Vector3 Velocity;
        private SpriteRenderer spriteRenderer;
        private float life = 0.45f;

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void Update()
        {
            transform.position += Velocity * Time.deltaTime;
            Velocity *= 0.94f;
            life -= Time.deltaTime;
            var color = spriteRenderer.color;
            color.a = Mathf.Clamp01(life / 0.45f);
            spriteRenderer.color = color;
            if (life <= 0f) Destroy(gameObject);
        }
    }
}
