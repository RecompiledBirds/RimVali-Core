using RimValiCore.CompatiblityPatches;
using RimValiCore.RVR;
using RimValiCore.RVRFrameWork;
using System;

using System.Linq;
using System.Reflection;
using Verse;

namespace RimValiCore
{
   
   public enum RimValiCore_Stage
    {
        Unloaded,
        Shaders,
        RVR,
        Ships,
        HealableMats,
        Floors,
        QLine,
        Compatiblity,
        Loaded,
    }
    [StaticConstructorOnStartup]
    public static class RVCInitalizer
    {
        
        public abstract class RVCPostLoad
        {
            public Action start;
            public virtual void InitAction()
            {

            }
            public void Init()
            {
                start = InitAction;
            }

            public RVCPostLoad()
            {

            }
        }


        public class RVCPreLoad
        {
            public Action start;
            public virtual void InitAction()
            {

            }
            public void Init()
            {
                start = InitAction;
            }

            public RVCPreLoad()
            {

            }
        }



        private static RimValiCore_Stage stage = RimValiCore_Stage.Unloaded;
        public static RimValiCore_Stage GetStage
        {
            get
            {
                return stage;
            }
        }
        public static Action postLoadAction;
        public static Action preLoadAction;

        /// <summary>
        /// Should be called in a preload function!
        /// Adds a postload function.
        /// </summary>
        /// <param name="postAction"></param>
        public static void AddPostLoadAction(Action postAction)
        {
            postLoadAction += postAction;
        }

        static RVCInitalizer()
        {
            Log.Message("<color=orange>[RimVali Core]: Starting up..</color>");
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes().Where(x =>x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(RVCPreLoad))))
                {
                    var preLoad = Activator.CreateInstance(t);
                    MethodInfo info = t.GetMethod("Init");
                    if (info != null)
                    {
                        info.Invoke(preLoad, null);
                        preLoadAction += (Action)t.GetField("start").GetValue(preLoad);
                    }
                }
            }
            preLoadAction?.Invoke();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(RVCPostLoad))))
                {
                    var postLoad = Activator.CreateInstance(t);
                    MethodInfo info = t.GetMethod("Init");
                    if (info != null)
                    {
                        info.Invoke(postLoad, null);
                        postLoadAction += (Action)t.GetField("start").GetValue(postLoad);
                    }
                }
            }
            Log.Message("[RimVali Core]: Loading shaders.");
            stage = RimValiCore_Stage.Shaders;
            AvaliShaderDatabase.LoadShaders();
            Log.Message("[RimVali Core]: Shaders loaded.");
            Log.Message("[RimVali Core]: Loading RVR Framework..");
            stage = RimValiCore_Stage.RVR;
            Log.Message("[RimVali Core]: Intializing RVR.");
            Log.Message("[RimVali Core]: Starting restriction setup.");
            RaceRestrictor.RunRestrictions();
            Restrictions.InitRestrictions();
            RVR.RVR.DoPatches();
            Log.Message("[RimVali Core]: Launching ships!");
            stage = RimValiCore_Stage.Ships;
            Ships.ShipPatches.DoPatches();
            Log.Message("[RimVali Core]: Ships in orbit!");
            Log.Message("[RimVali Core]: Preparing healable materials.");
            stage = RimValiCore_Stage.HealableMats;
            HealableMaterial.HealableMats.Intialize();
            Log.Message("[RimVali Core]: Successfully inialized healable materials.");
            Log.Message("[RimVali Core]: Constructing floors.");
            stage = RimValiCore_Stage.Floors;
            FloorConstructor.Initalize();
            Log.Message("[RimVali Core]: Constructed floors.");
            stage = RimValiCore_Stage.Compatiblity;
            Log.Message("[RimVali Core]: Doing compatiblity patches!");
            RVCCompatiblityPatches.DoPatches();
            Log.Message("[RimVali Core]: Finished compatiblity patches.");
            Log.Message("[RimVali Core]: Asking QLine to tell a story.");
            //todo
            Log.Message("[RimVali Core]: Finished listening to the story!");
            stage = RimValiCore_Stage.Loaded;
            Log.Message($"<color=orange>[RimVali Core]: RimVali Core has started!</color>");

            postLoadAction?.Invoke();
        }
    
    }
}