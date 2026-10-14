using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NexaQuest.Brasil;
[InitializeOnLoad] public static class ValidacaoFruitMergeAmazonia {
 const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
 static ValidacaoFruitMergeAmazonia(){EditorApplication.playModeStateChanged+=s=>{
  if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("Merge.TestStart",false)){SessionState.SetBool("Merge.TestStart",false);new GameObject("VerificacoesFruitMerge").AddComponent<ChecksFruitMergeAmazonia>();}
  if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool("Merge.TestEnd",false)){
   SessionState.SetBool("Merge.TestEnd",false);SessionState.SetBool("NexaQuest.ValidandoQuiz",false);
   bool real=SessionState.GetBool("Merge.SaveExists",false)==PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia)&&SessionState.GetString("Merge.Save","")==PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,"");
   File.WriteAllText(MontagemFruitMergeAmazonia.Docs+"progresso-real.txt","Progresso real preservado: "+real);
   typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",Flags).Invoke(null,null);
   File.WriteAllText(MontagemFruitMergeAmazonia.Docs+"estado-editor.txt",SceneManager.GetActiveScene().path+"\nDirty: "+SceneManager.GetActiveScene().isDirty+"\nPlay Mode: "+EditorApplication.isPlaying);
   if(SessionState.GetBool("Merge.Passed",false))File.WriteAllText("Library/QuizAmazonia.solicitacao","testar");
  }
 };}
 public static void Executar(){
  if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Salve Brasil.");
  Directory.CreateDirectory(MontagemFruitMergeAmazonia.Docs);SessionState.SetBool("Merge.SaveExists",PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia));SessionState.SetString("Merge.Save",PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,""));
  typeof(QuestHUDVisualValidation).GetMethod("SetGameView",Flags).Invoke(null,null);typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",Flags).Invoke(null,null);
  SessionState.SetBool("NexaQuest.ValidandoQuiz",true);PlayerPrefs.SetString("NexaQuest.Testes.Quiz","{\"versao\":1,\"energia\":100,\"coins\":100,\"level\":1,\"currentXP\":0,\"possuiChaveVila\":true}");PlayerPrefs.Save();
  SessionState.SetBool("Merge.TestStart",true);SessionState.SetBool("Merge.TestEnd",true);EditorApplication.isPlaying=true;
 }
}
public class ChecksFruitMergeAmazonia:MonoBehaviour {
 readonly List<string> results=new List<string>(),logs=new List<string>();int failed;
 MinijogoAmazoniaController jogo;PainelDesafiosAmazonia painel;ProgressionManager p;
 string Docs=>MontagemFruitMergeAmazonia.Docs;
 void C(bool ok,string text){results.Add((ok?"PASS: ":"FAIL: ")+text);if(!ok)failed++;File.WriteAllLines(Docs+"andamento.txt",results);}
 void Log(string msg,string stack,LogType type){if(type!=LogType.Log)logs.Add(type+": "+msg);}
 void Energia(int n){var json=PlayerPrefs.GetString("NexaQuest.Testes.Quiz");json=System.Text.RegularExpressions.Regex.Replace(json,"\"energia\":\\s*\\d+","\"energia\":"+n);PlayerPrefs.SetString("NexaQuest.Testes.Quiz",json);p.CarregarProgresso();}
 IEnumerator Foto(string n){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Docs+n+"-1080p.png");yield return null;}
 Vector2 PontoDaArte(string panel,string button){var art=jogo.transform.Find(panel+"/Arte").GetComponent<Image>();Vector2 px=panel=="Instrucoes"?new Vector2(button=="Comecar"?470:900,1023):panel=="ObjetivoComChave"?new Vector2(button=="Continuar"?515:1030,822.5f):panel=="ObjetivoSemChave"?new Vector2(button=="Continuar"?650:1122.5f,712.5f):new Vector2(button=="JogarNovamente"?520:1040,822.5f);var rect=art.sprite.rect;return art.transform.TransformPoint(new Vector2((px.x-rect.center.x)*art.rectTransform.sizeDelta.x/rect.width,(art.sprite.texture.height-px.y-rect.center.y)*art.rectTransform.sizeDelta.y/rect.height));}
 IEnumerator Click(string panel,string button){var b=jogo.transform.Find(panel+"/Arte/"+button).GetComponent<Button>();yield return null;Canvas.ForceUpdateCanvases();var data=new PointerEventData(EventSystem.current){position=PontoDaArte(panel,button),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);C(hits.Count>0&&hits[0].gameObject==b.gameObject,"Raycast real: "+panel+"/"+button);if(hits.Count>0){ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler);}yield return null;}
 IEnumerator Abrir(){painel.Abrir();yield return null;painel.SelecionarMinijogo();yield return null;}
 AmazoniaMergeItem Item(int n,float x,float y)=>jogo.CriarItem(n,new Vector2(jogo.CentroX+x,y));
 IEnumerator Start(){
  Application.logMessageReceived+=Log;Application.runInBackground=true;yield return null;yield return null;
  jogo=FindObjectOfType<MinijogoAmazoniaController>(true);painel=FindObjectOfType<PainelDesafiosAmazonia>(true);p=ProgressionManager.Instance;
  var area=painel.area;var jogador=jogo.mundo.player;jogador.enabled=false;jogo.mundo.TravelTo(area,area.defaultSpawn);yield return new WaitForSecondsRealtime(1);
  C(Screen.width==1920&&Screen.height==1080,"Play Mode real 1920x1080");C(!p.MinijogoAmazoniaConcluido,"Save antigo inicia flag do minijogo false");
  var playerPos=jogador.transform.position;var camera=Camera.main;var cameraPos=camera.transform.position;var mask=camera.cullingMask;
  yield return Abrir();C(jogo.Estado==EstadoMergeAmazonia.Instrucoes&&!painel.EstaAberto&&p.Energia==100,"Minijogo abre instruções gratuitamente");C(jogador.ControlsLocked,"Player bloqueado na entrada");yield return Foto("Instrucoes");
  yield return Click("Instrucoes","Voltar");C(painel.EstaAberto&&p.Energia==100,"Voltar retorna aos desafios sem energia");painel.SelecionarMinijogo();yield return null;
  Energia(19);yield return Click("Instrucoes","Comecar");C(jogo.Estado==EstadoMergeAmazonia.Instrucoes&&p.Energia==19&&jogo.interfaceJogo.avisoEnergia.gameObject.activeSelf,"19 Energia bloqueia com aviso real");Energia(0);jogo.Comecar();C(p.Energia==0&&jogo.Estado==EstadoMergeAmazonia.Instrucoes,"Energia zero não fica negativa");yield return Foto("EnergiaInsuficiente");
  Energia(100);yield return Click("Instrucoes","Comecar");jogo.Comecar();C(jogo.EmPartida&&p.Energia==80,"Começar cobra exatamente 20 e impede duplo clique");C(Camera.main==camera&&camera.cullingMask==mask,"Câmera principal preservada");
  yield return new WaitForSecondsRealtime(1.7f);C(jogo.EmPartida&&jogo.perigo.TempoAtual==0,"Preview mantido no topo não ativa Game Over");
  foreach(var name in new[]{"HUDQuest","HUDProximo","HUDCoins","HUDObjetivos","BotaoSair"}){var r=jogo.interfaceJogo.hud.transform.Find(name).GetComponent<RectTransform>();var corners=new Vector3[4];r.GetWorldCorners(corners);C(corners.All(v=>v.x>=0&&v.x<=1920&&v.y>=0&&v.y<=1080),"HUD dentro de 1080p: "+name);}
  C(jogo.interfaceJogo.proximo.preserveAspect&&jogo.interfaceJogo.quest.preserveAspect,"Próximo e Quest preservam proporção");
  C(jogo.spawner.Preview!=null&&!jogo.spawner.Preview.Solto&&!jogo.spawner.Preview.Colisor.enabled,"Preview sem colisão");jogo.spawner.Mover(-99999);C(jogo.spawner.Preview.transform.position.x>=jogo.CentroX+jogo.Esquerda+jogo.spawner.Preview.Colisor.radius-.01f,"Preview respeita parede esquerda e raio");jogo.spawner.Mover(99999);C(jogo.spawner.Preview.transform.position.x<=jogo.CentroX+jogo.Direita-jogo.spawner.Preview.Colisor.radius+.01f,"Preview respeita parede direita e raio");
  int next=jogo.spawner.ItemProximo;C(jogo.interfaceJogo.proximo.sprite==jogo.evolucoes[next-1].sprite,"HUD Próximo usa sprite real correto");C(jogo.spawner.Soltar()&&!jogo.spawner.Soltar(),"Drop não duplica no mesmo frame");C(jogo.spawner.ItemAtual==next&&jogo.spawner.ItemProximo>=1&&jogo.spawner.ItemProximo<=3,"Próximo vira Atual e sorteia apenas 1..3");yield return new WaitForSecondsRealtime(.2f);C(jogo.spawner.Preview==null,"Cooldown impede preview sobre o item recém solto");yield return new WaitForSecondsRealtime(.4f);
  jogo.LimparItens();yield return null;
  int earlyCoins=p.Coins;var earlyA=Item(1,-.25f,-1);var earlyB=Item(1,.25f,-1);jogo.TentarFundir(earlyA,earlyB);C(jogo.interfaceJogo.coinsRodada.text=="1","HUD mostra Coin da rodada imediatamente");
  jogo.Sair();yield return null;C(p.Coins==earlyCoins+1&&p.CurrentXP==0&&!p.PossuiChaveAmazonia&&!p.MinijogoAmazoniaConcluido&&p.Energia==80&&painel.EstaAberto,"Sair antes da missão guarda Coins sem EXP, chave ou reembolso");
  painel.SelecionarMinijogo();yield return null;jogo.ProcessarSaida(true);yield return null;C(painel.EstaAberto&&p.Energia==80,"ESC nas instruções retorna gratuitamente");
  painel.SelecionarMinijogo();yield return null;jogo.Comecar();jogo.LimparItens();yield return null;
  for(int n=1;n<=3;n++){
   var a=Item(n,-4+n*2,1);a.Corpo.velocity=Vector2.down*20;yield return new WaitForSecondsRealtime(1);
   C(a!=null&&a.transform.position.y-a.Colisor.radius>=jogo.Chao-.04f,"Nível "+n+" cai sem atravessar chão em alta velocidade");C(a.Corpo.collisionDetectionMode==CollisionDetectionMode2D.Continuous,"Colisão contínua nível "+n);
  }
  C(jogo.Itens.Where(i=>i!=null&&i.Solto).All(i=>i.Corpo.bodyType==RigidbodyType2D.Dynamic),"Peças soltas são Dynamic");
  C(jogo.materialFisico.friction==.45f&&jogo.materialFisico.bounciness==.04f,"Atrito moderado e quique baixo");
  for(int i=0;i<8;i++){C(jogo.evolucoes[i].sprite!=null,"Sprite real evolução "+(i+1));if(i>0)C(jogo.evolucoes[i].diametro>jogo.evolucoes[i-1].diametro,"Diâmetro cresce na evolução "+(i+1));}
  C(jogo.cameraJogo.cullingMask==(1<<30),"Câmera do minijogo isolada na layer 30");
  var line=jogo.interfaceJogo.hud.transform.Find("LinhaPerigo").GetComponent<Image>();var lr=line.sprite.rect;var lp=line.transform.TransformPoint(new Vector2((1086-lr.center.x)*line.rectTransform.sizeDelta.x/lr.width,(line.sprite.texture.height-360-lr.center.y)*line.rectTransform.sizeDelta.y/lr.height));C(Mathf.Abs(lp.y-jogo.cameraJogo.WorldToScreenPoint(new Vector2(jogo.CentroX,jogo.LinhaY)).y)<1,"Faixa vermelha alinhada ao limite real da DangerZone");
  C(jogo.perigo.GetComponent<BoxCollider2D>().isTrigger,"DangerZone usa trigger real");
  yield return Foto("FisicaPilha");
  var left=Item(1,-5.3f,-2);left.Corpo.velocity=Vector2.left*20;var right=Item(2,5.2f,-2);right.Corpo.velocity=Vector2.right*20;yield return new WaitForSecondsRealtime(.5f);
  C(left.transform.position.x-left.Colisor.radius>=jogo.CentroX+jogo.Esquerda-.04f,"Peça não atravessa parede esquerda");C(right.transform.position.x+right.Colisor.radius<=jogo.CentroX+jogo.Direita+.04f,"Peça não atravessa parede direita");
  jogo.LimparItens();yield return null;
  var lower=Item(3,0,-2.85f);var falling=Item(1,0,1);falling.Corpo.velocity=Vector2.down*25;yield return new WaitForSecondsRealtime(.7f);
  C(falling!=null&&lower!=null&&Vector2.Distance(falling.transform.position,lower.transform.position)>=falling.Colisor.radius+lower.Colisor.radius-.05f,"Açaí rápido não atravessa Castanha");
  jogo.LimparItens();yield return null;
  jogo.LimparItens();yield return null;var pushing=Item(3,-1,-2.7f);var pushed=Item(2,0,-2.7f);pushing.Corpo.velocity=Vector2.right*4;float pushedX=pushed.transform.position.x;yield return new WaitForSecondsRealtime(.3f);C(pushed.transform.position.x>pushedX+.04f,"Peças empurram outras sem merge de níveis diferentes");
  jogo.LimparItens();yield return null;
  var rolling=Item(2,0,-2.8f);rolling.Corpo.angularVelocity=100;rolling.Corpo.velocity=Vector2.right*2;float startX=rolling.transform.position.x;yield return new WaitForSecondsRealtime(.25f);
  C(rolling.transform.position.x>startX+.1f&&Mathf.Abs(rolling.transform.eulerAngles.z)>1,"Peças rolam e se deslocam fisicamente");
  jogo.LimparItens();yield return null;int beforeTriple=p.Coins;
  Item(1,-.6f,-1);Item(1,0,-1);Item(1,.6f,-1);yield return new WaitForSecondsRealtime(.3f);
  C(p.Coins==beforeTriple+1&&jogo.Itens.Count(i=>i!=null&&i.Solto)==2,"Três iguais não duplicam merge ou recompensa");
  jogo.LimparItens();yield return null;int beforeChain=p.Coins;
  var chainA=Item(3,-.4f,-1);var chainB=Item(3,.4f,-1);Item(4,.95f,-1);jogo.TentarFundir(chainA,chainB);yield return new WaitForSecondsRealtime(.4f);
  C(jogo.Itens.Any(i=>i!=null&&i.Nivel==5)&&p.Coins==beforeChain+2,"Reação em cadeia real: Castanhas Cupuaçu Sapo, +2 Coins");
  C(jogo.CupuacuObjetivo&&!jogo.MissaoConcluidaNaRodada,"Cupuaçu criado permanece marcado depois de fundir, sozinho não conclui");
  C(jogo.Itens.Any(i=>i!=null&&i.name=="Sapo"),"Reação em cadeia não deixa peças consumidas na lista");
  C(jogo.interfaceJogo.comemoracao.Contains(jogo.interfaceJogo.quest.sprite),"Quest comemora após fusão em cadeia");
  C(jogo.interfaceJogo.popupCoin.gameObject.activeSelf,"Popup de Coin aparece no merge");yield return new WaitForSecondsRealtime(1.1f);C(!jogo.interfaceJogo.popupCoin.gameObject.activeSelf,"Popup faz fade e desaparece");
  C(jogo.interfaceJogo.idle.Contains(jogo.interfaceJogo.quest.sprite),"Quest retorna ao Idle depois da comemoração");
  jogo.LimparItens();yield return null;
  for(int n=1;n<=7;n++){
   var a=Item(n,-.3f,-1);var b=Item(n,.3f,-1);int before=p.Coins;yield return new WaitForSecondsRealtime(.16f);
   C(jogo.Itens.Any(i=>i!=null&&i.Nivel==n+1),"Colisão real: dois níveis "+n+" geram "+(n+1));C(p.Coins==before+1,"Colisão nível "+n+" recompensa exatamente +1");C(!jogo.TentarFundir(a,b),"Merge duplicado bloqueado nível "+n);
   if(n<7){jogo.LimparItens();yield return null;}
  }
  C(jogo.interfaceJogo.coinsRodada.text==jogo.CoinsDaRodada.ToString(),"CoinsDaRodada coincide com HUD depois das sete fusões");
  C(jogo.MissaoConcluidaNaRodada&&jogo.Estado==EstadoMergeAmazonia.ObjetivoConcluido,"Cupuaçu Arara Onça completam missão");C(p.CurrentXP==50&&p.MinijogoAmazoniaConcluido&&p.PossuiChaveAmazonia&&jogo.ChaveNova,"Primeira missão concede 50 EXP, flag e mesma chave verde");C(jogo.interfaceJogo.checks.All(c=>c.activeSelf),"Checks dos três objetivos persistem após fusões");C(jogo.interfaceJogo.painelComChave.activeSelf&&!jogo.interfaceJogo.painelSemChave.activeSelf,"Painel COM chave correto");
  yield return Foto("ObjetivoComChave");var snapshot=jogo.Itens.Select(i=>i.transform.position).ToArray();yield return new WaitForSecondsRealtime(.3f);C(snapshot.SequenceEqual(jogo.Itens.Select(i=>i.transform.position)),"Popup pausa física e mantém pilha");int round=jogo.CoinsDaRodada;
  yield return Click("ObjetivoComChave","Continuar");C(jogo.EmPartida&&jogo.CoinsDaRodada==round&&jogo.CupuacuObjetivo&&jogo.OncaObjetivo,"Continuar preserva rodada e objetivos");
  var jag1=Item(8,-2,-1);var jag2=Item(8,2,-1);C(!jogo.TentarFundir(jag1,jag2),"Duas Onças não criam nível 9 nem Coins");
  C(jogo.Itens.Where(i=>i!=null).All(i=>i.Corpo.simulated),"Continuar reativa corpos existentes");
  var c1=Item(1,-3,0);var c2=Item(1,-2.6f,0);jogo.TentarFundir(c1,c2);C(p.CurrentXP==50&&jogo.CoinsDaRodada==round+1,"Bônus continua dando Coins sem repetir EXP");yield return Foto("RodadaBonus");
  jogo.LimparItens();yield return null;var high=Item(3,0,2.65f);high.Corpo.gravityScale=0;high.Corpo.constraints=RigidbodyConstraints2D.FreezeAll;
  yield return new WaitForSecondsRealtime(.5f);C(jogo.EmPartida,"DangerZone não perde instantaneamente");high.transform.position=new Vector2(jogo.CentroX,-1);yield return new WaitForSecondsRealtime(.1f);C(jogo.perigo.TempoAtual==0,"Sair da DangerZone reseta timer");
  high.transform.position=new Vector2(jogo.CentroX,2.65f);yield return new WaitForSecondsRealtime(1.7f);C(jogo.Estado==EstadoMergeAmazonia.GameOver,"Permanecer cerca de 1,5 s causa Game Over");C(jogo.interfaceJogo.faixaRodadaEncerrada.activeInHierarchy&&p.CurrentXP==50,"Game Over pós missão diz RODADA ENCERRADA e mantém rewards");C(jogo.interfaceJogo.coinsFinal.text==jogo.CoinsDaRodada.ToString(),"Número dinâmico mostra CoinsDaRodada");yield return Foto("GameOverPosObjetivo");
  C(jogo.Itens.Where(i=>i!=null).All(i=>!i.Corpo.simulated),"Game Over pausa somente física do minijogo");
  int global=p.Coins;Energia(19);yield return Click("GameOver","JogarNovamente");C(jogo.Estado==EstadoMergeAmazonia.GameOver&&p.Energia==19,"Replay sem energia mantém estado seguro");Energia(80);yield return Click("GameOver","JogarNovamente");C(jogo.perigo.TempoAtual==0,"Replay zera DangerTimer");C(p.Energia==60&&p.Coins==global&&!jogo.MissaoConcluidaNaRodada&&jogo.CoinsDaRodada==0&&jogo.Itens.Count==1,"Replay custa 20 e limpa somente estado temporário");C(p.MinijogoAmazoniaConcluido&&p.PossuiChaveAmazonia&&p.CurrentXP==50,"Replay mantém progresso persistente");
  jogo.LimparItens();yield return null;
  foreach(int n in new[]{3,5,7}){var a=Item(n,-.3f,-1);var b=Item(n,.3f,-1);jogo.TentarFundir(a,b);if(n<7){jogo.LimparItens();yield return null;}}
  C(jogo.interfaceJogo.painelSemChave.activeSelf&&!jogo.ChaveNova&&p.Level==2&&p.CurrentXP==0,"Nova missão dá mais 50 EXP e painel SEM chave");yield return Foto("ObjetivoSemChave");
  jogo.Continuar();jogo.Sair();yield return null;C(painel.EstaAberto&&p.PossuiChaveAmazonia&&p.MinijogoAmazoniaConcluido&&p.Coins>=global,"Sair mantém Coins EXP flags e retorna desafios");
  // Chave vinda do Quiz usa a mesma flag e o mesmo save.
  PlayerPrefs.SetString("NexaQuest.Testes.Quiz","{\"versao\":1,\"energia\":100,\"coins\":0,\"level\":1,\"currentXP\":0,\"possuiChaveVila\":true,\"possuiChaveAmazonia\":true,\"quizAmazoniaConcluido\":true}");p.CarregarProgresso();painel.SelecionarMinijogo();yield return null;jogo.Comecar();
  foreach(int n in new[]{3,5,7}){var a=Item(n,-.3f,-1);var b=Item(n,.3f,-1);jogo.TentarFundir(a,b);if(n<7){jogo.LimparItens();yield return null;}}
  C(p.QuizAmazoniaConcluido&&p.MinijogoAmazoniaConcluido&&p.CurrentXP==50&&!jogo.ChaveNova&&jogo.interfaceJogo.painelSemChave.activeSelf,"Chave já ganha pelo Quiz: EXP sim, segunda chave não");string save=PlayerPrefs.GetString("NexaQuest.Testes.Quiz");p.CarregarProgresso();C(p.MinijogoAmazoniaConcluido&&p.PossuiChaveAmazonia&&PlayerPrefs.GetString("NexaQuest.Testes.Quiz")==save,"Save real v1 persiste progresso do minijogo");
  jogo.Sair();yield return null;painel.SelecionarMinijogo();yield return null;jogo.Comecar();jogo.LimparItens();yield return null;high=Item(2,0,2.65f);high.Corpo.gravityScale=0;high.Corpo.constraints=RigidbodyConstraints2D.FreezeAll;yield return new WaitForSecondsRealtime(1.7f);
  C(jogo.Estado==EstadoMergeAmazonia.GameOver&&!jogo.interfaceJogo.faixaRodadaEncerrada.activeSelf,"Game Over antes da missão mantém Objetivo Não Concluído");yield return Foto("GameOver");
  jogo.ProcessarSaida(true);yield return null;
  C(painel.EstaAberto,"Sair no Game Over retorna ao PainelDesafiosAmazonia");
  painel.SelecionarMinijogo();yield return null;int energiaAntesEsc=p.Energia;jogo.Sair();yield return null;
  C(p.Energia==energiaAntesEsc&&painel.EstaAberto,"Saída das instruções não reembolsa nem cobra energia");
  painel.Fechar();yield return null;C(!jogador.ControlsLocked&&Vector3.Distance(jogador.transform.position,playerPos)<.01f,"Sair e fechar desafios libera Player sem deslocar");C(jogo.interfaceJogo.avisoEnergia.transform.parent.name=="PainelDesafiosFeira","Aviso compartilhado devolvido");C(Camera.main==camera&&camera.cullingMask==mask,"Câmera original intacta ao sair");jogo.mundo.TravelTo(jogo.mundo.initialArea,jogo.mundo.initialSpawn);yield return new WaitForSecondsRealtime(1);jogador.enabled=true;
  C(logs.Count==0,"Console sem novos erros/avisos");results.AddRange(logs);results.Add("Falhas: "+failed);File.WriteAllLines(Docs+"fruit-merge-playmode.txt",results);Application.logMessageReceived-=Log;SessionState.SetBool("Merge.Passed",failed==0);EditorApplication.isPlaying=false;
 }
}
