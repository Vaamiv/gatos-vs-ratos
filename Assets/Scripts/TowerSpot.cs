using UnityEngine;

namespace GatosVsRatos
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class TowerSpot : MonoBehaviour
    {
        public bool IsOccupied { get; private set; }
        private SpriteRenderer fill;
        private Color normalColor;

        private void Awake()
        {
            normalColor = new Color(1f, 0.9f, 0.45f, 0.42f);
            var outline = ArtFactory.SpriteObject("Outline", transform, ArtFactory.Circle, new Color(0.34f, 0.22f, 0.08f, 0.62f), new Vector2(1.28f, 1.28f), Vector3.zero, 2);
            fill = ArtFactory.SpriteObject("Fill", transform, ArtFactory.Circle, normalColor, new Vector2(1.08f, 1.08f), Vector3.zero, 3).GetComponent<SpriteRenderer>();
            ArtFactory.WorldText("Plus", transform, "+", Vector3.zero, 58, 0.055f, 5).color = new Color32(92, 65, 35, 255);
            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.64f;
            collider.isTrigger = true;
        }

        private void OnMouseDown()
        {
            if (!IsOccupied && GameApp.Instance != null) GameApp.Instance.TryBuild(this);
        }

        private void OnMouseEnter()
        {
            if (!IsOccupied) fill.color = new Color(1f, 0.98f, 0.62f, 0.8f);
        }

        private void OnMouseExit()
        {
            if (!IsOccupied) fill.color = normalColor;
        }

        public Tower Occupy(TowerKind kind)
        {
            IsOccupied = true;
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            GetComponent<Collider2D>().enabled = false;
            var towerObject = new GameObject(Balance.TowerName(kind));
            towerObject.transform.position = transform.position;
            towerObject.transform.SetParent(transform.parent, true);
            var tower = towerObject.AddComponent<Tower>();
            tower.Initialize(kind);
            return tower;
        }
    }
}
