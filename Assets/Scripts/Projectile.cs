using UnityEngine;

namespace GatosVsRatos
{
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour
    {
        private Enemy target;
        private TowerKind kind;
        private float damage;
        private float speed;
        private float splash;
        private bool impacted;
        private float life = 5f;

        public static Projectile Spawn(Vector3 position, Enemy target, TowerKind kind, float damage, float speed, float splash)
        {
            var go = new GameObject(kind + "Projectile");
            go.transform.position = position;
            var projectile = go.AddComponent<Projectile>();
            projectile.target = target;
            projectile.kind = kind;
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.splash = splash;
            projectile.CreateVisual();
            return projectile;
        }

        private void Awake()
        {
            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.13f;
            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
        }

        private void CreateVisual()
        {
            if (kind == TowerKind.Metralhadora)
            {
                ArtFactory.SpriteObject("Pellet", transform, ArtFactory.Circle, new Color32(255, 225, 74, 255), new Vector2(0.16f, 0.16f), Vector3.zero, 24);
            }
            else if (kind == TowerKind.Bazuca)
            {
                ArtFactory.SpriteObject("Rocket", transform, ArtFactory.Square, new Color32(65, 87, 88, 255), new Vector2(0.4f, 0.16f), Vector3.zero, 24);
                var tip = ArtFactory.SpriteObject("Tip", transform, ArtFactory.Triangle, new Color32(232, 72, 49, 255), new Vector2(0.18f, 0.18f), new Vector3(0.25f, 0, 0), 25);
                tip.transform.localRotation = Quaternion.Euler(0, 0, -90);
            }
            else
            {
                ArtFactory.SpriteObject("Stone", transform, ArtFactory.Circle, new Color32(91, 96, 103, 255), new Vector2(0.28f, 0.28f), Vector3.zero, 24);
            }
        }

        private void Update()
        {
            if (impacted) return;
            life -= Time.deltaTime;
            if (life <= 0 || target == null || !target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = target.transform.position - transform.position;
            if (kind == TowerKind.Bazuca && direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
            if (direction.sqrMagnitude <= Mathf.Max(0.04f, step * step * 1.5f)) Impact();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!impacted && other.GetComponent<Enemy>() == target) Impact();
        }

        private void Impact()
        {
            if (impacted) return;
            impacted = true;
            Color fxColor = kind == TowerKind.Catapulta ? new Color32(196, 168, 127, 255) : new Color32(255, 151, 54, 255);
            ArtFactory.Burst(transform.position, fxColor, kind == TowerKind.Metralhadora ? 3 : 8);
            GameApp.Instance.Audio.Impact();

            if (splash > 0f)
            {
                var enemies = GameApp.Instance.Enemies;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    Enemy enemy = enemies[i];
                    if (enemy != null && enemy.IsAlive && (enemy.transform.position - transform.position).sqrMagnitude <= splash * splash)
                        enemy.TakeDamage(damage);
                }
            }
            else if (target != null && target.IsAlive)
            {
                target.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
