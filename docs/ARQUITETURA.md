# Arquitetura do Army Commander

Este arquivo documenta o estado atual do projeto depois das mudancas feitas apos `fe9f54f6d9011db1971fdf7d207ff2d6705e10d0`. O foco e ajudar manutencao futura: onde cada comportamento nasce, quais classes conversam entre si e quais pontos sao mais frageis por dependerem de internals do Bannerlord.

## Visao geral

O Army Commander e construido ao redor de quatro eixos:

1. Alterar regras vanilla de exercitos com Harmony.
2. Substituir e ampliar partes do overlay e da tela `ArmyManagementVM` com UIExtenderEx.
3. Usar contextos/stores estaticos para conectar overlay, tela de gestao, behaviors de campanha e patches de AI.
4. Persistir ordens de exercitos AI-led e permissoes politicas/dialogadas em saves.

Na pratica, o mod transforma a gestao de exercito de uma experiencia centrada na party do jogador para uma experiencia centrada no exercito selecionado no overlay, com capacidade de comandar exercitos de terceiros quando o jogador tem permissao.

## Inicializacao

`MySubModule` e o ponto de entrada.

- `OnSubModuleLoad` aplica `Harmony.PatchAll(Assembly.GetExecutingAssembly())`, cria `UIExtender.Create("ArmyCommander")`, registra o assembly e habilita o UIExtender.
- `OnSubModuleUnloaded` desabilita e deregistra UIExtenderEx, remove patches Harmony pelo id `ArmyCommander` e limpa referencias.
- `OnGameStart` valida que o jogo e `Campaign`, reseta `ArmyCommandsContext`, `ArmyCommandsBehaviorStore` e `ACPermissionsStore`, e registra:
  - `ACArmyCommanderBehavior`;
  - `ACMercenaryArmyLeadershipDialogueBehavior`;
  - `ACVassalArmyCommanderDialogueBehavior`.

O arquivo tambem centraliza logging defensivo. Erro de log nao derruba o jogo.

## Build e deploy

`ArmyCommander.csproj` define:

- `TargetFrameworkVersion`: `v4.7.2`.
- `OutputType`: `Library`.
- `AssemblyName`: `ArmyCommander`.
- `BannerlordDir`: caminho absoluto local para a instalacao Steam de Bannerlord.
- `OutputPath`: pasta `Modules\ArmyCommander\bin\Win64_Shipping_Client`.

O target `DeployModFiles`, executado apos o build, usa `robocopy` para espelhar `GUI\` no modulo instalado e copia `SubModule.xml`.

`WatchAndMirror-GUI.ps1` e um utilitario separado para espelhar alteracoes da pasta `GUI` durante iteracao.

## Manifesto

`SubModule.xml` declara o modulo como singleplayer community module, versao `v2.2.0`, com dependencias obrigatorias:

- `Bannerlord.Harmony`
- `Bannerlord.ButterLib`
- `Bannerlord.UIExtenderEx`
- `Native`, `SandBoxCore`, `CustomBattle`, `Sandbox`

`StoryMode` e `NavalDLC` aparecem como opcionais. A classe carregada e `ArmyCommander.MySubModule`.

## Estado compartilhado

O projeto usa stores/contextos estaticos como cola entre patches e ViewModels.

- `ACArmyOverlayUIContext`: instancia ativa do overlay de exercito. Guarda `SelectedArmy`, contadores agregados, controle do botao de pagina e estado expandido do overlay.
- `ACArmyManagementUIContext`: instancia ativa da tela de gestao. Guarda `currentMainParty`, `mainPartyHasArmy`, alvo, ponto de reuniao, comportamento do exercito, flags de comando, influencia enviada e `movieIsLoaded`.
- `ACArmyLineUIContext`: contexto por linha do overlay. Carrega leader party, contadores, comida, influencia, coesao, custos e listas auxiliares.
- `ArmyCommandsBehaviorStore.army_commands`: dicionario `Army -> command tuple` usado para salvar e reaplicar ordens do jogador.
- `ArmyCommandsContext`: caches transientes para AI, incluindo `ArmyLastVisitedSettlementCache` e `ArmyIsResupplyingDic`.
- `ACPermissionsStore`: guarda ids de reino que concederam permissao de lideranca mercenaria ou comando de vassalo.
- `ACPolicyStore.MercenaryArmyLeadersPolicy`: referencia estatica para a politica criada no patch de `DefaultPolicies`.

Os contextos de UI sao reconstituidos em runtime pela UI e pelos eventos. As ordens e permissoes relevantes para save/load sao persistidas pelos behaviors de campanha.

## Behaviors de campanha

### ACArmyCommanderBehavior

`ACArmyCommanderBehavior` registra persistencia e manutencao das ordens de exercito.

Eventos:

- `OnSettlementOwnerChangedEvent`: revisa ordens quando posse de assentamentos muda.
- `OnPeaceOfferResolvedEvent`: revisa ordens quando paz muda a validade de alvo inimigo.
- `PartyAttachedAnotherParty`: reativa AI quando membro entra em exercito comandado.
- `HourlyTickEvent`: workaround para reativar AI de exercitos comandados que ficaram em comportamento `0`.

Persistencia:

- Usa a chave `ArmyCommander.ArmyCommands.v1`.
- Serializa XML com `leaderHeroId`, `armyType`, `targetSettlementId`, `gatherSettlementId` e flags booleanas.
- No load, procura o exercito pelo lider dentro de `Clan.PlayerClan.Kingdom.Armies`.
- Descarta comandos sem lider, alvo ou tipo suportado.
- Tipos suportados: `Besieger` e `Defender`.

Validacao:

- `RefreshArmyCommandsStore` chama `ACAIBehaviorHelpers.ValidatePlayerCommandAndAskIfNeeded`.
- Se um alvo de cerco deixa de ser inimigo, ou um alvo de defesa deixa de pertencer ao reino do jogador, o mod pergunta se o exercito deve esperar em um assentamento seguro ou voltar a AI vanilla.

### ACMercenaryArmyLeadershipDialogueBehavior

Adiciona dialogo para o jogador mercenario pedir permissao ao governante do reino contratado.

- Requer servico mercenario ativo.
- O interlocutor deve ser o lider do reino do jogador.
- Nao aparece se a permissao ja existe.
- Clique exige relacao minima 25 e clan tier minimo 3.
- Salva a permissao em `ACPermissionsStore._acKingdomIdThatAllowedPlayerMercenaryArmyLeadership`.
- Limpa a permissao quando o servico mercenario termina.

### ACVassalArmyCommanderDialogueBehavior

Adiciona dialogo para o jogador vassalo pedir permissao ao governante para comandar exercitos do reino.

- O interlocutor deve ser o lider do reino do jogador.
- Nao aparece para mercenarios.
- Nao aparece se `ACHelpers.HasPlayerPermissionForArmyCommand()` ja for verdadeiro.
- Clique exige relacao minima 40 e clan tier minimo 4.
- Salva a permissao em `ACPermissionsStore._acKingdomIdThatAllowedPlayerVassalArmyCommand`.
- Limpa a permissao quando o clan do jogador sai do reino que a concedeu.

## Overlay de exercitos

O overlay customizado e montado por UIExtenderEx.

- `UIExtension/UIPatches/ACArmyOverlayArmyListPatch.cs` substitui o `Window` do prefab `ArmyOverlay`.
- O patch carrega o XML original de `SandBox/GUI/Prefabs/Map/ArmyOverlay.xml`, extrai `ArmyOverlayWidget` e o injeta no placeholder `ArmyCommanderOriginalArmyOverlayWidgetPlaceholder`.
- `GUI/ArmyOverlayWindow.xml` adiciona o widget customizado `ACOverlayWidget` e conserva o overlay original logo depois.
- `HarmonyPatches/ArmyOverlayWidgetPatch.cs` ajusta posicao/paginacao do overlay customizado e propaga o estado expandido para `ACArmyOverlayUIContext.IsExtended`.
- `HarmonyPatches/ChatLogWidgetPatch.cs` registra o `ChatLogWidget` e ajusta `MarginBottom` dinamicamente para abrir espaco ao overlay.

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
5. `ArmyMenuOverlayVM_ExecuteOpenArmyManagement_Patch` chama `OpenArmyManagement` diretamente.

`MapScreen_OnRefreshState_Patch` recria ou remove o overlay de exercito conforme `ACHelpers.ShouldShowArmyOverlayForPlayer()`. `MapBarVM_GetIsGatherArmyVisible_Patch` esconde o botao vanilla de gather quando o overlay customizado deve ficar visivel.

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
  - se o jogador nao pode comandar exercitos, usa a party do jogador;
  - se nao ha exercito selecionado, usa a party do jogador;
  - caso contrario, usa a leader party do exercito selecionado.
- Cria o item principal (`_mainPartyItem`) e adiciona ao carrinho.
- Popula a lista esquerda com parties do mesmo mapa/faccao.
- Se a party principal ja lidera um exercito, move membros existentes para o carrinho com custo zero.
- Adiciona a main party tambem na esquerda quando o jogador pode comandar exercitos.
- Reordena listas e chama refresh original.

Fluxos importantes:

- `PlayerHasArmySetterPrefix`: so deixa `PlayerHasArmy` verdadeiro para army liderada pela main party.
- `ManagementItemComparerComparePrefix` e `OrderPartiesInPlace`: mantem lider atual no topo, depois itens no carrinho, main party, leaders, elegiveis e membros bloqueados.
- `OnFirstPartyAdded`: quando o primeiro item e adicionado, ele vira a party principal/contexto do exercito a criar ou editar.
- `OnArmyLeaderRemoved`: retirar o lider limpa a selecao e volta a tela para estado de criacao.
- `CustomOnAddToCart` e `CustomOnRemove`: substituem os fluxos vanilla para suportar editar exercitos alheios.
- `ExecuteDonePrefix`: aplica coesao, cria exercito novo, adiciona membros, grava/atualiza ordens, recalcula AI, desconta influencia, remove parties e fecha a tela.
- `CustomDisbandArmy`: desfaz exercito liderado pelo jogador setando `Army = null`; para outro lider, usa `DisbandArmyAction.ApplyByReleasedByPlayerAfterBattle`.
- `ExecuteResetPrefix` e `ExecuteCancelPrefix`: restauram influencia inicial e limpam alteracoes temporarias.
- `OnFinalizePostfix`: finaliza o mixin e remove `ACArmyManagementUIContext.Instance`.

## Controles customizados da gestao

`ArmyManagementVMMixIn` injeta/expõe controles extras usados por `GUI/ACArmyManagementWidgets.xml` e pelo wrapper do painel direito.

Estado controlado:

- comportamento do exercito (`Defender` ou `Besieger`);
- assentamento alvo;
- assentamento de reuniao;
- `CanEngageEnemyParties`;
- `CanHelpAlliedParties`;
- `CanResupply`;
- `CanRunFromDanger`;
- envio de influencia;
- remocao de ordens.

Regras principais:

- Para exercito novo, usa capital possivel do reino como alvo/reuniao inicial e defaults permissivos.
- Para exercito existente com ordem salva, carrega a ordem do `ArmyCommandsBehaviorStore`.
- Para exercito existente sem ordem salva, usa `ACAIBehaviorHelpers.GetDefaultAiCommands`.
- O alvo escolhido em assentamento aliado seta comportamento `Defender`.
- O alvo escolhido em assentamento inimigo seta comportamento `Besieger`.
- O ponto de reuniao fica habilitado quando o exercito esta esperando membros.
- `CanHelpAlliedParties` so fica habilitado quando `CanEngageEnemyParties` esta desativado.
- `ExecuteRemoveOrders` remove a entrada de `ArmyCommandsBehaviorStore` e volta o contexto para os comandos default da AI.
- `ExecuteSendInfluence` transfere 50 influencia do jogador para o clan do lider do exercito selecionado.

`UIExtension/UIPatches/ACArmyManagementRightPanelDisbandButtonPatch.cs` substitui o `DisbandButton` do painel direito por um wrapper que inclui `Remove Orders` ao lado do botao original.

`OpenArmyManagement_All_Patch` roda apos aberturas vindas do map bar, map overlay ou kingdom screen, marca `movieIsLoaded = true` e chama `UpdateWidgets`.

## Ordens de AI-led armies

O fluxo de comando usa `ArmyCommandsBehaviorStore`.

Cada ordem salva contem:

- `ArmyType`: `Besieger` ou `Defender`;
- `TargetSettlement`: alvo principal;
- `GatherSettlement`: local usado enquanto o exercito espera membros;
- `CanEngageEnemyParties`;
- `CanHelpAlliedParties`;
- `CanResupply`;
- `CanRunFromDanger`.

Quando `ExecuteDonePrefix` cria ou atualiza um exercito nao liderado pela main party, ele compara os novos comandos com os comandos default da AI e com comandos antigos. Se houver diferenca, salva a ordem e chama `ACAIBehaviorHelpers.OnPlayerArmyCommandChanged` quando o exercito esta disponivel.

`SetPartyAiActionPatch.cs` intercepta `SetPartyAiAction.ApplyInternal` e delega a decisao para `ACAIBehaviorHelpers.AiBehaviorRecalculated`.

`DefaultMobilePartyAIModelPatch.cs` corta iniciativa de AI no nivel de `GetBestInitiativeBehavior`:

- bloqueia `EngageParty` quando `CanEngageEnemyParties` e falso;
- preserva ajuda a aliados quando `CanHelpAlliedParties` e verdadeiro e a party alvo esta lutando com aliado;
- bloqueia comportamentos de fuga quando `CanRunFromDanger` e falso.

`AiPartyThinkBehaviorPatch.cs` faz dois transpilers:

- trata mercenarios com politica ativa como nao mercenarios para logica de gathering;
- troca `SiegeEvent.FinalizeSiegeEvent` por `FinalizeSiegeEventIfAllowed`, impedindo fim de cerco quando a ordem do jogador manda continuar e a situacao permite.

## Recalculo e ressuprimento de AI

`ACAIBehaviorHelpers` concentra a logica de execucao das ordens.

Funcoes principais:

- `GetDefaultAiCommands`: captura estado vanilla atual do exercito.
- `ValidatePlayerCommandAndAskIfNeeded`: valida alvo depois de paz/mudanca de dono e pergunta se o exercito deve esperar ou voltar a AI vanilla.
- `ApplyDefaultFallBackBehavior`: transforma a ordem em defesa passiva de um assentamento seguro, com combate/ajuda/ressuprimento/fuga desativados.
- `ReEnableAI`: libera decisoes e agenda rethink.
- `NewArmyCommandApplied`: aplica ordem nova para waiting, besieger ou defender.
- `FindBestSettlementForResupplying`: escolhe assentamento proximo para comida/tropas sem repetir o ultimo ou penultimo assentamento visitado.
- `FindBestSettlementForWaiting`: escolhe cidade/castelo seguro para espera.
- `AiBehaviorRecalculated`: decide se um comando vanilla deve ser substituido pela ordem salva.
- `ACShouldAttackerEndSiege`: impede fim de cerco quando a ordem manda continuar e a necessidade de comida nao exige encerrar.
- `ACShouldArmyContinueOrStartResupply`: usa histerese para decidir ressuprimento.

`ArmyCommandsContext.ArmyLastVisitedSettlementCache` e atualizado por `MobileParty_LastVisitedSettlement_Setter_Patch` para evitar loops de ressuprimento. `ArmyCommandsContext.ArmyIsResupplyingDic` registra se o exercito ja esta em ciclo de ressuprimento e ajusta thresholds de comida/tropas.

Thresholds atuais:

- Besieger fora de cerco: comida abaixo de 15 dias para iniciar, 20 para continuar.
- Defender/nao besieger fora de cerco: comida abaixo de 10 dias para iniciar, 15 para continuar.
- Troops ratio abaixo de 0.65 para iniciar, 0.75 para continuar.
- Besieger em cerco: comida abaixo de 5 dias; nao busca tropas nesse caso.

## Regras de elegibilidade e criacao

`DefaultArmyManagementCalculationModelPatch.cs` altera a elegibilidade das parties e regras de criacao de exercito.

`CheckPartyEligibility` e substituido por prefix completo:

- bloqueia party nula;
- bloqueia mercenarios sem politica quando a selecao ainda nao tem `currentMainParty`;
- bloqueia parties ocupadas, jogador ocupado, ruler quando inadequado, membros de outro exercito e parties pequenas;
- permite selecionar lideres de exercitos existentes quando o jogador tem permissao de comando.

`CanLordCreateArmy` tambem e substituido:

- permite mercenarios apenas quando a politica `Mercenary Army Leaders` esta ativa;
- filtra parties disponiveis;
- limita o quanto do reino pode estar comprometido com exercitos por uma heuristica de 70%;
- exige forca total minima de 1000 para criar exercito AI.

`CanPlayerCreateArmy` recebe transpiler para substituir a checagem simples de `Clan.IsUnderMercenaryService` por `IsUnderMercenaryServiceAndNoPermission`, que considera permissao de dialogo e politica.

`CampaignUIHelper_GetCanManageCurrentArmyWithReason_Patch` libera ou bloqueia o acesso a gestao de exercito com base em:

- jogador ocupado;
- permissao de comando de exercito;
- servico mercenario sem permissao/politica;
- jogador ja sendo membro de outro exercito.

## Politica de mercenarios

`DefaultPoliciesPatch.cs` cria a politica `army_commander_mercenary_army_leaders` durante `DefaultPolicies.InitializeAll`.

Impacto declarado:

- mercenarios podem formar e liderar exercitos em servico do reino;
- o cla governante paga 100 de influencia quando um exercito mercenario e formado.

O custo de 100 e aplicado no postfix de `Army.Gather` quando a policy esta ativa para o clan lider do exercito.

`ACHelpers.HasPlayerPermissionForMercenaryArmyLeadership` retorna verdadeiro quando:

- o jogador esta sob servico mercenario no reino atual; e
- o reino salvo em `ACPermissionsStore` e o reino atual; ou
- a politica `Mercenary Army Leaders` esta ativa para o clan.

## Dispersao, coesao e eventos de exercito

`ArmyPatch.cs` cobre eventos de gather/disperse:

- `Army_DisperseInternal_Patch` remove ordens salvas do exercito dispersado e atualiza o overlay.
- `Army_Gather_Patch` cobra influencia do governante para exercito mercenario com politica ativa e atualiza o overlay.
- `Army_SendLeaderPartyToReachablePointAroundPosition_ReversePatch` permite reusar o envio vanilla para ponto de reuniao.

`DisbandArmyActionPatch.cs` protege exercitos com ordens:

- impede dispersao por `Inactivity` e `ObjectiveFinished`;
- em `CohesionDepleted`, tenta gastar influencia do clan lider para recuperar coesao antes de dispersar.

## Outros patches relevantes

- `ArmyManagementItemVMPatch.cs`: ajusta distancia para a party lider selecionada, recalcula tempo por velocidade e troca nome/forca de leaders de exercito na lista.
- `CampaignUIHelperPatch.cs`: substitui `GetCanManageCurrentArmyWithReason`.
- `MapBarVMPatch.cs`: esconde o botao vanilla de gather army quando o overlay customizado deve aparecer.
- `MapScreenPatch.cs`: substitui `IMapStateHandler.OnRefreshState` para criar/remover overlay de exercito.
- `MobilePartyPatch.cs`: registra o penultimo assentamento visitado por leaders comandados, usado pelo ressuprimento.
- `ChatLogWidgetPatch.cs`: ajusta a margem inferior do chat log conforme overlay expandido/contraido e quantidade de linhas.
- `ACArmyManagementPatch.cs`: injeta `GUI/ACArmyManagementWidgets.xml` na tela de gestao.
- `ACArmyManagementRightPanelDisbandButtonPatch.cs`: injeta o wrapper de `Remove Orders` + botao original de disband.
- `OpenArmyManagement_All_Patch.cs`: garante atualizacao dos widgets customizados apos a abertura da tela.

## Helpers e calculos

`ACHelpers` concentra regras de disponibilidade e metricas:

- comparacao segura de `MBObjectBase`;
- disponibilidade do exercito para receber ordens;
- se jogador/party/exercito esta ocupado;
- se settlement esta em condicao aceitavel;
- se o overlay deve aparecer;
- permissao de comando e lideranca mercenaria;
- contagens de parties/tropas;
- distancia em dias;
- comida, influencia, coesao e custos;
- dias ate comida acabar;
- capital possivel do reino;
- agrupamento de tropas por `FormationClass`.

`ACHintHelpers` constroi tooltips para:

- totais do reino no topo do overlay;
- parties/tropas/comida/coesao/influencia de cada exercito.

`ACCalculationModel.DistributeToSmallestKeepOriginalOrder` distribui um incremento inteiro elevando primeiro os menores valores e retornando a ordem original.

`ACActions` contem helpers para enviar itens, transferir influencia, subtrair/adicionar influencia e mover recursos entre parties/clans.

## Testes manuais

`docs/ArmyCommander_InGame_Test_Procedures.md` cobre regressao manual para:

- persistencia de ordens apos save/load;
- politica `Mercenary Army Leaders`;
- criacao de exercitos por ruler, vassalo e mercenario;
- dialogo de permissao mercenaria;
- fim/reentrada de contrato mercenario;
- casos negativos e riscos conhecidos.

Esse documento ainda deve ser estendido para cobrir especificamente o dialogo de vassalo e as flags novas de comando (`CanEngageEnemyParties`, `CanHelpAlliedParties`, `CanResupply`, `CanRunFromDanger`).

## Pontos frageis observados

- Muitos patches acessam campos/metodos privados por `AccessTools`. Mudancas de versao do Bannerlord podem quebrar nomes como `_partiesToRemove`, `_mainPartyItem`, `_armyOverlay`, `ApplyInternal`, `ArmyToUse`, `SendLeaderPartyToReachablePointAroundPosition` e `GetInfluenceBudgetWhileCreatingArmy`.
- Os reverse patches lancam `NotImplementedException` por design se chamados sem Harmony substituir o corpo. Isso e esperado, mas dificulta testes unitarios comuns.
- A persistencia de ordens depende de ids de heroi e settlement. Se o lider deixar de liderar o exercito, se o reino do jogador for nulo ou se o alvo sumir, o comando e descartado no load.
- `FindArmyByLeaderHeroId` assume `Clan.PlayerClan.Kingdom` disponivel durante restore.
- `OnSettlementOwnerChanged` acessa `oldOwner.Clan.Kingdom` em alguns caminhos; eventos com `oldOwner`/`Clan` nulos seriam perigosos.
- `OpenArmyManagement_All_Patch.Postfix` assume que `ACArmyManagementUIContext.Instance` existe apos a abertura.
- `FindBestSettlementForResupplying` pode retornar null; chamadas de ressuprimento devem tolerar isso para evitar aplicar ordem sem settlement.
- Ha valores magicos importantes: envio de 50 influencia, custo de 100 para exercito mercenario, limite de 70% de parties do reino em exercitos, forca minima 1000, thresholds de comida/tropas e relacoes/tier dos dialogos.
- `BannerlordDir` esta hardcoded para uma instalacao local. Outro ambiente precisa ajustar a propriedade no `.csproj`.
- `ACPolicyStore.MercenaryArmyLeadersPolicy` depende do patch de `DefaultPolicies.InitializeAll`; codigo que consultar antes disso precisa tolerar null.
- `ACActions.SendItemQuantityOneToOne` calcula `amount_to_give`, mas chama `SendItem(..., quantity)` dentro do loop. Isso parece suspeito se a intencao era enviar somente a quantidade daquele item.
- O XML e o mixin possuem varios textos hardcoded em ingles e parte das strings ainda usa ids/localizacao parcial.

## Onde mexer para tarefas comuns

- Mudar visual do overlay: `GUI/ArmyOverlayWindow.xml` e `GUI/Brushes/ArmyCommanderBrushes.xml`.
- Mudar metricas das linhas: `ACArmyLineWidgetBuilders`, `ACArmyLineUIContext`, `ACHelpers` e `ACHintHelpers`.
- Mudar comportamento da tela de gestao: `HarmonyPatches/ArmyManagementVMPatch.cs`.
- Mudar controles da tela de gestao: `UIExtension/MixIns/ACArmyManagementVMMixIn.cs`, `GUI/ACArmyManagementWidgets.xml` e `GUI/ArmyManagementRightPanelDisbandButtonWrapper.xml`.
- Mudar regras de permissao/elegibilidade: `DefaultArmyManagementCalculationModelPatch.cs`, `CampaignUIHelperPatch.cs`, `ACHelpers.cs` e behaviors de dialogo.
- Mudar comandos de exercitos AI-led: `ACAIBehaviorHelpers.cs`, `SetPartyAiActionPatch.cs`, `DefaultMobilePartyAIModelPatch.cs`, `AiPartyThinkBehaviorPatch.cs` e o trecho `ExecuteDonePrefix` em `ArmyManagementVMPatch.cs`.
- Mudar persistencia de ordens: `ACArmyCommanderBehavior.cs` e `ArmyCommandsBehaviorStore.cs`.
- Mudar permissao de mercenario/vassalo: `ACMercenaryArmyLeadershipDialogueBehavior.cs`, `ACVassalArmyCommanderDialogueBehavior.cs`, `ACPermissionsStore.cs` e `ACHelpers.cs`.
- Mudar politica customizada: `DefaultPoliciesPatch.cs`, `AiPartyThinkBehaviorPatch.cs`, `DefaultArmyManagementCalculationModelPatch.cs` e `ArmyPatch.cs`.
