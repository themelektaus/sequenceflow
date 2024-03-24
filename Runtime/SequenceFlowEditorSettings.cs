using System;

#if UNITY_EDITOR
using System.IO;
using System.Runtime.CompilerServices;
#endif

using UnityEditor;

namespace Prototype.SequenceFlow
{
	public class SequenceFlowEditorSettings : UnityEngine.ScriptableObject
	{
        public static SequenceFlowEditorSettings instance { get; private set; }

#if UNITY_EDITOR
        public static void Load([CallerFilePath] string path = null)
        {
            path = Path.GetRelativePath(
                Environment.CurrentDirectory,
                new FileInfo(path).Directory.FullName
            );
            path = Path.Combine(path, $"{nameof(SequenceFlowEditorSettings)}.asset");

            instance = AssetDatabase.LoadAssetAtPath<SequenceFlowEditorSettings>(path);

            if (instance)
                return;

            instance = CreateInstance<SequenceFlowEditorSettings>();
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.Refresh();
        }
#endif

        public enum PortPosition { Top, Bottom }

		public PortPosition portPosition = PortPosition.Top;
	}
}
