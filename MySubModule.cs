using Bannerlord.UIExtenderEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ArmyCommander
{
    public class MySubModule : MBSubModuleBase
    {
        private UIExtender _uiExtender;
        private Harmony _harmony;

        private const string HarmonyId = "ArmyCommander";

        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArmyCommander"
        );

        private static readonly string LogPath = Path.Combine(
            LogDirectory,
            "ArmyCommander_Debug.log"
        );

        protected override void OnSubModuleLoad()
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);

                Log("");
                Log("==================================================");
                Log("OnSubModuleLoad começou");
                Log("Assembly: " + Assembly.GetExecutingAssembly().FullName);
                Log("Assembly location: " + Assembly.GetExecutingAssembly().Location);

                Log("Chamando base.OnSubModuleLoad()");
                base.OnSubModuleLoad();
                Log("base.OnSubModuleLoad() concluído");

                Log("Criando Harmony");
                _harmony = new Harmony(HarmonyId);
                Log("Harmony criado");

                Log("Chamando Harmony.PatchAll()");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log("Harmony.PatchAll() concluído");

                Log("Criando UIExtender");
                _uiExtender = UIExtender.Create("ArmyCommander");
                Log("UIExtender criado");

                Log("Registrando assembly no UIExtender");
                _uiExtender.Register(Assembly.GetExecutingAssembly());
                Log("UIExtender.Register() concluído");

                Log("Habilitando UIExtender");
                _uiExtender.Enable();
                Log("UIExtender.Enable() concluído");

                Log("OnSubModuleLoad terminou com sucesso");
            }
            catch (TargetInvocationException ex)
            {
                Log("ERRO: TargetInvocationException em OnSubModuleLoad");
                LogException(ex);
                Log("InnerException principal:");
                LogException(ex.InnerException);

                throw;
            }
            catch (ReflectionTypeLoadException ex)
            {
                Log("ERRO: ReflectionTypeLoadException em OnSubModuleLoad");
                LogException(ex);

                if (ex.LoaderExceptions != null)
                {
                    Log("LoaderExceptions:");

                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        LogException(loaderException);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                Log("ERRO: Exception em OnSubModuleLoad");
                LogException(ex);

                throw;
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try
            {
                Log("OnSubModuleUnloaded começou");

                base.OnSubModuleUnloaded();

                Log("Desabilitando UIExtender");
                _uiExtender?.Disable();

                Log("Deregistrando UIExtender");
                _uiExtender?.Deregister();

                _uiExtender = null;

                Log("Removendo patches Harmony");
                _harmony?.UnpatchAll(HarmonyId);

                _harmony = null;

                Log("OnSubModuleUnloaded terminou com sucesso");
            }
            catch (Exception ex)
            {
                Log("ERRO: Exception em OnSubModuleUnloaded");
                LogException(ex);

                throw;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            try
            {
                Log("OnGameStart começou");

                if (game == null)
                {
                    Log("game está null");
                    return;
                }

                if (gameStarterObject == null)
                {
                    Log("gameStarterObject está null");
                    return;
                }

                Log("GameType: " + game.GameType?.GetType().FullName);

                if (game.GameType is Campaign)
                {
                    Log("GameType é Campaign");

                    CampaignGameStarter campaignGameStarter = gameStarterObject as CampaignGameStarter;

                    if (campaignGameStarter == null)
                    {
                        Log("gameStarterObject NÃO é CampaignGameStarter. Tipo real: " + gameStarterObject.GetType().FullName);
                        return;
                    }

                    Log("Adicionando behavior ArmyCommander");
                    campaignGameStarter.AddBehavior(new ArmyCommander());
                    Log("Behavior ArmyCommander adicionado");
                }
                else
                {
                    Log("GameType não é Campaign. Ignorando AddBehavior.");
                }

                Log("OnGameStart terminou com sucesso");
            }
            catch (TargetInvocationException ex)
            {
                Log("ERRO: TargetInvocationException em OnGameStart");
                LogException(ex);
                Log("InnerException principal:");
                LogException(ex.InnerException);

                throw;
            }
            catch (Exception ex)
            {
                Log("ERRO: Exception em OnGameStart");
                LogException(ex);

                throw;
            }
        }

        private static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);

                File.AppendAllText(
                    LogPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + message + Environment.NewLine
                );
            }
            catch
            {
                // Não deixa erro de log derrubar o jogo.
            }
        }

        private static void LogException(Exception ex)
        {
            if (ex == null)
            {
                Log("Exception está null");
                return;
            }

            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Tipo: " + ex.GetType().FullName);
                sb.AppendLine("Mensagem: " + ex.Message);
                sb.AppendLine("Source: " + ex.Source);
                sb.AppendLine("TargetSite: " + ex.TargetSite);
                sb.AppendLine("StackTrace:");
                sb.AppendLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    sb.AppendLine("----- InnerException -----");
                    sb.AppendLine("Tipo: " + ex.InnerException.GetType().FullName);
                    sb.AppendLine("Mensagem: " + ex.InnerException.Message);
                    sb.AppendLine("Source: " + ex.InnerException.Source);
                    sb.AppendLine("TargetSite: " + ex.InnerException.TargetSite);
                    sb.AppendLine("StackTrace:");
                    sb.AppendLine(ex.InnerException.StackTrace);
                }

                Log(sb.ToString());
            }
            catch
            {
                // Não deixa erro de log derrubar o jogo.
            }
        }
    }
}