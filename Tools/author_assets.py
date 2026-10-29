"""Reproducible original pixel art, sound and Unity YAML authoring (Python + Pillow)."""
from pathlib import Path
import hashlib,json,random,math,wave,struct,shutil
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1]; A=ROOT/'Assets'; R=A/'Resources'; random.seed(1978)
def guid(path):return hashlib.md5(str(path).encode()).hexdigest()
def meta(path,extra=''):
 p=Path(path); p.with_name(p.name+'.meta').write_text('fileFormatVersion: 2\nguid: '+guid(p.relative_to(ROOT))+'\n'+extra)
# 24x32 sprites, at 16 px / world unit
palette={'ink':'#0b1824','metal':'#667e8c','light':'#b7ccd0','green':'#82cda0','orange':'#d8a052','red':'#cb5f55','blue':'#46708b'}
def sprite(name,draw):
 im=Image.new('RGBA',(32,40));d=ImageDraw.Draw(im);draw(d);p=R/(name+'.png');im.save(p)
 meta(p,'TextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  textureType: 8\n  spriteMode: 1\n  spritePixelsToUnits: 16\n  alphaIsTransparency: 1\n  spritePivot: {x: 0.5, y: 0.5}\n  spriteAlignment: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: '+guid(name)+'\n    internalID: 0\n')
def machine(d,typ):
 d.rectangle((3,7,29,36),fill=palette['ink']);d.rectangle((5,9,27,34),fill=palette['metal']);d.line((7,10,25,10),fill=palette['light'],width=2)
 if typ=='generator':
  d.rectangle((7,15,26,27),fill=palette['ink']);[d.line((x,16,x,25),fill=palette['orange']) for x in range(9,26,4)];d.rectangle((8,30,13,32),fill=palette['green']);d.rectangle((20,2,24,10),fill=palette['metal'])
 elif typ=='board':
  for y in [14,21,28]:
   for x in [9,20]:d.rectangle((x,y,x+3,y+3),fill=palette['green']);d.line((x+1,y+2,x+4,y-2),fill=palette['orange'])
 elif typ=='heater':
  for x in range(8,26,4):d.rectangle((x,14,x+2,30),fill=palette['orange'])
 elif typ=='shutter':
  for y in range(12,34,5):d.line((6,y,26,y),fill=palette['ink'],width=2)
 else:
  d.rectangle((8,13,24,25),fill=palette['ink']);d.line((9,20,13,17,16,22,20,16,23,19),fill=palette['green']);d.rectangle((10,29,23,31),fill=palette['light'])
for n in ['generator','board','heater','shutter','terminal']:sprite(n,lambda d,n=n:machine(d,n))
def tech(d,frame):
 d.rectangle((11,3,22,14),fill=palette['ink']);d.rectangle((12,4,21,12),fill=palette['orange']);d.rectangle((14,7,23,10),fill=palette['light']);d.rectangle((10,13,23,29),fill=palette['blue']);d.rectangle((8,16,11,27),fill=palette['orange']);d.rectangle((24,16 if frame!=3 else 10,27,26 if frame!=3 else 18),fill=palette['orange']);d.line((17,14,17,28),fill=palette['light']);
 d.rectangle((11,29,15,36 if frame!=1 else 33),fill=palette['ink']);d.rectangle((20,29,24,36 if frame!=2 else 33),fill=palette['ink']);d.rectangle((9,35 if frame!=1 else 32,16,38 if frame!=1 else 35),fill=palette['metal']);d.rectangle((20,35 if frame!=2 else 32,27,38 if frame!=2 else 35),fill=palette['metal'])
for i,n in enumerate(['tech_idle','tech_walk1','tech_walk2','tech_repair']):sprite(n,lambda d,i=i:tech(d,i))
sprite('canister',lambda d:(d.rectangle((9,12,24,32),fill=palette['orange']),d.rectangle((12,8,21,13),outline=palette['metal'],width=2),d.line((11,16,22,28),fill=palette['ink'],width=2),d.line((22,16,11,28),fill=palette['ink'],width=2)))
sprite('fuse',lambda d:(d.rectangle((12,15,20,29),fill=palette['light']),d.rectangle((11,12,21,16),fill=palette['orange']),d.rectangle((11,28,21,32),fill=palette['orange'])))
sprite('tools',lambda d:(d.line((10,29,23,14),fill=palette['metal'],width=5),d.arc((18,8,28,19),0,300,fill=palette['light'],width=4),d.ellipse((7,27,13,33),fill=palette['orange'])))
sprite('parts',lambda d:(d.rectangle((6,18,27,33),fill=palette['orange']),d.rectangle((9,21,24,24),fill=palette['ink']),d.line((12,10,12,20),fill=palette['metal'],width=3),d.line((22,12,22,20),fill=palette['light'],width=3)))
sprite('record',lambda d:(d.rectangle((9,12,25,33),fill='#c7c8b3'),[d.line((12,y,22,y),fill=palette['ink']) for y in range(16,30,4)]))
sprite('plant',lambda d:(d.polygon([(11,29),(23,29),(21,37),(13,37)],fill=palette['orange']),d.line((17,29,17,11),fill='#509e78',width=2),d.ellipse((7,12,16,19),fill=palette['green']),d.ellipse((18,18,26,24),fill='#509e78')))
sprite('fan',lambda d:(d.ellipse((6,10,26,30),outline=palette['metal'],width=2),d.rectangle((14,11,18,29),fill=palette['metal']),d.rectangle((7,18,25,22),fill=palette['metal']),d.ellipse((13,17,19,23),fill=palette['orange'])))
sprite('tank',lambda d:(d.rounded_rectangle((5,2,27,38),radius=5,fill=palette['metal']),d.rectangle((8,7,24,31),fill='#1b544a'),d.line((15,11,20,17,12,22,18,28),fill='#569779',width=3)))
sprite('flare',lambda d:(d.rectangle((13,18,18,34),fill=palette['red']),d.polygon([(15,4),(11,17),(20,17)],fill=palette['orange'])))
sprite('mug',lambda d:(d.rectangle((10,22,22,33),fill=palette['light']),d.arc((20,23,28,31),270,90,fill=palette['light'],width=2)))
sprite('oldradio',lambda d:(d.rectangle((6,20,27,33),fill=palette['orange']),d.rectangle((9,23,18,30),fill=palette['ink']),d.ellipse((21,25,25,29),fill=palette['light']),d.line((24,20,20,5),fill=palette['metal'])))
sprite('bunk',lambda d:(d.rectangle((2,18,30,30),fill=palette['metal']),d.rectangle((4,20,28,25),fill='#ad9c7c'),d.rectangle((3,29,6,38),fill=palette['ink']),d.rectangle((26,29,29,38),fill=palette['ink'])))
sprite('creature',lambda d:(d.polygon([(15,3),(22,9),(21,18),(26,24),(31,38),(26,38),(19,25),(18,37),(11,39),(10,24),(4,38),(1,36),(6,20),(10,15),(9,8)],fill='#050b13'),d.point((14,10),fill=palette['red']),d.point((18,10),fill=palette['red'])))
p=R/'pixel.png';Image.new('RGBA',(1,1),'white').save(p);meta(p,'TextureImporter:\n  serializedVersion: 13\n  textureType: 8\n  spriteMode: 1\n  spritePixelsToUnits: 1\n  alphaIsTransparency: 1\n  textureSettings:\n    filterMode: 0\n')
shutil.copy('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',R/'StationFont.ttf')
# Audio synthesized offline; no runtime service.
for name,seconds in [('wind',6),('hum',4),('step',.14),('knock',2)]:
 data=[];last=0
 for i in range(int(22050*seconds)):
  t=i/22050;noise=random.uniform(-1,1);last=.94*last+.06*noise
  if name=='wind':v=last*(.4+.2*math.sin(t*1.1))
  elif name=='hum':v=.1*math.sin(2*math.pi*55*t)+.035*math.sin(2*math.pi*110*t)
  elif name=='step':v=noise*math.exp(-t*35)*.4
  else:
   q=t% .55;v=(math.sin(2*math.pi*74*q)*.6+noise*.15)*math.exp(-q*28) if t<1.6 else 0
  data.append(struct.pack('<h',int(max(-1,min(1,v))*32767)))
 with wave.open(str(R/(name+'.wav')),'wb') as w:w.setparams((1,2,22050,0,'NONE','not compressed'));w.writeframes(b''.join(data))
# Stable script GUIDs.
for p in A.rglob('*.cs'):meta(p,'MonoImporter:\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n')
for p in [R/'StationFont.ttf',*R.glob('*.wav')]:meta(p)
shifts=[
('Прибытие','Предыдущая команда не сдала журнал. Возьмите канистру, заправьте генератор. Затем доставьте предохранитель из мастерской к щиту. ЛКМ по объекту прокладывает маршрут. Для предметов нужна свободная рука.','Запустите генератор. Установите предохранитель. К концу смены вернитесь к обогревателю.','Последняя запись: «Не отвечать на вызовы лифта после полуночи».',3,1),
('Второй сотрудник','Архив открыт. Заберите ящик запчастей и журнал. Для радио нужны инструмент, предохранитель и 2 детали за каждый из трёх этапов ремонта. Инструмент после работы можно положить кнопкой внизу.','Исследуйте архив. Начните ремонт передатчика.','Список экспедиции: восемь фамилий. Девятая вписана тем же почерком восемь раз.',4,1),
('Белая стена','Метель усилилась. Отопление требует дополнительный киловатт. В середине смены ожидается остановка генератора. Принесите инструмент заранее.','Переживите метель и почините генератор.','Метеосводка: ветер дует из шахты. Прибор проверен дважды.',3,1.6),
('Здоровый образец','Биолаборатория разблокирована. Без биосдерживания растёт опасность. Сеть тянет либо отопление с биосдерживанием, либо отопление, свет и радио. Переключайте питание у щита.','Изучите лабораторию. Следите за температурой и биосдерживанием.','Протокол: образец реагирует на шум. Свет для него необязателен.',3,1),
('Кто-то идёт','При стуке закройте заслонку у лифта. Фальшфейеры на нижней площадке отпугивают образец. Он слышит форсированный генератор. Не задерживайтесь внизу.','Подготовьте заслонку. Сохраните запас топлива.','Охрана: три удара перед входом. Он всё ещё соблюдает инструкцию.',3,1),
('Последние детали','Заберите детали со склада. Завершите три этапа ремонта передатчика. Генератор потребует ремонта. Не расходуйте все детали на улучшения.','Передатчик 3/3. Заготовьте топливо для последней смены.','Диспетчер запрашивает эвакуацию одного человека. Второй сотрудник отказался подниматься.',4,1),
('Смена не окончена','Окно связи открыто. Закройте заслонку, заправьте генератор. Включите отопление и радио: свет можно оставить аварийным. Настройте частоту по максимуму сигнала и держите панель открытой для SOS. Прерывание сохраняет прогресс.','Передайте SOS и удержите станцию до конца смены.','Исходящая радиограмма: «Мы здесь одни». В журнале исправлено: «Я».',3,1)]
config='''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: SCRIPT, type: 3}
  m_Name: StationBalance
  m_EditorClassIdentifier: 
  shiftSeconds: 225
  fuelCapacity: 110
  fuelPerSecond: 0.13
  coldPerSecond: 0.24
  warmthPerSecond: 0.45
  generatorPower: 5
  boostPower: 8
  demand: 02000000010000000300000002000000
  sosSeconds: 85
  repairSeconds: 3
  warningSeconds: 9
  shifts:
'''.replace('SCRIPT',guid(Path('Assets/Scripts/StationConfig.cs')))
for title,brief,task,record,parts,mult in shifts:
 config+='  - title: '+json.dumps(title,ensure_ascii=False)+'\n    briefing: '+json.dumps(brief,ensure_ascii=False)+'\n    task: '+json.dumps(task,ensure_ascii=False)+'\n    record: '+json.dumps(record,ensure_ascii=False)+f'\n    parts: {parts}\n    heatingMultiplier: {mult}\n'
p=R/'StationBalance.asset';p.write_text(config);meta(p,'NativeFormatImporter:\n  mainObjectFileID: 11400000\n')
# Saved scene, authored interactables; no inspector wiring required.
header='%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'
def entity(base,name,x,y,script,fields=''):
 return f'''--- !u!1 &{base}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {base+1}}}
  - component: {{fileID: {base+2}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_IsActive: 1
--- !u!4 &{base+1}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {x}, y: {y}, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
--- !u!114 &{base+2}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {base}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {guid(Path(script))}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{fields}'''
scene=header+entity(100,'NorthernLab',0,0,'Assets/Scripts/StationGame.cs','  config: {fileID: 11400000, guid: '+guid(Path('Assets/Resources/StationBalance.asset'))+', type: 2}\n')
# kind enum ordering matches C#.
devices=[('Генератор',1,-17,2,0,1),('Канистра',0,-14,2,0,1),('Щит',2,-11.5,2,0,1),('Обогреватель',3,-7,2,0,1),('Образец',12,-4,2,0,1),('Инструмент',4,3,2,0,1),('F-3',5,6,2,0,1),('Передатчик',6,15,2,0,1),('Запас топлива',0,-17,-5,1,1),('Детали',7,-13,-5,1,1),('Журнал',8,-7,-5,1,2),('Архив деталей',7,-4,-5,1,2),('Протокол',8,6,-5,1,4),('Лаб. запас',7,3,-5,1,4),('Заслонка',9,1.5,2,0,1),('Фальшфейер',13,18,-5,1,1),('Лифт',10,0,-5,1,1)]
for i,(title,kind,x,y,floor,unlock) in enumerate(devices):
 fields=f'  kind: {kind}\n  title: {json.dumps(title,ensure_ascii=False)}\n  floor: {floor}\n  unlockShift: {unlock}\n  room: {i}\n  consumed: 0\n'
 block=entity(200+i*10,title,x,y,'Assets/Scripts/StationDevice.cs',fields);scene+=block
 p=A/'Prefabs'/f'Device{i:02}.prefab';p.write_text(header+block);meta(p,'PrefabImporter:\n  externalObjects: {}\n')
p=A/'Scenes/Station.unity';p.write_text(scene);meta(p,'DefaultImporter:\n  externalObjects: {}\n')
(ROOT/'ProjectSettings/EditorBuildSettings.asset').write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!1045 &1\nEditorBuildSettings:\n  m_ObjectHideFlags: 0\n  serializedVersion: 2\n  m_Scenes:\n  - enabled: 1\n    path: Assets/Scenes/Station.unity\n    guid: '+guid(Path('Assets/Scenes/Station.unity'))+'\n  m_configObjects: {}\n')
print('Authored sprites, audio, scene, 17 device prefabs, 7-shift configuration.')
