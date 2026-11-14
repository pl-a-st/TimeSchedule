from pathlib import Path
import re, xml.etree.ElementTree as ET
root=Path(__file__).parents[1]
project=root/'PATCH'/'time schedule'/'time schedule.csproj'
codec=root/'PATCH'/'time schedule'/'CompactTaskCodec.cs'
store=root/'PATCH'/'time schedule'/'CompactTaskStore.cs'
tests=root/'TESTS'/'CompactStorePreviewTests.cs'
passes=[]
def check(ok,desc):
 passes.append((ok,desc))
 print(('PASS ' if ok else 'FAIL ')+desc)
for f in (project,codec,store,tests): check(f.is_file(),'file exists '+f.name)
ns={'m':'http://schemas.microsoft.com/developer/msbuild/2003'}
tree=ET.parse(str(project))
includes={el.attrib.get('Include') for el in tree.findall('.//m:Compile',ns)}
for name in ('TaskStore.cs','CompactTaskCodec.cs','CompactTaskStore.cs'):
 check(name in includes,'csproj compiles '+name)
for f in (codec,store):
 s=f.read_text(encoding='utf-8')
 check(s.count('{')==s.count('}'),'balanced braces '+f.name)
 check(s.count('/// <summary>')>=10,'xml summaries '+f.name)
 check('[Serializable]' not in s,'new codec types do not alter legacy binary serialization '+f.name)
check('GetNextNumForTask' not in codec.read_text(),'new codec preserves old task numbers')
check('File.Replace(temporary, destination' in store.read_text(),'manifest atomic replacement')
check('FileShare.ReadWrite | FileShare.Delete' in store.read_text(),'snapshot manifest readers permit replace')
check('Parallel.ForEach' in store.read_text(),'parallel segment loading')
check('new BinaryFormatter().Deserialize' in store.read_text() and 'new BinaryFormatter().Serialize' not in store.read_text(),'legacy source is read-only in new repository')
check('Copy-Item -LiteralPath $InputLegacyFile' in (root/'TESTS'/'Run-CompactPreview.ps1').read_text(),'PowerShell copies source before testing')
print('SUMMARY',sum(ok for ok,_ in passes),'/',len(passes),'PASS')
raise SystemExit(0 if all(ok for ok,_ in passes) else 1)
