using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
        Loaded
    }
    [StaticConstructorOnStartup]
    public static class Initalizer
    {
       private static RimValiCore_Stage stage = RimValiCore_Stage.Unloaded;
        public static RimValiCore_Stage GetStage
        {
            get
            {
                return stage;
            }
        }
        static Initalizer()
        {
            Log.Message("<color=orange>[RimVali Core]: Starting up..</color>");
            Log.Message("[RimVali Core]: Loading shaders.");
            stage = RimValiCore_Stage.Shaders;
            AvaliShaderDatabase.LoadShaders();
            Log.Message("[RimVali Core]: Shaders loaded.");
            Log.Message("[RimVali Core]: Loading RVR Framework..");
            stage = RimValiCore_Stage.RVR;
            Log.Message("[RimVali Core]: Intializing RVR.");
            RVR.Restrictions.InitRestrictions();
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
            Log.Message("[RimVali Core]: Asking QLine to tell a story.");
            stage = RimValiCore_Stage.QLine;
            //todo
            Log.Message("[RimVali Core]: Finished listening to the story!");
            stage = RimValiCore_Stage.Loaded;
            Log.Message($"<color=orange>[RimVali Core]: RimVali Core has started!</color>");
        }
    
    }
}
