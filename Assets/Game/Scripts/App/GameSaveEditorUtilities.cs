#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game.App
{
    public static class GameSaveEditorUtilities
    {
        [MenuItem("Gameplay/Clear Game Save")]
        private static void ClearSaveFromEditor()
        {
            GameSaveService.ClearSave();
            Debug.Log("Game save file cleared.");
        }
    }
}
#endif