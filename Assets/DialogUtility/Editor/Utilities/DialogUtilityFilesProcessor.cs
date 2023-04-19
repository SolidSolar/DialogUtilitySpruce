using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DialogUtilitySpruce.Editor
{
    public class DialogUtilityFilesProcessor :  AssetModificationProcessor
    {
        private const string ContainersPath = "Assets/Resources/DialogUtility/Containers";
        private const string LocalisationPath = "Assets/Resources/DialogUtility/ContainerLocalisation/";
        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject as DialogGraphContainer != null)
            {
                DialogUtilityWindow.OpenDialogUtilityWindow(Selection.activeObject.name);
                Selection.activeObject = null;
                return true; //catch open file
            }

            return false; // let unity open the file
        }


        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (assetPath.Contains(ContainersPath)&&assetPath.Contains(".asset"))
            {
                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                DialogLanguageHandler.Instance?.DeleteAllRelatedLocalisation(fileName);
            }

            return AssetDeleteResult.DidNotDelete;
        }

        private static void OnWillCreateAsset(string assetName)
        {
            if (!assetName.Contains(".meta") && assetName.Contains(ContainersPath))
            {
                var container = AssetDatabase.LoadAssetAtPath<DialogGraphContainer>(assetName);
                if (container)
                {
                    DialogUtilityUsagesHandler.Instance.ProcessIfCopy(container);
                }
            }
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (sourcePath.Contains(ContainersPath) && sourcePath.Contains(".asset"))
            {
                string sourceDirectory = "Assets\\"+ Path.GetRelativePath(Path.GetFullPath("Assets"),(Path.GetDirectoryName(sourcePath)));
                string destinationDirectory = "Assets\\"+ Path.GetRelativePath( Path.GetFullPath("Assets"),(Path.GetDirectoryName(destinationPath)));
                if (destinationDirectory == sourceDirectory)
                {
                    var directories = Directory.GetDirectories(Path.GetFullPath(LocalisationPath));
                    foreach (var d in directories)
                    {
                        var path = "Assets\\"+ Path.GetRelativePath( Path.GetFullPath("Assets"), Path.GetFullPath(d)+ "\\" + Path.GetFileName(sourcePath));
                        if (File.Exists(path))
                        {
                            AssetDatabase.RenameAsset(path,
                                Path.GetFileName(destinationPath));
                        }
                    }
                }
            }
            return AssetMoveResult.DidNotMove;
        }
        
    }
}