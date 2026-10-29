using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace NorthernLab.Tests {
public class StationTests {
 GameObject root; StationGame game; StationConfig config;
 [SetUp] public void Setup(){root=new GameObject("Test station");game=root.AddComponent<StationGame>();config=ScriptableObject.CreateInstance<StationConfig>();config.shifts=new StationConfig.Shift[7];for(int i=0;i<7;i++)config.shifts[i]=new StationConfig.Shift{heatingMultiplier=1};game.config=config;game.mode=StationGame.Mode.Playing;game.state.generator=true;}
 [TearDown] public void Teardown(){Object.DestroyImmediate(root);Object.DestroyImmediate(config);}
 void Call(string method,float dt){typeof(StationGame).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,new object[]{dt});}
 [Test] public void OverloadCutsAllPoweredSystems(){game.power=new[]{true,true,true,true};Assert.AreEqual(8,game.Load);Assert.IsFalse(game.Powered(0));game.power=new[]{true,false,false,true};Assert.IsTrue(game.Powered(0));Assert.IsTrue(game.Powered(3));}
 [Test] public void FinalSignalPausesWithoutReset(){game.state.shift=7;game.state.radio=3;game.panel="radio";game.radioTuned=true;game.power=new[]{true,false,false,true};Call("Simulate",1f);Assert.AreEqual(1,game.state.sos);game.power[3]=false;Call("Simulate",1f);Assert.AreEqual(1,game.state.sos);}
 [Test] public void FinalIsReachableWithoutUpgrades(){game.state.shift=7;game.state.sos=config.sosSeconds;game.clock=config.shiftSeconds;game.state.upgrades=0;Call("Simulate",.01f);Assert.AreEqual(StationGame.Mode.Won,game.mode);}
 [Test] public void LiftRouteChangesFloorBeforeApproachingTarget(){var obj=new GameObject("Target");try{var d=obj.AddComponent<StationDevice>();d.floor=1;d.transform.position=new Vector3(15,-5,0);game.destination=d;game.actor=new Vector2(-16,2);for(int i=0;i<100;i++)Call("Walk",.1f);Assert.AreEqual(1,game.floor);Assert.Greater(game.actor.x,0);}finally{Object.DestroyImmediate(obj);}}
 [Test] public void ColdFailureHasReason(){game.state.shift=3;game.state.temperature=.01f;game.state.generator=false;Call("Simulate",1);Assert.AreEqual(StationGame.Mode.Lost,game.mode);StringAssert.Contains("Переохлаждение",game.lossReason);}
 [Test] public void FirstShiftForgivesCold(){game.state.temperature=0;game.state.generator=false;Call("Simulate",1);Assert.GreaterOrEqual(game.state.temperature,25);Assert.AreNotEqual(StationGame.Mode.Lost,game.mode);}
 [Test] public void SaveSchemaRoundTripsCampaignState(){var a=new SaveData{shift=6,radio=3,parts=7,upgrades=10,fuse=true,sos=42};var b=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(a));Assert.AreEqual(a.shift,b.shift);Assert.AreEqual(a.upgrades,b.upgrades);Assert.AreEqual(a.sos,b.sos);Assert.IsTrue(b.fuse);}
 [Test] public void SevenShiftsCanCloseAtHeater(){for(int i=1;i<7;i++){game.state.shift=i;game.actor=new Vector2(-7,2);game.floor=0;game.clock=config.shiftSeconds;game.mode=StationGame.Mode.Playing;Call("Simulate",.01f);Assert.AreEqual(StationGame.Mode.Report,game.mode,"Shift "+i);}}
}}
