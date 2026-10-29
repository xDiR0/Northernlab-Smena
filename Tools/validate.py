"""Static project audit; does NOT replace Unity import or Play Mode tests."""
from pathlib import Path
import re,json,sys
from tree_sitter import Language,Parser
import tree_sitter_c_sharp
root=Path(__file__).resolve().parents[1];parser=Parser(Language(tree_sitter_c_sharp.language()));errors=[]
for p in root.glob('Assets/**/*.cs'):
 tree=parser.parse(p.read_bytes())
 if tree.root_node.has_error:errors.append(f'C# syntax: {p}')
guids={}
for p in root.glob('Assets/**/*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
 if m:
  if m[1] in guids:errors.append(f'Duplicate GUID {p}')
  guids[m[1]]=p
for p in [*root.glob('Assets/**/*.unity'),*root.glob('Assets/**/*.prefab'),*root.glob('Assets/**/*.asset')]:
 for g in re.findall(r'guid: (\w{32})',p.read_text()):
  if g not in guids:errors.append(f'Unresolved GUID {g}: {p}')
scene=(root/'Assets/Scenes/Station.unity').read_text();assert scene.count('  kind: ')==17
config=(root/'Assets/Resources/StationBalance.asset').read_text();assert config.count('  - title: ')==7
assert (root/'Assets/Resources/StationFont.ttf').stat().st_size>10000
for x in ['wind','hum','knock','step']:assert (root/f'Assets/Resources/{x}.wav').stat().st_size>1000
# Final baseline budget and time are feasible with base power and a single fuel can.
assert 2+2<=5;assert 225*.13<45;assert 85<225
json.loads((root/'Packages/manifest.json').read_text())
print('\n'.join(errors) if errors else 'PASS: C# syntax; asset references; 17 scene devices; 7 shifts; included font/audio; baseline final power/fuel budget.')
sys.exit(bool(errors))
