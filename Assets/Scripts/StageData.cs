using UnityEngine;

namespace GatosVsRatos
{
    public sealed class StageDefinition
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Description;
        public readonly Color GroundColor;
        public readonly Color PathColor;
        public readonly Color PathOutlineColor;
        public readonly Color AccentColor;
        public readonly Vector3[] Path;
        public readonly Vector3[] TowerSpots;

        public StageDefinition(int index, string name, string description, Color ground, Color path,
            Color outline, Color accent, Vector3[] waypoints, Vector3[] towerSpots)
        {
            Index = index;
            Name = name;
            Description = description;
            GroundColor = ground;
            PathColor = path;
            PathOutlineColor = outline;
            AccentColor = accent;
            Path = waypoints;
            TowerSpots = towerSpots;
        }
    }

    public static class Campaign
    {
        public static readonly StageDefinition[] Stages =
        {
            new(0, "Quintal Ensolarado", "O começo da invasão. Um caminho longo e equilibrado.",
                new Color32(121, 190, 104, 255), new Color32(225, 184, 118, 255), new Color32(130, 91, 54, 255), new Color32(255, 211, 74, 255),
                new[]
                {
                    new Vector3(-10.2f, -2.15f), new Vector3(-6.65f, -2.15f), new Vector3(-6.65f, 2.45f),
                    new Vector3(-2.45f, 2.45f), new Vector3(-2.45f, -1.5f), new Vector3(1.25f, -1.5f),
                    new Vector3(1.25f, 2.05f), new Vector3(4.95f, 2.05f), new Vector3(4.95f, -0.35f),
                    new Vector3(8.72f, -0.35f)
                },
                new[]
                {
                    new Vector3(-8.25f, -0.55f), new Vector3(-5.15f, -0.05f), new Vector3(-4.7f, 3.7f), new Vector3(-3.85f, 0.3f),
                    new Vector3(-0.55f, -3.05f), new Vector3(-0.42f, 0.2f), new Vector3(2.82f, 0.28f), new Vector3(3.55f, 3.55f),
                    new Vector3(6.15f, 1.05f), new Vector3(6.55f, -2.1f)
                }),

            new(1, "Horta Secreta", "Os ratos descobriram os canteiros. Curvas curtas exigem alcance.",
                new Color32(105, 168, 91, 255), new Color32(199, 154, 93, 255), new Color32(102, 69, 39, 255), new Color32(236, 124, 71, 255),
                new[]
                {
                    new Vector3(-10.2f, -2.8f), new Vector3(-7.25f, -2.8f), new Vector3(-7.25f, 1.85f),
                    new Vector3(-4.6f, 1.85f), new Vector3(-4.6f, -1.0f), new Vector3(-1.15f, -1.0f),
                    new Vector3(-1.15f, 3.0f), new Vector3(2.15f, 3.0f), new Vector3(2.15f, 0.0f),
                    new Vector3(5.0f, 0.0f), new Vector3(5.0f, -2.05f), new Vector3(8.72f, -2.05f)
                },
                new[]
                {
                    new Vector3(-8.65f, -0.35f), new Vector3(-5.85f, 3.25f), new Vector3(-5.85f, 0.05f), new Vector3(-3.0f, 0.55f),
                    new Vector3(-2.7f, -2.7f), new Vector3(0.45f, 1.0f), new Vector3(3.65f, 1.55f), new Vector3(3.45f, -1.65f),
                    new Vector3(6.35f, -0.5f), new Vector3(6.7f, -3.35f)
                }),

            new(2, "Parque do Lago", "Pontes estreitas e corredores rápidos ao redor da água.",
                new Color32(93, 169, 135, 255), new Color32(214, 190, 139, 255), new Color32(99, 81, 61, 255), new Color32(69, 171, 205, 255),
                new[]
                {
                    new Vector3(-10.2f, 2.7f), new Vector3(-7.1f, 2.7f), new Vector3(-7.1f, -2.45f),
                    new Vector3(-3.8f, -2.45f), new Vector3(-3.8f, 1.4f), new Vector3(-0.4f, 1.4f),
                    new Vector3(-0.4f, -2.55f), new Vector3(3.05f, -2.55f), new Vector3(3.05f, 2.3f),
                    new Vector3(6.15f, 2.3f), new Vector3(6.15f, -0.25f), new Vector3(8.72f, -0.25f)
                },
                new[]
                {
                    new Vector3(-8.65f, 0.2f), new Vector3(-5.45f, 3.7f), new Vector3(-5.45f, -0.25f), new Vector3(-2.15f, -0.45f),
                    new Vector3(-2.0f, 3.05f), new Vector3(1.25f, -0.55f), new Vector3(1.3f, 3.35f), new Vector3(4.65f, 0.25f),
                    new Vector3(7.45f, 1.25f), new Vector3(7.25f, -2.1f)
                }),

            new(3, "Telhados da Vila", "Uma rota apertada sobre a vila. Ratões blindados aparecem cedo.",
                new Color32(116, 137, 151, 255), new Color32(194, 147, 104, 255), new Color32(82, 58, 52, 255), new Color32(150, 112, 184, 255),
                new[]
                {
                    new Vector3(-10.2f, 0.0f), new Vector3(-7.55f, 0.0f), new Vector3(-7.55f, 3.0f),
                    new Vector3(-4.15f, 3.0f), new Vector3(-4.15f, -2.55f), new Vector3(-0.55f, -2.55f),
                    new Vector3(-0.55f, 2.55f), new Vector3(2.55f, 2.55f), new Vector3(2.55f, -1.55f),
                    new Vector3(5.55f, -1.55f), new Vector3(5.55f, 1.0f), new Vector3(8.72f, 1.0f)
                },
                new[]
                {
                    new Vector3(-8.8f, -2.25f), new Vector3(-6.0f, 1.5f), new Vector3(-5.8f, -0.1f), new Vector3(-2.4f, 0.15f),
                    new Vector3(-2.35f, -3.65f), new Vector3(1.0f, 0.25f), new Vector3(1.0f, 3.75f), new Vector3(4.0f, 0.3f),
                    new Vector3(6.9f, -0.25f), new Vector3(7.0f, 2.65f)
                }),

            new(4, "Fortaleza do Queijo", "A última defesa. Muitas curvas e as maiores hordas da campanha.",
                new Color32(87, 119, 92, 255), new Color32(188, 169, 128, 255), new Color32(70, 62, 51, 255), new Color32(225, 190, 68, 255),
                new[]
                {
                    new Vector3(-10.2f, -3.0f), new Vector3(-7.1f, -3.0f), new Vector3(-7.1f, 3.15f),
                    new Vector3(-3.55f, 3.15f), new Vector3(-3.55f, -2.05f), new Vector3(-0.05f, -2.05f),
                    new Vector3(-0.05f, 2.1f), new Vector3(3.25f, 2.1f), new Vector3(3.25f, -2.55f),
                    new Vector3(6.15f, -2.55f), new Vector3(6.15f, 0.0f), new Vector3(8.72f, 0.0f)
                },
                new[]
                {
                    new Vector3(-8.65f, -0.1f), new Vector3(-5.45f, 0.25f), new Vector3(-5.25f, -3.65f), new Vector3(-1.9f, 0.15f),
                    new Vector3(-1.85f, 3.6f), new Vector3(1.55f, -0.25f), new Vector3(1.55f, 3.55f), new Vector3(4.65f, 0.2f),
                    new Vector3(4.75f, -3.65f), new Vector3(7.45f, 1.75f)
                })
        };

        public static int UnlockedStage
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("GVR_UnlockedStage", 0), 0, Stages.Length - 1);
            set
            {
                PlayerPrefs.SetInt("GVR_UnlockedStage", Mathf.Clamp(value, 0, Stages.Length - 1));
                PlayerPrefs.Save();
            }
        }

        public static bool IsCleared(int stage, Difficulty difficulty)
        {
            return PlayerPrefs.GetInt($"GVR_Clear_{stage}_{(int)difficulty}", 0) == 1;
        }

        public static bool MarkCleared(int stage, Difficulty difficulty)
        {
            bool firstClear = !IsCleared(stage, difficulty);
            PlayerPrefs.SetInt($"GVR_Clear_{stage}_{(int)difficulty}", 1);
            if (stage < Stages.Length - 1 && UnlockedStage < stage + 1) UnlockedStage = stage + 1;
            PlayerPrefs.Save();
            return firstClear;
        }

        public static int ClearedCount()
        {
            int total = 0;
            for (int i = 0; i < Stages.Length; i++)
            {
                if (IsCleared(i, Difficulty.Normal)) total++;
                if (IsCleared(i, Difficulty.Dificil)) total++;
                if (IsCleared(i, Difficulty.Insano)) total++;
            }
            return total;
        }
    }
}
