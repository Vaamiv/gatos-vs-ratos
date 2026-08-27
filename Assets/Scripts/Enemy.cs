using System.Collections.Generic;
using UnityEngine;

namespace GatosVsRatos
{
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public sealed class Enemy : MonoBehaviour
    {
        public EnemyKind Kind { get; private set; }
        public bool IsAlive { get; private set; }
        public float Progress { get; private set; }

        private IReadOnlyList<Vector3> path;
        private int waypoint;
        private float maxHealth;
        private float health;
        private float speed;
        private int baseDamage;
        private int bounty;
        private Transform healthFill;
        private Transform visual;

        public void Initialize(EnemyKind kind, IReadOnlyList<Vector3> waypoints, float healthMultiplier,
            float speedMultiplier, float bountyMultiplier = 1f, int baseDamageBonus = 0)
        {
            Kind = kind;
            path = waypoints;
            waypoint = 1;
            transform.position = path[0];

            switch (kind)
            {
                case EnemyKind.Corredor:
                    maxHealth = 30f * healthMultiplier;
                    speed = 1.75f * speedMultiplier;
                    baseDamage = 1;
                    bounty = 7;
                    break;
                case EnemyKind.Grandao:
                    maxHealth = 115f * healthMultiplier;
                    speed = 0.72f * speedMultiplier;
                    baseDamage = 3;
                    bounty = 15;
                    break;
                default:
                    maxHealth = 50f * healthMultiplier;
                    speed = 1.08f * speedMultiplier;
                    baseDamage = 1;
                    bounty = 9;
                    break;
            }
            baseDamage += baseDamageBonus;
            bounty = Mathf.Max(2, Mathf.RoundToInt(bounty * bountyMultiplier));
            health = maxHealth;
            IsAlive = true;

            var collider = GetComponent<CircleCollider2D>();
            collider.radius = kind == EnemyKind.Grandao ? 0.42f : 0.32f;
            collider.isTrigger = true;
            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;

            ArtFactory.CreateRat(transform, kind);
            visual = transform.Find("RatVisual");
            CreateHealthBar();
            GameApp.Instance.RegisterEnemy(this);
        }

        private void CreateHealthBar()
        {
            var bar = new GameObject("HealthBar").transform;
            bar.SetParent(transform, false);
            bar.localPosition = new Vector3(0, 0.62f, 0);
            ArtFactory.SpriteObject("Background", bar, ArtFactory.Square, new Color32(72, 38, 43, 255), new Vector2(0.76f, 0.095f), Vector3.zero, 28);
            healthFill = ArtFactory.SpriteObject("Fill", bar, ArtFactory.Square, new Color32(88, 211, 106, 255), new Vector2(0.72f, 0.06f), new Vector3(-0.02f, 0, 0), 29).transform;
        }

        private void Update()
        {
            if (!IsAlive || GameApp.Instance == null || GameApp.Instance.Phase != GamePhase.Playing) return;
            if (waypoint >= path.Count)
            {
                ReachBase();
                return;
            }

            Vector3 target = path[waypoint];
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(before, target, speed * Time.deltaTime);
            Vector3 direction = target - before;
            if (visual != null && Mathf.Abs(direction.x) > 0.02f)
            {
                Vector3 scale = visual.localScale;
                scale.x = Mathf.Abs(scale.x) * (direction.x >= 0 ? 1f : -1f);
                visual.localScale = scale;
            }

            float segmentLength = Vector3.Distance(path[waypoint - 1], target);
            float segmentDone = segmentLength <= 0.001f ? 1f : 1f - Vector3.Distance(transform.position, target) / segmentLength;
            Progress = (waypoint - 1) + Mathf.Clamp01(segmentDone);

            if (Vector3.SqrMagnitude(transform.position - target) < 0.0025f)
            {
                waypoint++;
                if (waypoint >= path.Count) ReachBase();
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            health -= amount;
            float ratio = Mathf.Clamp01(health / maxHealth);
            healthFill.localScale = new Vector3(0.72f * ratio, 0.06f, 1f);
            healthFill.localPosition = new Vector3(-0.38f * (1f - ratio), 0, 0);
            if (health <= 0f) Die(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsAlive && other.GetComponent<BaseGoal>() != null) ReachBase();
        }

        private void ReachBase()
        {
            if (!IsAlive) return;
            GameApp.Instance.EnemyReachedBase(baseDamage);
            Die(false);
        }

        private void Die(bool defeated)
        {
            if (!IsAlive) return;
            IsAlive = false;
            ArtFactory.Burst(transform.position, defeated ? new Color32(255, 208, 75, 255) : new Color32(229, 98, 82, 255), defeated ? 7 : 4);
            GameApp.Instance.UnregisterEnemy(this, defeated, defeated ? bounty : 0);
            Destroy(gameObject);
        }
    }

    public sealed class BaseGoal : MonoBehaviour { }
}
