#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GatosVsRatos.Editor
{
    public static class ProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("Gatos vs Ratos/Configurar projeto")]
        public static void ConfigureProject()
        {
            PlayerSettings.companyName = "Projeto TD";
            PlayerSettings.productName = "Gatos vs Ratos";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.runInBackground = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Gatos vs Ratos: projeto configurado com sucesso.");
        }

        [MenuItem("Gatos vs Ratos/Gerar executável Windows")]
        public static void BuildWindows()
        {
            ConfigureProject();
            Directory.CreateDirectory("Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/GatosVsRatos.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception($"Build falhou: {report.summary.result}");
            Debug.Log($"Build concluído: {report.summary.outputPath} ({report.summary.totalSize} bytes)");
        }

        public static void ValidateProject()
        {
            ConfigureProject();
            string[] required =
            {
                ScenePath,
                "Assets/Scripts/GameApp.cs",
                "Assets/Scripts/StageData.cs",
                "Assets/Scripts/Tower.cs",
                "Assets/Scripts/Enemy.cs",
                "Assets/Scripts/Projectile.cs",
                "Assets/Resources/MenuBackground.png"
            };
            foreach (string path in required)
                if (!File.Exists(path)) throw new FileNotFoundException("Arquivo obrigatório ausente", path);
            if (GatosVsRatos.Campaign.Stages.Length != 5)
                throw new System.Exception("A campanha precisa conter exatamente cinco fases.");
            foreach (var stage in GatosVsRatos.Campaign.Stages)
            {
                if (stage.Path.Length < 8) throw new System.Exception($"A fase {stage.Index + 1} tem poucos pontos de caminho.");
                if (stage.TowerSpots.Length < 10) throw new System.Exception($"A fase {stage.Index + 1} precisa de pelo menos dez pontos de torre.");
            }
            Debug.Log("VALIDAÇÃO CONCLUÍDA: scripts compilados e arquivos obrigatórios presentes.");
        }
    }
}
#endif
