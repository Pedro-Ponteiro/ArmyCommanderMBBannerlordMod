# Arquitetura do Army Commander

Este arquivo documenta o que foi aprendido ao estudar o projeto. O foco e ajudar manutencao futura: onde cada comportamento nasce, quais classes conversam entre si e quais pontos sao mais frageis por dependerem de internals do Bannerlord.

## Visao geral

O Army Commander e construido ao redor de tres eixos:

1. Alterar regras vanilla de exercitos com Harmony.
2. Substituir e ampliar partes do overlay e da tela `ArmyManagementVM` com UIExtenderEx.
3. Usar contextos estaticos para conectar o overlay customizado, a tela de gestao e os patches que executam acoes no campaign model.

Na pratica, o mod transforma a gestao de exercito de uma experiencia centrada na party do jogador para uma experiencia centrada no exercito selecionado no overlay.

## Inicializacao

`MySubModule` e o ponto de entrada.

- `OnSubModuleLoad` cria o diretorio de log, aplica `Harmony.PatchAll(Assembly.GetExecutingAssembly())`, cria `UIExtender.Create("ArmyCommander")`, registra o assembly e habilita o UIExtender.
- `OnSubModuleUnloaded` desabilita e deregistra UIExtenderEx, remove patches Harmony pelo id `ArmyCommander` e limpa referencias.
- `OnGameStart` hoje apenas valida que o jogo e `Campaign`. O comportamento antigo/comentado de `AddBehavior` nao esta ativo.

O arquivo tambem centraliza logging defensivo. Erro de log nao derruba o jogo.

## Build e deploy

`ArmyCommander.csproj` define:

- `TargetFrameworkVersion`: `v4.7.2`.
- `OutputType`: `Library`.
- `AssemblyName`: `ArmyCommander`.
- `BannerlordDir`: caminho absoluto local para a instalacao Steam de Bannerlord.
- `OutputPath`: pasta `Modules\ArmyCommander\bin\Win64_Shipping_Client`.

O target `DeployModFiles`, executado apos o build, usa `robocopy` para espelhar `GUI\` no modulo instalado e copia `SubModule.xml`.

## Manifesto

`SubModule.xml` declara o modulo como singleplayer community module, versao `v2.2.0`, com dependencias obrigatorias:

- `Bannerlord.Harmony`
- `Bannerlord.ButterLib`
- `Bannerlord.UIExtenderEx`
- `Native`, `SandBoxCore`, `CustomBattle`, `Sandbox`

`StoryMode` e `NavalDLC` aparecem como opcionais. A classe carregada e `ArmyCommander.MySubModule`.

## Estado compartilhado

O projeto usa stores/contextos estaticos como cola entre patches e ViewModels.

- `ACArmyOverlayUIContext`: instancia ativa do overlay de exercito. Guarda `SelectedArmy`, contadores agregados e controle do botao de paginacao.
- `ACArmyManagementUIContext`: instancia ativa da tela de gestao. Guarda `currentMainParty`, se ela ja tem exercito, alvo, comportamento do exercito, influencia enviada e flag `movieIsLoaded`.
- `ACArmyLineUIContext`: contexto por linha do overlay. Carrega leader party, contadores, comida, influencia, coesao e custos.
- `ArmyCommandsBehaviorStore.army_commands`: dicionario estatico `Army -> (ArmyType, Settlement)` usado para reaplicar/forcar comandos de AI-led armies.
- `ACPolicyStore.MercenaryArmyLeadersPolicy`: referencia estatica para a politica criada no patch de `DefaultPolicies`.

Esses estados nao parecem persistidos em save/load; eles sao reconstituidos em runtime pela UI e pelos eventos.

## Overlay de exercitos

O overlay customizado e montado por UIExtenderEx.

- `UIExtension/UIPatches/ACArmyOverlayArmyListPatch.cs` substitui o `Window` do prefab `ArmyOverlay`.
- O patch carrega o XML original de `SandBox/GUI/Prefabs/Map/ArmyOverlay.xml`, extrai `ArmyOverlayWidget` e o injeta no placeholder `ArmyCommanderOriginalArmyOverlayWidgetPlaceholder`.
- `GUI/ArmyOverlayWindow.xml` adiciona o widget customizado `ACOverlayWidget` e conserva o overlay original logo depois.
- `UIExtension/UIPatches/ACChatLogPatch.cs` aumenta a margem inferior do chat log para abrir espaco visual.
- `HarmonyPatches/ArmyOverlayWidgetPatch.cs` ajusta posicao/paginacao quando o overlay tem id `ACOverlayWidget`.

`ArmyMenuOverlayVMMixin` e o mixin principal dessa UI:

- Cria `ACArmyOverlayUIContext`.
- Mantem `ArmyOverlayArmiesList`.
- Reconstroi linhas com `RenewLeftArmyOverlay`.
- Atualiza totais do topo em `UpdateTopWidgets`.
- Escuta `CampaignEvents.HourlyTickEvent` para atualizar o overlay.
- Reage a exercitos criados/desfeitos por callbacks chamados de patches em `ArmyPatch.cs`.

Cada linha e composta por:

- `SelectableArmyLineVM`: item clicavel, selecao/hover e execucao de `OnArmyOverlaySetDirty`.
- `SelectableArmyLeaderVisualVM`: retrato, banner, tooltip, link de enciclopedia e camera no mapa.
- `SelectableArmyPropertiesRow`: agrupa metricas em linhas.
- `SelectableArmyItemPropertyVM`: metrica individual com sprite, valor, delta, warning e tooltip.
- `ACArmyLineWidgetBuilders`: builder dos widgets de parties, tropas, comida, influencia, coesao e custo de coesao.

## Selecao de exercito

O exercito ativo da UI fica em `ACArmyOverlayUIContext.SelectedArmy`.

Quando o usuario clica numa linha:

1. `SelectableArmyLineVM.ExecuteClickFunction` grava `SelectedArmy = LeaderParty.Army`.
2. O overlay vanilla e marcado dirty por `CampaignEventDispatcher.Instance.OnArmyOverlaySetDirty()`.
3. O patch `ArmyMenuOverlayVM_get_ArmyToUse_Patch` faz o getter `ArmyToUse` retornar o exercito selecionado, ou faz fallback para o exercito da main party, ou para o primeiro exercito do reino.
4. `ArmyMenuOverlayVM_GetIsPlayerArmyLeader_Patch` sempre retorna `true`, liberando caminhos de UI que normalmente dependem do jogador ser lider.

## Tela de gestao de exercito

`HarmonyPatches/ArmyManagementVMPatch.cs` e a peca mais central do projeto.

Ele cria reverse patches para chamar metodos originais de `ArmyManagementVM` quando necessario:

- `OnRefresh`
- `OnAddToCart`
- `OnRemove`
- `OnFocus`

No constructor postfix, o patch reconstrui a VM:

- Zera/recria `PartyList`, `PartiesInCart`, `_partiesToRemove` e outros campos internos.
- Decide `currentMainParty`:
  - se o jogador nao e lider do reino, usa a party do jogador;
  - se nao ha exercito selecionado, usa a party do jogador;
  - caso contrario, usa a leader party do exercito selecionado.
- Cria o item principal (`_mainPartyItem`) e adiciona ao carrinho.
- Popula a lista esquerda com parties do mesmo mapa/faccao.
- Se a party principal ja lidera um exercito, move membros existentes para o carrinho com custo zero.
- Reordena listas e chama refresh original.

Fluxos importantes:

- `OnFirstPartyAdded`: quando o primeiro item e adicionado, ele vira a party principal/contexto do exercito a criar ou editar.
- `OnArmyLeaderRemoved`: retirar o lider limpa a selecao e volta a tela para estado de criacao.
- `CustomOnAddToCart` e `CustomOnRemove`: substituem os fluxos vanilla para suportar editar exercitos alheios.
- `ExecuteDonePrefix`: aplica coesao, cria exercito novo, adiciona membros, grava comandos, chama `Gather` ou `SetPartyAiAction`, desconta influencia, remove parties e fecha a tela.
- `CustomDisbandArmy`: desfaz exercito liderado pelo jogador setando `Army = null`; para outro lider, usa `DisbandArmyAction.ApplyByReleasedByPlayerAfterBattle`.
- `ExecuteResetPrefix` e `ExecuteCancelPrefix`: restauram influencia inicial e limpam alteracoes temporarias.

`ArmyManagementVMMixIn` injeta controles extras em `GUI/ACArmyManagementWidgets.xml`:

- texto de comportamento/comando do exercito;
- botao de enviar 50 influencia;
- seletor de assentamento alvo;
- texto do comportamento (`Defender` ou `Besieger`).

O mixin atualiza esses widgets conforme `currentMainParty`, `mainPartyHasArmy` e disponibilidade do exercito.

## Regras de elegibilidade e criacao

`DefaultArmyManagementCalculationModelPatch.cs` altera a elegibilidade das parties e regras de criacao de exercito.

- `CheckPartyEligibility` e substituido por prefix completo.
- Mercenarios podem criar exercitos apenas se a politica `Mercenary Army Leaders` estiver ativa.
- Parties ocupadas, em cerco, prisioneiras, em eventos, disbanding, em raft/sea ou sem homens suficientes sao bloqueadas.
- O lider do reino recebe regras especiais para selecionar lideres de exercitos ja existentes.

`CanLordCreateArmy` tambem e substituido. A versao do mod:

- Permite mercenarios com politica ativa.
- Filtra parties disponiveis.
- Limita o quanto do reino pode estar comprometido com exercitos por uma heuristica de 70%.
- Exige forca total minima de 1000 para criar exercito AI.

`CanPlayerCreateArmy` recebe transpiler para substituir a checagem simples de mercenario por `IsUnderMercenaryServiceAndPolicyNotEnacted`.

## Politica de mercenarios

`DefaultPoliciesPatch.cs` cria a politica `army_commander_mercenary_army_leaders` durante `DefaultPolicies.InitializeAll`.

Impacto declarado:

- mercenarios podem formar e liderar exercitos em servico do reino;
- o cla governante paga 100 de influencia quando um exercito mercenario e formado.

O custo de 100 e aplicado no postfix de `Army.Gather` quando a policy esta ativa para o cla lider do exercito.

`AiPartyThinkBehaviorPatch.cs` transpila `PartyHourlyAiTick` para tratar mercenarios com a politica ativa como se nao estivessem sob servico mercenario para a logica de gathering.

## Comandos de AI-led armies

O fluxo de comando usa `ArmyCommandsBehaviorStore`.

Quando `ExecuteDonePrefix` atualiza ou cria um exercito que nao e liderado pela main party, ele grava:

`army_commands[army] = (armyBehavior, targetSettlement)`

Depois, `SetPartyAiActionPatch.cs` intercepta `SetPartyAiAction.ApplyInternal`. Se o jogador e lider do reino e o owner pertence ao reino do jogador:

- comandos vanilla como ir/patrulhar/raid/siege/defender/escort podem ser substituidos;
- para `Besieger`, o patch reaplica acao de sitiar o assentamento salvo;
- para `Defender`, reaplica acao de defender o assentamento salvo;
- se a situacao politica mudou, o comando salvo e removido e o vanilla segue.

## Outros patches relevantes

- `CampaignUIHelperPatch.cs`: substitui `GetCanManageCurrentArmyWithReason`, liberando gestao quando o jogador nao esta ocupado e respeitando a politica de mercenarios.
- `MapBarVMPatch.cs`: esconde o botao vanilla de gather army quando o overlay customizado deve aparecer.
- `MapScreenPatch.cs`: substitui `IMapStateHandler.OnRefreshState` para criar/remover overlay de exercito conforme `ACHelpers.ShouldShowArmyOverlayForPlayer()`.
- `ArmyPatch.cs`: notifica o mixin quando exercitos sao dispersos ou reunidos.
- `ArmyManagementItemVMPatch.cs`: altera nome e forca de itens que representam lideres de exercito na lista de gestao.
- `OpenArmyManagement_All_Patch.cs`: apos abrir a gestao por map bar, map overlay ou kingdom screen, marca `movieIsLoaded` e atualiza widgets customizados.

## Helpers e calculos

`ACHelpers` concentra regras de disponibilidade e metricas:

- se jogador/party/exercito esta ocupado;
- se settlement esta em condicao aceitavel;
- contagens de parties/tropas;
- distancia em dias;
- comida, influencia, coesao e custos;
- agrupamento de tropas por `FormationClass`.

`ACHintHelpers` constroi tooltips para:

- totais do reino no topo do overlay;
- parties/tropas/comida/coesao/influencia de cada exercito.

`ACCalculationModel.DistributeToSmallestKeepOriginalOrder` distribui um incremento inteiro elevando primeiro os menores valores e retornando a ordem original.

`ACActions` contem helpers para enviar itens e transferir/subtrair/adicionar influencia.

## Pontos frageis observados

- Muitos patches acessam campos/metodos privados por `AccessTools`. Mudancas de versao do Bannerlord podem quebrar nomes como `_partiesToRemove`, `_mainPartyItem`, `_armyOverlay`, `ApplyInternal` e `ArmyToUse`.
- Os reverse patches lancam `NotImplementedException` por design se chamados sem Harmony substituir o corpo. Isso e esperado, mas dificulta testes unitarios comuns.
- `ArmyCommandsBehaviorStore` e estatico e nao persistido. Comandos salvos podem se perder entre sessoes.
- Ha valores magicos importantes: envio de 50 influencia, custo de 100 para exercito mercenario, limite de 70% de parties do reino em exercitos e forca minima 1000.
- `BannerlordDir` esta hardcoded para uma instalacao local. Outro ambiente precisa ajustar a propriedade no `.csproj`.
- `ACPolicyStore.MercenaryArmyLeadersPolicy` depende do patch de `DefaultPolicies.InitializeAll`; codigo que consultar antes disso precisa tolerar null.
- `ACActions.SendItemQuantityOneToOne` calcula `amount_to_give`, mas chama `SendItem(..., quantity)` dentro do loop. Isso parece suspeito se a intencao era enviar somente a quantidade daquele item.

## Onde mexer para tarefas comuns

- Mudar visual do overlay: `GUI/ArmyOverlayWindow.xml` e `GUI/Brushes/ArmyCommanderBrushes.xml`.
- Mudar metricas das linhas: `ACArmyLineWidgetBuilders`, `ACArmyLineUIContext`, `ACHelpers` e `ACHintHelpers`.
- Mudar comportamento da tela de gestao: `HarmonyPatches/ArmyManagementVMPatch.cs`.
- Mudar regras de permissao/elegibilidade: `DefaultArmyManagementCalculationModelPatch.cs` e `CampaignUIHelperPatch.cs`.
- Mudar comandos de exercitos AI-led: `SetPartyAiActionPatch.cs` e o trecho `ExecuteDonePrefix` em `ArmyManagementVMPatch.cs`.
- Mudar politica customizada: `DefaultPoliciesPatch.cs`, `AiPartyThinkBehaviorPatch.cs`, `DefaultArmyManagementCalculationModelPatch.cs` e `ArmyPatch.cs`.
