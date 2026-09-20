using System.IO;
using UnityEditor;

// Executa apenas solicitacoes locais explicitas de montagem/teste nesta sessao.
[InitializeOnLoad]
public static class BrasilEditorJob
{
    const string RequestPath = "Library/BrasilValidation.request";
    static BrasilEditorJob() { EditorApplication.update += CheckRequest; }
    static void CheckRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(RequestPath)) return;
        string job = File.ReadAllText(RequestPath).Trim();
        File.Delete(RequestPath);
        if (job == "build-and-test") { BrasilSetup.Build(); BrasilValidation.Run(); }
        else if (job == "test") BrasilValidation.Run();
    }
}