using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace NorthernLab {
public class StationGame : MonoBehaviour {
 public StationConfig config;
 public enum Mode {Menu, Playing, Briefing, Report, Paused, Lost, Won, Settings}
 public Mode mode=Mode.Menu;
 public SaveData state=new SaveData();
 public bool[] power={true,true,false,false};
 public float clock, heat, danger, signalFrequency=88, targetFrequency=97.4f, tuneProgress;
 public bool boosted, broken, shutter, radioTuned;
 public string carried="", message="", panel="", lossReason="";
 public float messageTime, actionProgress;
 public StationDevice destination;
 public Vector2 actor=new Vector2(-16,2);
 public int floor;
 public List<StationDevice> devices=new List<StationDevice>();
 public SpriteRenderer player, hand, creature;
 public float monsterX=16, warning, quiet, monsterCooldown=25;
 public bool monsterActive, repairing;
 public float volume=.65f;
 public bool windowed=true;
 public Camera worldCamera;
 float stepTimer, repairTick, eventTimer, initialFuel, ambientPhase, cameraZoom=13.5f;
 int initialParts, reportChoice; bool tutorialMessage, arrival, settingsFromPause;
 string savePath {get{return Path.Combine(Application.persistentDataPath,"northernlab-save.json");}}
 public int Supply {get{return !state.generator||state.fuel<=0||broken?0:(boosted?config.boostPower:config.generatorPower)+((state.upgrades&8)!=0?1:0);}}
 public int Load {get {int n=0; for(int i=0;i<4;i++)if(power[i]) n+=config.demand[i]+(i==0&&state.shift==3?1:0);return n;}}
 public bool Powered(int i){return state.generator&&Supply>=Load&&power[i];}
 public bool HasSave {get{return File.Exists(savePath);}}
 void Awake(){if(config==null)config=Resources.Load<StationConfig>("StationBalance"); volume=PlayerPrefs.GetFloat("volume",.65f);windowed=PlayerPrefs.GetInt("windowed",1)==1; AudioListener.volume=volume;}
 void Start(){StationView.Build(this);}
 public void Notify(string s){message=s;messageTime=9;}
 public void NewGame(){state=new SaveData();power=new[]{true,true,false,false}; Save(); BeginShift();}
 public void Continue(){try {state=JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath)); if(state==null||state.shift<1||state.shift>7)throw new Exception(); BeginShift();}catch {Notify("Не удалось прочитать сохранение. Начните новую игру.");}}
 public void Save(){File.WriteAllText(savePath+".tmp",JsonUtility.ToJson(state,true)); if(File.Exists(savePath))File.Delete(savePath);File.Move(savePath+".tmp",savePath);}
 public void BeginShift(){
 clock=0;heat=0;danger=0;warning=0;eventTimer=0;broken=false;boosted=false;shutter=false;monsterActive=false;monsterCooldown=25;panel="";carried="";destination=null;floor=0;actor=new Vector2(-16,2);radioTuned=false;tuneProgress=0;actionProgress=0;quiet=0;arrival=false;
 initialFuel=state.fuel;initialParts=state.parts; tutorialMessage=state.shift>1;power=new[]{true,true,false,false};
 foreach(var d in devices)d.consumed=false;
 mode=Mode.Briefing; StationView.Refresh(this);
 }
 void Update(){
 if(Input.GetKeyDown(KeyCode.Escape)){if(panel!="")panel="";else if(mode==Mode.Playing)mode=Mode.Paused;else if(mode==Mode.Paused)mode=Mode.Playing;}
 CameraControl(); if(mode!=Mode.Playing){StationView.Animate(this,Time.deltaTime);return;}
 float dt=Mathf.Min(Time.deltaTime,.1f); clock+=dt;messageTime-=dt;
 if(Input.GetMouseButtonDown(0)&&Input.mousePosition.y<Screen.height-100&&panel==""&&Input.mousePosition.y>70){
 Vector2 w=worldCamera.ScreenToWorldPoint(Input.mousePosition);StationDevice best=null;float dist=1.6f;
 foreach(var d in devices){if(d.consumed)continue;float n=Vector2.Distance(d.position,w);if(n<dist){dist=n;best=d;}}
 if(best!=null){if(best.unlockShift>state.shift)Notify("Доступ откроется в смену "+best.unlockShift);else {destination=best;actionProgress=0;StationAudio.Beep(480,.04f);}}
 }
 Walk(dt); Simulate(dt); StationView.Animate(this,dt);
 }
 void CameraControl(){if(worldCamera==null)return;cameraZoom=Mathf.Clamp(cameraZoom-Input.mouseScrollDelta.y,7,18);worldCamera.orthographicSize=cameraZoom; Vector3 p=worldCamera.transform.position;if(Input.GetMouseButton(2))p+=new Vector3(-Input.GetAxis("Mouse X"),-Input.GetAxis("Mouse Y"),0)*.3f; if(Input.GetKey(KeyCode.Home)){p=new Vector3(0,0,-10);cameraZoom=13.5f;}p.x=Mathf.Clamp(p.x,-14,14);p.y=Mathf.Clamp(p.y,-6,6);worldCamera.transform.position=p;}
 void Walk(float dt){
 if(destination==null)return;
 float tx=destination.floor==floor?destination.position.x:0;
 if(Mathf.Abs(actor.x-tx)>.12f){actor.x=Mathf.MoveTowards(actor.x,tx,dt*4.5f);stepTimer-=dt;if(stepTimer<=0){StationAudio.Step();stepTimer=.3f;}return;}
 if(destination.floor!=floor){actor.y=Mathf.MoveTowards(actor.y,destination.position.y,dt*3);if(Mathf.Abs(actor.y-destination.position.y)<.1f){floor=destination.floor;StationAudio.Beep(220,.15f);}return;}
 if(Mathf.Abs(actor.y-destination.position.y)>.12f){actor.y=Mathf.MoveTowards(actor.y,destination.position.y,dt*3);return;}
 var d=destination;destination=null;Interact(d);
 }
 void Interact(StationDevice d){
 switch(d.kind){
 case DeviceKind.Fuel: if(carried==""){carried="Канистра";d.consumed=true;Notify("Канистра в руках. Отнесите к генератору.");}else Notify("Руки заняты. Положите предмет кнопкой HUD.");break;
 case DeviceKind.Tools: if(carried=="")carried="Инструмент";else Notify("Сначала освободите руки.");break;
 case DeviceKind.Fuse: if(carried==""){carried="Предохранитель";d.consumed=true;}break;
 case DeviceKind.Generator:
 if(carried=="Канистра"){state.fuel=Mathf.Min(config.fuelCapacity,state.fuel+45);carried="";state.generator=true;Notify("Генератор запущен. Шум в пределах служебной нормы.");StationAudio.Beep(180,.4f);}
 else if(broken){if(carried=="Инструмент"){panel="repair";repairTick=0;}else Notify("Нужен инструмент из мастерской.");}
 else if(state.fuel>0){state.generator=true;panel="generator";}else Notify("Нужно топливо со склада.");break;
 case DeviceKind.Board: if(carried=="Предохранитель"&&!state.fuse)panel="fuse";else panel="board";break;
 case DeviceKind.Heater:panel="rest";break;
 case DeviceKind.Radio: if(state.radio>=3)panel="radio";else if(carried=="Инструмент"&&state.parts>=2&&state.fuse){panel="repairRadio";repairTick=0;}else Notify("Ремонт: инструмент в руках, 2 запчасти и исправный предохранитель щита. Всего 3 этапа.");break;
 case DeviceKind.Parts:state.parts+=config.shifts[state.shift-1].parts;d.consumed=true;Notify("Найдены запчасти. Всего: "+state.parts);break;
 case DeviceKind.Record:state.records++;d.consumed=true;Notify(config.shifts[state.shift-1].record);break;
 case DeviceKind.Shutter:shutter=!shutter;Notify(shutter?"Аварийная заслонка закрыта. Проход снизу перекрыт.":"Заслонка открыта.");StationAudio.Beep(110,.3f);break;
 case DeviceKind.Plant:state.watered=true;Notify("Единственный здоровый образец полит. Ваш статус уточняется.");break;
 case DeviceKind.Flare:if(state.flares>0){state.flares--;monsterActive=false;warning=0;monsterCooldown=35;danger=Mathf.Max(0,danger-30);Notify("Фальшфейер: существо отступило.");StationAudio.Beep(880,.5f);}else Notify("Фальшфейеры закончились.");break;
 case DeviceKind.Lift:Notify("Выберите объект другого этажа — техник сам пройдёт через лифт.");break;
 }
 }
 void Simulate(float dt){
 if(state.generator&&state.fuel>0){state.fuel=Mathf.Max(0,state.fuel-dt*config.fuelPerSecond*(boosted?2.1f:1)*((state.upgrades&2)!=0?.7f:1));heat=Mathf.Clamp(heat+dt*(boosted?1.6f:-1.2f),0,100);if(heat>=100){broken=true;boosted=false;Notify("Перегрев генератора. Ремонт инструментом.");}}
 state.temperature=Mathf.Clamp(state.temperature+dt*(Powered(0)?config.warmthPerSecond:-config.coldPerSecond*config.shifts[state.shift-1].heatingMultiplier),0,100);
 if(!tutorialMessage&&state.generator&&Powered(0)){tutorialMessage=true;Notify("Питание восстановлено. Обнаружено сотрудников: 2.");}
 if(state.shift==1)state.temperature=Mathf.Max(25,state.temperature);
 if(state.temperature<=0){Lose("Переохлаждение: отопление слишком долго оставалось без питания.");return;}
 if(state.shift>=4){danger=Mathf.Clamp(danger+dt*(Powered(2)?-.8f:.38f)+(boosted?dt*.35f:0),0,100);}
 if(state.shift>=2&&!arrival&&clock>config.shiftSeconds*.55f){arrival=true;broken=state.shift==3||state.shift==6;Notify(broken?"Автоматика: остановка генератора. Требуется ремонт инструментом.":"Лифт вызван с закрытого уровня. Заявка на дополнительный персонал отклонена: по документам вы здесь не один.");StationAudio.Knock();}
 monsterCooldown-=dt;
 if(state.shift>=5&&!monsterActive&&warning<=0&&monsterCooldown<=0&&(danger>40||clock>config.shiftSeconds*.65f)){warning=config.warningSeconds;Notify("Три удара снизу. Красный индикатор: закройте заслонку или приготовьте фальшфейер.");StationAudio.Knock();}
 if(warning>0){warning-=dt;if(warning<=0){if(!shutter){monsterActive=true;monsterX=18;quiet=0;}else{danger=Mathf.Max(0,danger-25);monsterCooldown=30;Notify("За заслонкой скребут. Пока она держит.");}}}
 if(monsterActive){float target=(boosted&&Mathf.Abs(actor.x-monsterX)>5)?-17:actor.x;float speed=floor==1?2.3f:1.5f;monsterX=Mathf.MoveTowards(monsterX,target,dt*speed);quiet+=dt;
 if(floor==1&&Mathf.Abs(actor.x-monsterX)<1.1f){Lose("Контакт с образцом. Закройте заслонку до появления или используйте фальшфейер у лифта.");return;}
 if((shutter&&floor==0)||quiet>24){monsterActive=false;monsterCooldown=30;danger=20;}}
 if(panel=="rest"&&Powered(0)){state.temperature=Mathf.Min(100,state.temperature+dt*((state.upgrades&1)!=0?5:3));}
 if(panel=="radio"&&state.radio>=3&&Powered(3)&&radioTuned&&state.shift==7)state.sos=Mathf.Min(config.sosSeconds,state.sos+dt);
 if(state.shift==7&&state.sos>=config.sosSeconds&&clock>=config.shiftSeconds){mode=Mode.Won;panel="";Notify("Эвакуация подтверждена.");return;}
 if(clock>=config.shiftSeconds){if(state.shift==7){Notify("Эвакуация ждёт сигнал: завершите SOS. Смена продлена.");clock=config.shiftSeconds;}else if(Mathf.Abs(actor.x+7)<4&&floor==0){mode=Mode.Report;panel="";reportChoice=0;}else if(clock>config.shiftSeconds+60){Lose("Смена не закрыта: вернитесь к обогревателю жилого отсека в течение минуты после сирены.");}else if(!arrival||eventTimer<=0){Notify("Смена окончена. Вернитесь в жилой отсек к обогревателю. Осталось "+Mathf.CeilToInt(config.shiftSeconds+60-clock)+" с.");eventTimer=8;}}
 eventTimer-=dt;
 }
 void Lose(string reason){lossReason=reason;mode=Mode.Lost;panel="";}
 public void Drop(){if(carried=="")return; if(carried=="Инструмент"){carried="";return;}var go=new GameObject("Dropped "+carried);var d=go.AddComponent<StationDevice>();d.kind=carried=="Канистра"?DeviceKind.Fuel:DeviceKind.Fuse;d.title=carried;d.floor=floor;d.transform.position=new Vector3(actor.x,actor.y,0);devices.Add(d);StationView.DrawDevice(d);carried="";}
 void FinishShift(){state.shift++;state.temperature=Mathf.Max(55,state.temperature);state.fuel=Mathf.Max(45,state.fuel);state.flares=Mathf.Min(4,state.flares+1);Save();BeginShift();}
 void OnGUI(){StationUI.Draw(this);}
 public void DrawUI(){
 GUI.skin.font=Resources.Load<Font>("StationFont");GUI.skin.label.fontSize=16;GUI.skin.button.fontSize=16;GUI.skin.box.fontSize=16;
 float scale=Mathf.Min(Screen.width/1280f,Screen.height/800f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-800*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
 if(mode==Mode.Playing){
 GUI.Box(new Rect(16,12,1248,76),"");GUI.Label(new Rect(30,20,1220,25),"NORTHERNLAB  /  СМЕНА "+state.shift+" : "+config.shifts[state.shift-1].title+"     "+Mathf.FloorToInt(clock/60).ToString("00")+":"+Mathf.FloorToInt(clock%60).ToString("00")+"     ТОПЛИВО "+state.fuel.ToString("0")+"     ДЕТАЛИ "+state.parts+"     ТЕПЛО "+state.temperature.ToString("0")+"%");
 GUI.Label(new Rect(30,49,1200,30),config.shifts[state.shift-1].task+"   [ESC — пауза]");
 GUI.Box(new Rect(16,710,1248,78),"");GUI.Label(new Rect(30,719,1200,26),messageTime>0?message:"ЛКМ — подойти и взаимодействовать · колесо — масштаб · СКМ — камера · Home — общий вид");
 if(GUI.Button(new Rect(30,750,260,28),"В руках: "+(carried==""?"ничего":carried)+" ↓"))Drop();
 GUI.Label(new Rect(310,752,920,25),"РАДИО "+state.radio+"/3  · SOS "+(100*state.sos/config.sosSeconds).ToString("0")+"%  · "+(shutter?"ЗАСЛОНКА ЗАКРЫТА":"ЗАСЛОНКА ОТКРЫТА")+(warning>0?"  ⚠ СТУК СНИЗУ":""));
 if(panel!="")DrawPanel();return;
 }
 GUI.Box(new Rect(290,130,700,540),"");GUILayout.BeginArea(new Rect(320,155,640,495));
 GUILayout.Label("NORTHERNLAB: СМЕНА НЕ ОКОНЧЕНА",GUILayout.Height(42));
 switch(mode){
 case Mode.Menu: GUILayout.Label("Семь смен. Восемь помещений. Два сотрудника по документам.",GUILayout.Height(65));if(B("Новая игра"))NewGame();GUI.enabled=HasSave;if(B("Продолжить"))Continue();GUI.enabled=true;if(B("Настройки")){settingsFromPause=false;mode=Mode.Settings;}if(B("Выход"))Application.Quit();break;
 case Mode.Briefing:GUILayout.Label("СМЕНА "+state.shift+" / "+config.shifts[state.shift-1].title);GUILayout.Space(20);GUILayout.Label(config.shifts[state.shift-1].briefing,GUILayout.Height(140));GUILayout.Label(config.shifts[state.shift-1].task,GUILayout.Height(80));if(B("Принять смену"))mode=Mode.Playing;break;
 case Mode.Paused:GUILayout.Label("ПАУЗА");if(B("Вернуться"))mode=Mode.Playing;if(B("Настройки")){settingsFromPause=true;mode=Mode.Settings;}if(B("Повторить смену с контрольной точки"))Continue();if(B("Главное меню"))mode=Mode.Menu;break;
 case Mode.Settings:GUILayout.Label("Громкость: "+Mathf.RoundToInt(volume*100)+"%");volume=GUILayout.HorizontalSlider(volume,0,1);AudioListener.volume=volume;windowed=GUILayout.Toggle(windowed,"Оконный режим");if(B("Применить")){Screen.fullScreen=!windowed;PlayerPrefs.SetFloat("volume",volume);PlayerPrefs.SetInt("windowed",windowed?1:0);PlayerPrefs.Save();mode=settingsFromPause?Mode.Paused:Mode.Menu;}break;
 case Mode.Report:GUILayout.Label("ОТЧЁТ / СМЕНА "+state.shift,GUILayout.Height(35));GUILayout.Label("Топливо: "+(initialFuel-state.fuel).ToString("0")+" израсходовано (с учётом заправок)\nЗапчасти: "+state.parts+" (было "+initialParts+") · Радио: "+state.radio+"/3\nСведений: "+state.records+" · Растение: "+(state.watered?"стабильно":"ожидает воду"),GUILayout.Height(100));
 string[] upgrades={"Утепление жилья — быстрее согревает","Экономичный генератор — расход −30%","Инструменты — ремонт вдвое быстрее","Аккумулятор — +1 мощность"};for(int i=0;i<4;i++){GUI.enabled=state.parts>=3&&(state.upgrades&(1<<i))==0; if(B(upgrades[i]+" [3 детали]")){state.parts-=3;state.upgrades|=1<<i;reportChoice++;}GUI.enabled=true;}if(B("Следующая смена · сохранить"))FinishShift();break;
 case Mode.Lost:GUILayout.Label("СМЕНА ПРЕРВАНА",GUILayout.Height(40));GUILayout.Label(lossReason,GUILayout.Height(150));if(B("Повторить текущую смену"))Continue();if(B("Главное меню"))mode=Mode.Menu;break;
 case Mode.Won:GUILayout.Label("СМЕНА ОКОНЧЕНА",GUILayout.Height(45));GUILayout.Label("Прожекторы эвакуационной машины пробиваются сквозь снег.\n\nСигнал передан. Журнал содержит "+state.records+" записей.\nРастение "+(state.watered?"едет с вами.":"остаётся дежурить.")+"\n\nДиспетчер: «Перед уходом выключите свет.\nВторой сотрудник смену ещё не сдал». ",GUILayout.Height(230));if(B("Ещё одна экспедиция"))NewGame();if(B("Главное меню"))mode=Mode.Menu;break;
 }GUILayout.EndArea();
 }
 bool B(string s){return GUILayout.Button(s,GUILayout.Height(40));}
 void DrawPanel(){GUI.Box(new Rect(360,180,560,480),"");GUILayout.BeginArea(new Rect(390,200,500,430));
 if(panel=="board"){GUILayout.Label("РАСПРЕДЕЛИТЕЛЬНЫЙ ЩИТ  ·  "+Load+" / "+Supply+" кВт");string[] names={"Отопление","Освещение","Биосдерживание","Радиосвязь"};for(int i=0;i<4;i++){bool v=GUILayout.Toggle(power[i],names[i]+"   "+(config.demand[i]+(i==0&&state.shift==3?1:0))+" кВт");if(v!=power[i]){power[i]=v;StationAudio.Beep(350,.07f);}}GUILayout.Label(Load>Supply?"ПЕРЕГРУЗКА: системы обесточены. Снизьте нагрузку.":"Питание стабильно.");GUILayout.Label(state.fuse?"Предохранитель установлен.":"Для ремонта радио вставьте предохранитель из мастерской.");}
 if(panel=="generator"){GUILayout.Label("ГЕНЕРАТОР / НАГРЕВ "+heat.ToString("0")+"%");boosted=GUILayout.Toggle(boosted,"Форсаж: 8 кВт, двойной расход, шум и нагрев");GUILayout.Label("До перегрева отключите форсаж. Топливо: "+state.fuel.ToString("0"));}
 if(panel=="fuse"){GUILayout.Label("ПРЕДОХРАНИТЕЛЬ F-3 · выберите разъём на 3 А");foreach(int amps in new[]{5,3,10})if(B("Разъём "+amps+" А")){if(amps==3){state.fuse=true;carried="";panel="board";Notify("Цепь передатчика восстановлена.");}else{Notify("Номинал не совпадает. Предохранитель остался цел.");StationAudio.Beep(100,.2f);}}}
 if(panel=="repair"||panel=="repairRadio"){GUILayout.Label("РЕМОНТ / удерживайте кнопку");bool held=GUILayout.RepeatButton("Ремонтировать",GUILayout.Height(90));repairing=held;if(held){repairTick+=Time.deltaTime*((state.upgrades&4)!=0?2:1);if(Time.time% .2f<Time.deltaTime)StationAudio.Beep(700,.02f);}GUILayout.Label("Прогресс: "+Mathf.Min(100,100*repairTick/config.repairSeconds).ToString("0")+"%");if(repairTick>=config.repairSeconds){if(panel=="repairRadio"){state.parts-=2;state.radio++;Notify("Передатчик: этап "+state.radio+"/3 восстановлен.");}else{broken=false;heat=20;Notify("Генератор исправен.");}repairing=false;panel="";}}
 if(panel=="rest"){GUILayout.Label("ЖИЛОЙ ОТСЕК");GUILayout.Label(Powered(0)?"Обогреватель работает. Вы согреваетесь.":"Обогреватель холодный. Включите отопление на щите.");GUILayout.Label("Тепло: "+state.temperature.ToString("0")+"%\nВ конце смены вернитесь сюда для отчёта.");}
 if(panel=="radio"){GUILayout.Label("РАДИО / настройте частоту по сигналу");signalFrequency=GUILayout.HorizontalSlider(signalFrequency,88,108);float strength=Mathf.Clamp01(1-Mathf.Abs(signalFrequency-targetFrequency)/3);GUILayout.Label(signalFrequency.ToString("0.0")+" МГц   СИГНАЛ "+(strength*100).ToString("0")+"%");if(B("Зафиксировать частоту")){radioTuned=strength>.92f;Notify(radioTuned?"Частота захвачена.":"Слабый сигнал. Ищите максимум между 88 и 108 МГц.");}GUILayout.Label(state.shift<7?"Окно эвакуационной связи откроется в седьмую смену.":"SOS: "+(state.sos/config.sosSeconds*100).ToString("0")+"%\nДля передачи оставьте панель открытой и включите радиосвязь.\nПрогресс сохраняется при прерывании.");GUILayout.Label(Powered(3)?"Питание передатчика есть.":"Нет питания радиосвязи.");}
 GUILayout.FlexibleSpace();if(B("Закрыть")){panel="";repairing=false;}GUILayout.EndArea();}
}
public static class StationUI {public static void Draw(StationGame g){g.DrawUI();}}
}
