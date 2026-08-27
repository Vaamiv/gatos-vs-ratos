using UnityEngine;

namespace GatosVsRatos
{
    public enum TowerKind { Metralhadora, Bazuca, Catapulta }
    public enum EnemyKind { Comum, Corredor, Grandao }
    public enum Difficulty { Normal, Dificil, Insano }
    public enum GamePhase { Menu, Map, Playing, Victory, Defeat }

    public readonly struct TowerStats
    {
        public readonly float Range;
        public readonly float FireInterval;
        public readonly float Damage;
        public readonly float ProjectileSpeed;
        public readonly float SplashRadius;

        public TowerStats(float range, float interval, float damage, float speed, float splash = 0f)
        {
            Range = range;
            FireInterval = interval;
            Damage = damage;
            ProjectileSpeed = speed;
            SplashRadius = splash;
        }
    }

    public static class Balance
    {
        public const float SellRefundRate = 0.3f;

        public static int BuildCost(TowerKind kind, Difficulty difficulty = Difficulty.Normal)
        {
            int baseCost = kind switch
            {
                TowerKind.Metralhadora => 80,
                TowerKind.Bazuca => 140,
                _ => 120
            };
            float difficultyFactor = difficulty switch
            {
                Difficulty.Dificil => 1.05f,
                Difficulty.Insano => 1.15f,
                _ => 1f
            };
            return Mathf.RoundToInt(baseCost * difficultyFactor / 5f) * 5;
        }

        public static int UpgradeCost(TowerKind kind, int currentLevel, Difficulty difficulty = Difficulty.Normal)
        {
            float multiplier = currentLevel == 1 ? 0.9f : 1.3f;
            float difficultyFactor = difficulty switch
            {
                Difficulty.Dificil => 1.15f,
                Difficulty.Insano => 1.35f,
                _ => 1f
            };
            int normalBuildCost = BuildCost(kind, Difficulty.Normal);
            return Mathf.RoundToInt(normalBuildCost * multiplier * difficultyFactor / 5f) * 5;
        }

        public static int SellRefund(int totalInvested)
        {
            return Mathf.FloorToInt(Mathf.Max(0, totalInvested) * SellRefundRate);
        }

        public static TowerStats Tower(TowerKind kind, int level)
        {
            int extra = Mathf.Clamp(level, 1, 3) - 1;
            return kind switch
            {
                TowerKind.Metralhadora => new TowerStats(2.75f + extra * 0.18f, 0.26f - extra * 0.035f, 7f + extra * 5f, 12f),
                TowerKind.Bazuca => new TowerStats(4.2f + extra * 0.25f, 2.15f - extra * 0.2f,
                    extra == 0 ? 65f : extra == 1 ? 105f : 150f, 7f),
                _ => new TowerStats(3.55f + extra * 0.22f, 1.55f - extra * 0.14f,
                    23f + extra * 14f, 6.2f, 1.25f + extra * 0.22f)
            };
        }

        public static float DamageMultiplier(TowerKind tower, EnemyKind enemy)
        {
            if (enemy != EnemyKind.Grandao) return 1f;
            return tower switch
            {
                TowerKind.Metralhadora => 0.75f,
                TowerKind.Bazuca => 1.40f,
                _ => 1f
            };
        }

        public static string TowerName(TowerKind kind)
        {
            return kind switch
            {
                TowerKind.Metralhadora => "Gato Metralha",
                TowerKind.Bazuca => "Gato Bazuca",
                _ => "Gato Catapulta"
            };
        }

        public static string TowerRole(TowerKind kind)
        {
            return kind switch
            {
                TowerKind.Metralhadora => "Muito rápido • corredores • fraca contra grandões",
                TowerKind.Bazuca => "Longo alcance • +40% contra grandões",
                _ => "Área ampliada • ideal contra grupos"
            };
        }

        public static Color TowerColor(TowerKind kind)
        {
            return kind switch
            {
                TowerKind.Metralhadora => new Color32(242, 139, 42, 255),
                TowerKind.Bazuca => new Color32(112, 122, 142, 255),
                _ => new Color32(232, 180, 98, 255)
            };
        }
    }
}
