# Army Commander

Army Commander e um mod singleplayer para Mount & Blade II: Bannerlord. Ele estende a gestao de exercitos do reino, troca partes do overlay de exercito, permite selecionar exercitos do reino na interface e altera regras de criacao/comando de exercitos via Harmony e UIExtenderEx.

## O que o mod faz

- Mostra um overlay customizado com todos os exercitos do reino do jogador.
- Exibe indicadores por exercito: parties, tropas, comida, influencia, coesao e custo de coesao perdida.
- Permite selecionar um exercito no overlay e abrir a tela de gestao usando esse exercito como alvo.
- Permite que o lider do reino gerencie ou comande exercitos liderados por outros lordes.
- Adiciona a politica `Mercenary Army Leaders`, permitindo que mercenarios formem e liderem exercitos quando a politica esta ativa.
- Redireciona comandos de exercitos AI-led para defender ou sitiar assentamentos escolhidos pelo jogador.

## Tecnologia e dependencias

- Projeto C# class library em .NET Framework 4.7.2.
- Mod de Bannerlord registrado por `SubModule.xml`.
- Patching em runtime com Harmony.
- Injecao de ViewModels e prefabs com Bannerlord.UIExtenderEx.
- Dependencias de mod declaradas no manifesto: `Bannerlord.Harmony`, `Bannerlord.ButterLib` e `Bannerlord.UIExtenderEx`.

O caminho do jogo esta configurado em `ArmyCommander.csproj` pela propriedade `BannerlordDir`. O build copia a DLL para:

`$(BannerlordDir)\Modules\ArmyCommander\bin\Win64_Shipping_Client\`

Depois do build, o alvo `DeployModFiles` tambem sincroniza `GUI\` e copia `SubModule.xml` para a pasta do modulo no Bannerlord.

## Mapa do projeto

- `MySubModule.cs`: ponto de entrada do mod. Inicializa logs, aplica Harmony, registra UIExtenderEx e limpa recursos ao descarregar.
- `SubModule.xml`: manifesto do modulo, dependencias, versao do mod e classe de submodulo.
- `ArmyCommander.csproj`: referencias do Bannerlord, Harmony, UIExtenderEx e regra de deploy.
- `HarmonyPatches/`: patches que alteram regras de exercito, overlay, tela de gestao, AI e politicas.
- `UIExtension/`: mixins, contextos, ViewModels e patches de prefab para as telas.
- `GUI/`: XMLs injetados ou substituidos via UIExtenderEx.
- `Helpers/`: calculos auxiliares, checagens de disponibilidade e tooltips.
- `Store/`: estado estatico usado pelos patches.
- `Actions/`: acoes auxiliares como transferencia de influencia e itens.
- `CalculationModels/`: utilitarios de calculo independentes.

## Fluxo principal

1. `MySubModule.OnSubModuleLoad` aplica todos os patches Harmony e habilita UIExtenderEx.
2. O overlay de exercito vanilla e substituido por `GUI/ArmyOverlayWindow.xml`, que mantem o overlay original como placeholder e adiciona a lista customizada.
3. `ArmyMenuOverlayVMMixin` cria linhas para os exercitos do reino, calcula os indicadores e registra eventos para atualizar a UI.
4. Ao clicar em uma linha, `ACArmyOverlayUIContext.SelectedArmy` passa a apontar para o exercito selecionado.
5. Ao abrir a gestao de exercito, `ArmyManagementVMPatches` reconstrui a tela para usar o lider do exercito selecionado como `currentMainParty`.
6. Ao confirmar, `ExecuteDonePrefix` cria, atualiza, comanda ou desfaz o exercito conforme o estado do carrinho da tela.

## Debug

O mod grava log em:

`%LOCALAPPDATA%\ArmyCommander\ArmyCommander_Debug.log`

Esse log cobre carregamento/descarregamento do submodulo, aplicacao de patches, registro do UIExtenderEx e excecoes capturadas no ciclo de vida principal.

## Documentacao tecnica

Veja [docs/ARQUITETURA.md](docs/ARQUITETURA.md) para um mapa mais detalhado dos patches, fluxos de dados e pontos de manutencao.
