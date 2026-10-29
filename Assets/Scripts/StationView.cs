using System.Collections.Generic;
using UnityEngine;
namespace NorthernLab {
public static class StationView {
 static List<SpriteRenderer> lamps=new List<SpriteRenderer>(), fans=new List<SpriteRenderer>(), frosts=new List<SpriteRenderer>();
 static List<GameObject> decor=new List<GameObject>(); static SpriteRenderer generator;static Transform snow;static TextMesh status;static StationGame current;
 static Color C(string s){Color c;ColorUtility.TryParseHtmlString(s,out c);return c;}
 public static SpriteRenderer Box(string name,Vector2 p,Vector2 size,Color col,int order=0,Transform parent=null){var go=new GameObject(name);if(parent!=null)go.transform.SetParent(parent);go.transform.position=p;var r=go.AddComponent<SpriteRenderer>();r.sprite=Resources.Load<Sprite>("pixel");r.color=col;r.sortingOrder=order;go.transform.localScale=new Vector3(size.x,size.y,1);return r;}
 public static TextMesh Label(string text,Vector2 p,float size=.22f,Color? color=null){var go=new GameObject(text);go.transform.position=new Vector3(p.x,p.y,-1);var t=go.AddComponent<TextMesh>();t.text=text;t.font=Resources.Load<Font>("StationFont");t.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;t.fontSize=42;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=color??C("#96adba");t.GetComponent<MeshRenderer>().sortingOrder=15;return t;}
 static SpriteRenderer Sprite(string path,Vector2 p,int order=4){var go=new GameObject(path);go.transform.position=p;var r=go.AddComponent<SpriteRenderer>();r.sprite=Resources.Load<Sprite>(path);r.sortingOrder=order;return r;}
 public static void Build(StationGame g){
 current=g;lamps.Clear();fans.Clear();frosts.Clear();decor.Clear();
 if(Camera.main==null){var go=new GameObject("Main Camera");go.tag="MainCamera";g.worldCamera=go.AddComponent<Camera>();g.worldCamera.orthographic=true;go.AddComponent<AudioListener>();}else g.worldCamera=Camera.main;
 g.worldCamera.backgroundColor=C("#071322");g.worldCamera.transform.position=new Vector3(0,0,-10);g.worldCamera.orthographic=true;g.worldCamera.orthographicSize=13.5f;
 Box("Permafrost",new Vector2(0,-11),new Vector2(100,12),C("#121e2b"),-20);Box("Surface snow",new Vector2(0,6.9f),new Vector2(100,.5f),C("#66889f"),-19);
 for(int i=0;i<14;i++){Box("Ice layers",new Vector2(Random.Range(-26,26),Random.Range(-16,-7)),new Vector2(Random.Range(1,8),.08f),C("#243847"),-18);}
 string[] rooms={"01 / ГЕНЕРАТОРНАЯ","02 / ЖИЛОЙ ОТСЕК","03 / МАСТЕРСКАЯ","04 / РАДИОРУБКА","05 / СКЛАД","06 / АРХИВ","07 / БИОСДЕРЖИВАНИЕ","08 / НИЖНЯЯ ПЛОЩАДКА"};
 for(int i=0;i<8;i++){
 float x=-15+(i%4)*10, y=i<4?3:-4;Color color=i==1?C("#403528"):i==6?C("#163d39"):C("#253344");
 Box(rooms[i],new Vector2(x,y),new Vector2(9.7f,6.6f),color,-8);Box("Floor",new Vector2(x,y-2.6f),new Vector2(10,.45f),C("#677c87"),2);Box("Ceiling",new Vector2(x,y+3),new Vector2(10,.2f),C("#455b6a"),1);
 for(int k=0;k<4;k++){Box("Wall rib",new Vector2(x-4+k*2.7f,y),new Vector2(.12f,5.5f),C("#334b57"),-5);}
 Label(rooms[i],new Vector2(x,y+2.3f),.16f);
 var lamp=Box("Lamp",new Vector2(x,y+1.8f),new Vector2(2,.12f),C("#adc9bc"),3);lamps.Add(lamp);
 Box("Window",new Vector2(x+2,y+.4f),new Vector2(2,1.8f),C(i<4?"#102b46":"#0c1b29"),-3);
 frosts.Add(Box("Frost",new Vector2(x+2,y+.4f),new Vector2(2,1.8f),new Color(.65f,.85f,1,.05f),-2));
 var fan=Sprite("fan",new Vector2(x-3.5f,y+1),2);fans.Add(fan);
 for(int k=0;k<6;k++)Box("Pipe",new Vector2(x-4.6f+k*1.5f,y-2.1f),new Vector2(.65f,.12f),C("#8294a0"),3);
 }
 // Lift shaft is a readable central connection, with open corridors through each floor.
 Box("Lift shaft",new Vector2(0,-.5f),new Vector2(1.8f,13.5f),C("#0a1521"),0);for(int i=-1;i<=1;i+=2)Box("Lift rail",new Vector2(i*.65f,-.5f),new Vector2(.08f,13),C("#839ba6"),3);
 Label("ЛИФТ ↕",new Vector2(0,6.5f),.18f);Box("Lift cage",new Vector2(0,1),new Vector2(1.5f,.2f),C("#d2a34c"),4);
 // Editable authored devices are loaded from the scene. Scene rebuild can author them again.
 g.devices.AddRange(Object.FindObjectsByType<StationDevice>(FindObjectsSortMode.None));
 foreach(var d in g.devices)DrawDevice(d);
 Sprite("bunk",new Vector2(-9,2.5f));Sprite("mug",new Vector2(-5,2.4f));Sprite("oldradio",new Vector2(-6,2.7f));
 Sprite("tank",new Vector2(14,-3.1f));Sprite("tank",new Vector2(17,-3.1f));Label("ОБРАЗЕЦ 02",new Vector2(15.5f,-1.3f),.16f);
 Label("NORTHERNLAB / 1978",new Vector2(-13,7.8f),.35f,C("#d1dae0"));Label("ПОЛЯРНЫЙ КОМПЛЕКС · ВХОД ПО ПРОПУСКАМ",new Vector2(11,7.8f),.18f);
 status=Label("",new Vector2(0,-9.4f),.22f);
 g.player=Sprite("tech_idle",g.actor,12);g.hand=Sprite("canister",g.actor,13);g.hand.transform.localScale=Vector3.one*.65f;
 g.creature=Sprite("creature",new Vector2(18,-5),11);g.creature.gameObject.SetActive(false);
 snow=new GameObject("Blizzard particles").transform;for(int i=0;i<150;i++){var s=Box("Snow",new Vector2(Random.Range(-30,30),Random.Range(6.8f,15)),new Vector2(.06f,.06f),new Color(.7f,.83f,1,Random.Range(.2f,.8f)),-9,snow);}
 var audio=new GameObject("Station Sound");audio.AddComponent<StationAudio>();
 }
 public static void DrawDevice(StationDevice d){string art="terminal";switch(d.kind){case DeviceKind.Fuel:art="canister";break;case DeviceKind.Generator:art="generator";break;case DeviceKind.Board:art="board";break;case DeviceKind.Heater:art="heater";break;case DeviceKind.Tools:art="tools";break;case DeviceKind.Fuse:art="fuse";break;case DeviceKind.Parts:art="parts";break;case DeviceKind.Record:art="record";break;case DeviceKind.Plant:art="plant";break;case DeviceKind.Shutter:art="shutter";break;case DeviceKind.Flare:art="flare";break;}
 var r=Sprite(art,d.position+Vector2.up*.4f,5);r.transform.SetParent(d.transform,true);if(d.kind==DeviceKind.Generator)generator=r;
 var t=Label(d.title,d.position+Vector2.down*.75f,.13f);t.transform.SetParent(d.transform,true);
 }
 public static void Refresh(StationGame g){foreach(var d in g.devices){foreach(Transform c in d.transform)c.gameObject.SetActive(!d.consumed);}}
 public static void Animate(StationGame g,float dt){if(g.player==null)return;
 float t=Time.time;bool walking=g.destination!=null;string anim=g.repairing?"tech_repair":walking?(Mathf.Sin(t*12)>0?"tech_walk1":"tech_walk2"):"tech_idle";
 g.player.sprite=Resources.Load<Sprite>(anim);g.player.transform.position=new Vector3(g.actor.x,g.actor.y+.4f+(walking?Mathf.Sin(t*18)*.04f:0),0);g.player.flipX=g.destination!=null&&g.destination.position.x<g.actor.x;
 g.hand.gameObject.SetActive(g.carried!="");g.hand.transform.position=g.player.transform.position+new Vector3(.5f,0,0);g.hand.sprite=Resources.Load<Sprite>(g.carried=="Канистра"?"canister":g.carried=="Инструмент"?"tools":"fuse");
 g.creature.gameObject.SetActive(g.monsterActive);g.creature.transform.position=new Vector3(g.monsterX,-4.4f+Mathf.Sin(t*7)*.1f,0);
 foreach(var f in fans)f.transform.Rotate(0,0,dt*(g.state.generator?150:8));
 for(int i=0;i<lamps.Count;i++){lamps[i].color=g.Powered(1)?new Color(.7f,.9f,.78f):new Color(.6f+Mathf.Sin(t*3)*.2f,.08f,.06f);frosts[i].color=new Color(.6f,.85f,1,Mathf.Clamp01(1-g.state.temperature/80)*.7f);}
 if(generator!=null){var p=generator.transform.localPosition;p.x=Mathf.Sin(t*60)*(g.state.generator?.03f:0);generator.transform.localPosition=new Vector3(p.x,.4f,0);generator.color=(g.state.upgrades&2)!=0?new Color(.75f,1,.8f):Color.white;}
 foreach(var d in g.devices){foreach(Transform c in d.transform)c.gameObject.SetActive(!d.consumed);var rend=d.GetComponentInChildren<SpriteRenderer>();if(rend!=null&&d.kind!=DeviceKind.Generator)rend.color=d.unlockShift>g.state.shift?new Color(.25f,.3f,.35f):((d.kind==DeviceKind.Heater&&(g.state.upgrades&1)!=0)?new Color(1,.75f,.5f):Color.white);}
 foreach(Transform s in snow){s.position+=new Vector3(-dt*5,-dt*1.2f,0);if(s.position.x<-30||s.position.y<6.8f)s.position=new Vector3(Random.Range(20,30),Random.Range(8,15),0);}
 status.text=g.warning>0?"! СТУК СНИЗУ · "+Mathf.CeilToInt(g.warning)+" с до контакта":g.Load>g.Supply&&g.state.generator?"ПЕРЕГРУЗКА СЕТИ · УМЕНЬШИТЕ НАГРУЗКУ НА ЩИТЕ":"";
 }
}
}
