# Army Commander

Army Commander e um mod singleplayer para Mount & Blade II: Bannerlord. Ele amplia a gestao de exercitos do reino, adiciona novo overlay de exercito, permite escolher qual exercito sera gerenciado e adiciona ordens persistentes para exercitos liderados por AI.

## O que o mod faz

- Mostra um overlay customizado com todos os exercitos do reino do jogador.
- Exibe indicadores por exercito: parties, tropas, comida, influencia, coesao e custo de recuperar coesao.
- Permite selecionar um exercito no overlay e abrir a tela de gestao usando esse exercito como alvo.
- Permite que o lider do reino, ou um vassalo autorizado, comande exercitos liderados por outros lordes.
- Permite que mercenarios formem/liderem exercitos quando a politica `Mercenary Army Leaders` esta ativa ou quando o governante concedeu permissao por dialogo.
- Adiciona dialogos para pedir permissao de lideranca mercenaria e comando de exercitos como vassalo.
- Redireciona exercitos AI-led para defender ou sitiar assentamentos escolhidos pelo jogador.
- Persiste ordens de exercito em saves, incluindo alvo, ponto de reuniao e flags de autonomia da AI.
- Controla desvios de AI para combate, ajuda a aliados, ressuprimento e fuga conforme as ordens salvas.
- Protege exercitos com ordens contra dispersao automatica por inatividade/objetivo concluido e tenta recuperar coesao quando possivel.

## Tecnologia e dependencias

- Projeto C# class library em .NET Framework 4.7.2.
- Mod de Bannerlord registrado por `SubModule.xml`.
- Patching em runtime com Harmony.
- Injecao de ViewModels e prefabs com Bannerlord.UIExtenderEx.
- Dependencias de mod declaradas no manifesto: `Bannerlord.Harmony`, `Bannerlord.ButterLib` e `Bannerlord.UIExtenderEx`.

O caminho do jogo esta configurado em `ArmyCommander.csproj` pela propriedade `BannerlordDir`. O build copia a DLL para:

`$(BannerlordDir)\Modules\ArmyCommander\bin\Win64_Shipping_Client\`

Depois do build, o alvo `DeployModFiles` espelha `GUI\` com `robocopy /MIR` e copia `SubModule.xml` para a pasta do modulo no Bannerlord.

## Mapa do projeto

- `MySubModule.cs`: ponto de entrada do mod. Aplica Harmony, registra UIExtenderEx, reseta stores/contextos e registra behaviors de campanha.
- `SubModule.xml`: manifesto do modulo, dependencias, versao do mod e classe de submodulo.
- `ArmyCommander.csproj`: referencias do Bannerlord, Harmony, UIExtenderEx, lista de fontes e regra de deploy.
- `ACBehaviors/`: behaviors de campanha para persistencia de ordens e dialogos de permissao.
- `ACBehaviors/Context/`: caches transientes usados por comandos de AI, como ultimo assentamento visitado e estado de ressuprimento.
- `HarmonyPatches/`: patches que alteram regras de exercito, overlay, tela de gestao, AI, disband, chat log e politicas.
- `UIExtension/`: mixins, contextos, ViewModels e patches de prefab para overlay e gestao de exercito.
- `GUI/`: XMLs injetados ou substituidos via UIExtenderEx.
- `Helpers/`: calculos auxiliares, checagens de disponibilidade, permissoes, comandos de AI e tooltips.
- `Store/`: estado estatico usado pelos patches, incluindo ordens persistiveis e permissoes concedidas.
- `Actions/`: acoes auxiliares como transferencia de influencia e itens.
- `CalculationModels/`: utilitarios de calculo independentes.
- `WatchAndMirror-GUI.ps1`: script auxiliar para espelhar alteracoes da pasta `GUI`.

## Fluxo principal

1. `MySubModule.OnSubModuleLoad` aplica todos os patches Harmony e habilita UIExtenderEx.
2. `MySubModule.OnGameStart` valida `Campaign`, reseta `ArmyCommandsContext`, `ArmyCommandsBehaviorStore` e `ACPermissionsStore`, e registra os behaviors de persistencia/dialogo.
3. O overlay de exercito vanilla e substituido por `GUI/ArmyOverlayWindow.xml`, que mantem o overlay original como placeholder e adiciona a lista customizada.
4. `ArmyMenuOverlayVMMixin` cria linhas para os exercitos do reino, calcula os indicadores, atualiza totais e mantem a selecao.
5. Ao clicar em uma linha, `ACArmyOverlayUIContext.SelectedArmy` passa a apontar para o exercito selecionado.
6. Ao abrir a gestao de exercito, `ArmyManagementVMPatches` reconstrui a tela para usar a party lider do exercito selecionado como `currentMainParty`.
7. `ArmyManagementVMMixIn` injeta controles de ordem: alvo, ponto de reuniao, comportamento, combate, ajuda a aliados, ressuprimento, fuga, e remocao de ordens.
8. Ao confirmar, `ExecuteDonePrefix` cria/edita/desfaz exercitos, aplica custos, salva ordens em `ArmyCommandsBehaviorStore` e chama a recalculacao de AI quando necessario.
9. `SetPartyAiActionPatch`, `DefaultMobilePartyAIModelPatch`, `AiPartyThinkBehaviorPatch` e `ACAIBehaviorHelpers` fazem a AI obedecer as ordens salvas e lidar com ressuprimento, fuga, combate e cerco.

## Ordens de exercito

As ordens persistidas ficam em `ArmyCommandsBehaviorStore.army_commands`. Cada entrada guarda:

- tipo de comportamento (`Defender` ou `Besieger`);
- assentamento alvo;
- assentamento de reuniao enquanto o exercito espera membros;
- se pode engajar inimigos;
- se pode ajudar aliados quando combate geral esta bloqueado;
- se pode ressuprir;
- se pode fugir de perigo.

`ACArmyCommanderBehavior` salva essas ordens em XML dentro do save usando ids estaveis de heroi e assentamento. Ao carregar, ele resolve o lider do exercito, o alvo e o ponto de reuniao, descartando entradas que nao existem mais.

## Permissoes

- Mercenario: pode pedir ao governante permissao para formar/liderar exercitos. Requer clan tier 3 e relacao 25.
- Vassalo: pode pedir ao governante permissao para comandar exercitos do reino. Requer clan tier 4 e relacao 40.
- Governante do reino: sempre passa em `HasPlayerPermissionForArmyCommand`.
- Politica `Mercenary Army Leaders`: tambem libera lideranca mercenaria quando ativa.

As permissoes concedidas sao salvas em `ACPermissionsStore` como ids de reino e sao limpas quando o contrato mercenario termina ou quando o clan do jogador sai do reino que concedeu a permissao de vassalo.

## Debug e testes

O mod grava log principal em:

`%LOCALAPPDATA%\ArmyCommander\ArmyCommander_Debug.log`

`ACArmyCommanderBehavior` tambem tem log proprio em:

`%LOCALAPPDATA%\ArmyCommander\ArmyCommander_Behavior.log`

Procedimentos manuais de regressao ficam em:

`docs/ArmyCommander_InGame_Test_Procedures.md`

## Documentacao tecnica

Veja [docs/ARQUITETURA.md](docs/ARQUITETURA.md) para um mapa mais detalhado dos patches, fluxos de dados e pontos de manutencao.
