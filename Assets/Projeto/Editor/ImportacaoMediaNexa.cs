using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
public class ImportacaoMediaNexa:AssetPostprocessor {
 void OnPreprocessAudio(){if(!assetPath.StartsWith("Assets/Projeto/Audio/Polimento20261021/"))return;var i=(AudioImporter)assetImporter;var s=i.defaultSampleSettings;s.loadType=assetPath.Contains("/Music/")?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;s.compressionFormat=assetPath.Contains("/Music/")?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;s.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;s.quality=.8f;i.defaultSampleSettings=s;}
 void OnPreprocessAsset(){if(assetPath=="Assets/Projeto/Video/MenuFinal.mp4"&&assetImporter is VideoClipImporter i){var s=i.defaultTargetSettings;s.enableTranscoding=false;i.defaultTargetSettings=s;}}
}
