using ArmyCommander.ACBehaviors;
using ArmyCommander.ACBehaviors.Context;
using ArmyCommander.Store;
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

                base.OnSubModuleLoad();



                _harmony = new Harmony(HarmonyId);



                _harmony.PatchAll(Assembly.GetExecutingAssembly());



                _uiExtender = UIExtender.Create("ArmyCommander");



                _uiExtender.Register(Assembly.GetExecutingAssembly());



                _uiExtender.Enable();



            }
            catch (TargetInvocationException ex)
            {

                LogException(ex);

                LogException(ex.InnerException);

                throw;
            }
            catch (ReflectionTypeLoadException ex)
            {

                LogException(ex);

                if (ex.LoaderExceptions != null)
                {


                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        LogException(loaderException);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {

                LogException(ex);

                throw;
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try
            {


                base.OnSubModuleUnloaded();


                _uiExtender?.Disable();


                _uiExtender?.Deregister();

                _uiExtender = null;


                _harmony?.UnpatchAll(HarmonyId);

                _harmony = null;


            }
            catch (Exception ex)
            {

                LogException(ex);

                throw;
            }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            try
            {


                if (game == null)
                {

                    return;
                }

                if (gameStarterObject == null)
                {

                    return;
                }

                if (game.GameType is Campaign)
                {


                    CampaignGameStarter campaignGameStarter = gameStarterObject as CampaignGameStarter;

                    if (campaignGameStarter == null)
                    {

                        return;
                    }

                    ArmyCommandsContext.Reset();
                    ArmyCommandsBehaviorStore.Reset();
                    ACPermissionsStore.Reset();

                    campaignGameStarter.AddBehavior(new ACArmyCommanderBehavior());
                    campaignGameStarter.AddBehavior(new ACMercenaryArmyLeadershipDialogueBehavior());
                    campaignGameStarter.AddBehavior(new ACVassalArmyCommanderDialogueBehavior());

                }
            }
            catch (TargetInvocationException ex)
            {

                LogException(ex);

                LogException(ex.InnerException);

                throw;
            }
            catch (Exception ex)
            {

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
                // Do not let logging errors take the game down.
            }
        }

        private static void LogException(Exception ex)
        {
            if (ex == null)
            {

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
                // Do not let logging errors take the game down.
            }
        }
    }
}
