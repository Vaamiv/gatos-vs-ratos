using UnityEngine;

namespace GatosVsRatos
{
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class Tower : MonoBehaviour
    {
        public TowerKind Kind { get; private set; }
        public int Level { get; private set; }
        public int UpgradePrice => Level >= 3 ? 0 : Balance.UpgradeCost(Kind, Level,
            GameApp.Instance != null ? GameApp.Instance.CurrentDifficulty : Difficulty.Normal);

        private TowerStats stats;
        private Enemy target;
        private Transform weaponPivot;
        private GameObject rangeView;
        private Transform pips;
        private float nextShot;
        private float nextSearch;

        public void Initialize(TowerKind kind)
        {
            Kind = kind;
            Level = 1;
            stats = Balance.Tower(kind, Level);

            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.58f;
            collider.isTrigger = true;

            ArtFactory.SpriteObject("PlatformShadow", transform, ArtFactory.Circle, new Color(0.1f, 0.12f, 0.1f, 0.3f), new Vector2(1.38f, 0.52f), new Vector3(0, -0.48f, 0), 3);
            ArtFactory.SpriteObject("Platform", transform, ArtFactory.Circle, new Color32(213, 189, 133, 255), new Vector2(1.24f, 0.48f), new Vector3(0, -0.4f, 0), 4);
            ArtFactory.SpriteObject("PlatformTop", transform, ArtFactory.Circle, new Color32(245, 222, 165, 255), new Vector2(1.07f, 0.34f), new Vector3(0, -0.34f, 0), 5);
            weaponPivot = ArtFactory.CreateCat(transform, kind);

            rangeView = ArtFactory.SpriteObject("Range", transform, ArtFactory.Circle, new Color(1f, 0.86f, 0.3f, 0.13f), Vector2.one * stats.Range * 2f, Vector3.zero, 1);
            rangeView.SetActive(false);
            pips = new GameObject("LevelPips").transform;
            pips.SetParent(transform, false);
            pips.localPosition = new Vector3(0, 0.95f, 0);
            DrawPips();
        }

        private void Update()
        {
            if (GameApp.Instance == null || GameApp.Instance.Phase != GamePhase.Playing) return;
            if (Time.time >= nextSearch || target == null || !target.IsAlive || !InRange(target))
            {
                FindTarget();
                nextSearch = Time.time + 0.12f;
            }

            if (target == null) return;
            AimAt(target.transform.position);
            if (Time.time >= nextShot)
            {
                Projectile.Spawn(transform.position + (target.transform.position - transform.position).normalized * 0.55f,
                    target, Kind, stats.Damage, stats.ProjectileSpeed, stats.SplashRadius);
                nextShot = Time.time + stats.FireInterval;
                GameApp.Instance.Audio.Shoot(Kind);
            }
        }

        private void FindTarget()
        {
            target = null;
            float bestProgress = float.MinValue;
            var enemies = GameApp.Instance.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || !InRange(enemy)) continue;
                if (enemy.Progress > bestProgress)
                {
                    bestProgress = enemy.Progress;
                    target = enemy;
                }
            }
        }

        private bool InRange(Enemy enemy)
        {
            return (enemy.transform.position - transform.position).sqrMagnitude <= stats.Range * stats.Range;
        }

        private void AimAt(Vector3 point)
        {
            Vector3 direction = point - weaponPivot.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void OnMouseDown()
        {
            if (GameApp.Instance != null && GameApp.Instance.Phase == GamePhase.Playing)
                GameApp.Instance.SelectTower(this);
        }

        public bool TryUpgrade()
        {
            if (Level >= 3 || !GameApp.Instance.TrySpend(UpgradePrice)) return false;
            Level++;
            stats = Balance.Tower(Kind, Level);
            rangeView.transform.localScale = Vector3.one * stats.Range * 2f;
            DrawPips();
            ArtFactory.Burst(transform.position + Vector3.up * 0.55f, new Color32(255, 220, 74, 255), 10);
            GameApp.Instance.Audio.Upgrade();
            return true;
        }

        public void SetSelected(bool selected)
        {
            if (rangeView != null) rangeView.SetActive(selected);
        }

        private void DrawPips()
        {
            for (int i = pips.childCount - 1; i >= 0; i--) Destroy(pips.GetChild(i).gameObject);
            for (int i = 0; i < 3; i++)
            {
                Color color = i < Level ? new Color32(255, 217, 63, 255) : new Color32(105, 108, 109, 180);
                ArtFactory.SpriteObject("Pip", pips, ArtFactory.Circle, color, new Vector2(0.14f, 0.14f), new Vector3((i - 1) * 0.18f, 0, 0), 27);
            }
        }
    }
}
