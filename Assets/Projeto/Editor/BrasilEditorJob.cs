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
        else if (job == "portal") BrasilPortalSetup.Run();
        else if (job == "portal-test") BrasilPortalValidation.Run();
        else if (job == "npcs") FeiraNPCSetup.Run();
        else if (job == "npcs-test") FeiraNPCValidation.Run();
        else if (job == "npcs-links") FeiraNPCSetup.FixReferences();
        else if (job == "npcs-spawn") FeiraNPCSetup.CheckReturnSpawn();
    }
}
