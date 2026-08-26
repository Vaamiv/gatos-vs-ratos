# Gatos vs Ratos — Tower Defense

Tower Defense 2D em estilo cartoon feito na Unity 6. O jogador protege a despensa dos gatos contra ondas de ratos usando três tipos de torres evoluíveis.

## O jogo

- **Gato Metralha:** menor dano por tiro e a maior velocidade de ataque.
- **Gato Bazuca:** ataque mais lento, maior dano e maior alcance.
- **Gato Catapulta:** lança pedras que causam dano em área.
- Cada gato pode evoluir do nível 1 ao nível 3.
- Três inimigos: rato comum, rato corredor e ratão resistente.
- Campanha com **5 fases**, caminhos e identidades visuais diferentes.
- Cada fase possui modo **Normal com 10 ondas**, **Difícil com 15 ondas** e **Insano com 20 ondas**.
- As hordas começam menores e crescem progressivamente em quantidade, resistência, velocidade e presença de ratões.
- No Difícil e no Insano, torres e evoluções custam mais e os ratos rendem menos peixes; no Insano, eles também causam dano extra à base.
- Fases desbloqueáveis e 15 medalhas de progresso salvas localmente.
- Cronômetro, vida da base, economia e condição de vitória/derrota.
- Ranking local salvo entre partidas (recorde de ratos e número de vitórias).
- Música original para menu/mapa e música de batalha, além dos efeitos sonoros.
- Interface compatível com mouse ou toque; atalhos `1`, `2`, `3` e `E` no PC.

## Como abrir

1. No Unity Hub, escolha **Add > Add project from disk**.
2. Selecione esta pasta.
3. Abra com **Unity 6000.5.9f1** ou outra versão Unity 6 compatível.
4. Abra `Assets/Scenes/Game.unity` e pressione **Play**.

O jogo é montado em tempo de execução pelo `GameApp`; a cena propositalmente é mínima.

## Como gerar o executável

No editor, use o menu **Gatos vs Ratos > Gerar executável Windows**. O resultado é salvo em `Builds/Windows/GatosVsRatos.exe`.

Também é possível executar em modo batch:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe" `
  -batchmode -quit -projectPath . `
  -executeMethod GatosVsRatos.Editor.ProjectBuilder.BuildWindows
```

## Estrutura principal

- `Assets/Scripts/GameApp.cs` — estados, menu, HUD, ondas, economia e resultados.
- `Assets/Scripts/StageData.cs` — dados, rotas, cores e progresso das cinco fases.
- `Assets/Scripts/Tower.cs` — detecção, mira, disparo e evolução das torres.
- `Assets/Scripts/Enemy.cs` — caminho, vida, dano e tipos de rato.
- `Assets/Scripts/Projectile.cs` — translação, colisão, dano direto e em área.
- `Assets/Scripts/ArtFactory.cs` — arte vetorial/cartoon criada em tempo de execução.
- `Assets/Scripts/AudioKit.cs` — efeitos e duas músicas originais sintetizadas por código.
- `Assets/Resources/MenuBackground.png` — ilustração original do menu.

## Entrega da disciplina

- Executável: **ADICIONAR LINK**
- Vídeo demonstrativo: **ADICIONAR LINK**

`diegopatr` 

## Arte

A ilustração do menu foi criada especialmente para este projeto com geração de imagem da OpenAI. Os demais personagens, cenários, animações, efeitos e músicas são construídos por código, portanto o projeto não depende de assets de terceiros.
