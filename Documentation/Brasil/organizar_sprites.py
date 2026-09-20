from pathlib import Path
from collections import deque
import numpy as np
from PIL import Image
import json, hashlib
root=Path.cwd()
out=root/'Assets/Projeto/Sprites/Quest/Organizados'
out.mkdir(parents=True,exist_ok=True)
qa=root/'Documentation/Brasil/Validacao'
qa.mkdir(parents=True,exist_ok=True)
protected=[*Path('Assets/Projeto/Scenes').glob('*.unity'),Path('Assets/Projeto/Scripts/SceneLoader.cs'),Path('Packages/manifest.json'),*Path('ProjectSettings').glob('*.asset'),*Path('Assets/Projeto/Sprites').rglob('*.png')]
(qa/'antes.json').write_text(json.dumps({str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected},indent=2),encoding='utf-8')
report=[]
for kind,filename,cols,ppu in [('Walk','Sprite Sheet - Quest_Walk.png',8,400),('Idle','Sprite Sheet - Quest Idle.png',4,550)]:
 im=np.array(Image.open(root/'Assets/Projeto/Sprites/Quest/Sprite Sheets'/filename).convert('RGBA'))
 h,w=im.shape[:2]; solid=im[:,:,3]>32
 seen=np.zeros((h,w),bool); labels=np.zeros((h,w),np.int16); comps=[]
 for y,x in zip(*np.where(solid)):
  if seen[y,x]:continue
  q=[(int(y),int(x))];seen[y,x]=True;coords=[]
  while q:
   yy,xx=q.pop();coords.append((yy,xx))
   for ny,nx in [(yy-1,xx),(yy+1,xx),(yy,xx-1),(yy,xx+1)]:
    if 0<=ny<h and 0<=nx<w and solid[ny,nx] and not seen[ny,nx]:seen[ny,nx]=True;q.append((ny,nx))
  if len(coords)>500:
   arr=np.array(coords);comps.append(arr)
 assert len(comps)==cols*4,(kind,len(comps))
 comps.sort(key=lambda a:float(a[:,0].mean()))
 comps=[a for row in range(4) for a in sorted(comps[row*cols:(row+1)*cols],key=lambda a:float(a[:,1].mean()))]
 anchors=[]
 for i,a in enumerate(comps):
  labels[a[:,0],a[:,1]]=i+1
  bottom=int(a[:,0].max())+1
  feet=a[a[:,0]<int(a[:,0].min())+int((bottom-int(a[:,0].min()))*0.4)]
  anchor=int(round(float(np.median(feet[:,1]))))
  anchors.append((anchor,bottom))
 # Expand ownership from actual silhouettes, not rectangular cells. Every original
 # nontransparent pixel is copied exactly once, including antialiasing/fringe.
 for iteration in range(h+w):
  if np.all(labels):break
  updated=labels.copy()
  for dst,src in [((slice(1,None),slice(None)),(slice(None,-1),slice(None))),((slice(None,-1),slice(None)),(slice(1,None),slice(None))),((slice(None),slice(1,None)),(slice(None),slice(None,-1))),((slice(None),slice(None,-1)),(slice(None),slice(1,None)))]:
   d=updated[dst];s=labels[src];m=(d==0)&(s!=0);d[m]=s[m]
  labels=updated
 frame_data=[];left=right=top=below=0
 for i,(ax,ay) in enumerate(anchors):
  ys,xs=np.where((labels==i+1)&(im[:,:,3]>0))
  left=max(left,ax-int(xs.min()));right=max(right,int(xs.max())-ax+1)
  top=max(top,ay-int(ys.min()));below=max(below,int(ys.max())-ay+1)
  frame_data.append((ys,xs))
 left+=4;right+=4;top+=4;below+=4
 cw=left+right;ch=top+below
 atlas=np.zeros((ch*4,cw*cols,4),np.uint8)
 frames=[]
 for i,((ax,ay),(ys,xs)) in enumerate(zip(anchors,frame_data)):
  row,col=divmod(i,cols); dy=row*ch+top+ys-ay;dx=col*cw+left+xs-ax
  atlas[dy,dx]=im[ys,xs]
  assert np.array_equal(atlas[dy,dx],im[ys,xs])
  frames.append({'name':kind+['Down','Left','Right','Up'][row]+'_'+str(col+1).zfill(2),'x':col*cw,'y':(3-row)*ch,'width':cw,'height':ch,'pivotX':left/cw,'pivotY':below/ch})
 assert int((atlas[:,:,3]>0).sum())==int((im[:,:,3]>0).sum())
 Image.fromarray(atlas).save(out/('Quest'+kind+'.png'))
 (out/('Quest'+kind+'.json')).write_text(json.dumps({'pixelsPerUnit':ppu,'frames':frames},indent=2),encoding='utf-8')
 report.append({'sheet':kind,'frames':len(frames),'cell':[cw,ch],'atlas':[cw*cols,ch*4],'copiedNontransparentPixels':int((atlas[:,:,3]>0).sum()),'sourceAnchors':anchors,'pixelValuesUnchanged':True})
 # Contact sheet is only QA: compose over grey to inspect silhouette isolation.
 previews=Image.new('RGBA',(cols*220,4*240),(54,54,60,255))
 for i,((ax,ay),(ys,xs)) in enumerate(zip(anchors,frame_data)):
  frame=np.zeros((ch,cw,4),np.uint8);frame[top+ys-ay,left+xs-ax]=im[ys,xs]
  crop=Image.fromarray(frame).crop((left-140,top-290,left+150,top+12)) if kind=='Idle' else Image.fromarray(frame).crop((left-105,top-215,left+105,top+12))
  crop.thumbnail((210,230),Image.Resampling.NEAREST)
  previews.alpha_composite(crop,((i%cols)*220,(i//cols)*240))
 previews.convert('RGB').save(qa/(kind+'-recortes.png'))
(qa/'sprites.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
for name in ['Amazonas','Ponto Turisitico']:
 Image.open(root/'Assets/Projeto/Sprites/Brasil/Mapa'/(name+'.png')).convert('RGB').save(qa/(name+'.png'))
print(json.dumps(report,indent=2))