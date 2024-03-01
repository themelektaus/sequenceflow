using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Prototype.SequenceFlow.Editor
{
    public class SearchWindowProvider : UnityEngine.ScriptableObject, ISearchWindowProvider
    {
        static SearchWindowProvider instance;

        static Texture2D icon => Utils.CreateCircle(new(1, 1, 1, .3f), 32, 16);

        public static void Open(Vector3 mousePosition, Action<SequenceMethodDefinition> onSelectEntry)
        {
            var screenMousePosition = GUIUtility.GUIToScreenPoint(mousePosition);

            var context = new SearchWindowContext(screenMousePosition, 400);

            if (!instance)
                instance = CreateInstance<SearchWindowProvider>();

            instance.onSelectEntry = onSelectEntry;

            SearchWindow.Open(context, instance);
        }

        Action<SequenceMethodDefinition> onSelectEntry;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var hierarchy = new Dictionary<string, object>();

            var definitions = SequenceMethodDefinition.GetDefaultInstances();

            foreach (var definition in definitions)
                Prepare(hierarchy, definition.menuPath.Split('/'), definition);

            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Select Method"), 0),
                new(new GUIContent("(None)", icon)) { level = 1 }
            };

            Build(tree, hierarchy, hierarchy.Keys, 0);

            return tree;
        }

        void Prepare(Dictionary<string, object> hierarchy, string[] path, SequenceMethodDefinition definition)
        {
            var breadcrumb = hierarchy;

            for (int i = 0; i < path.Length; i++)
            {
                var item = path[i];

                if (i == path.Length - 1)
                {
                    breadcrumb[item] = definition;
                    continue;
                }

                if (!breadcrumb.ContainsKey(item))
                    breadcrumb[item] = new Dictionary<string, object>();

                breadcrumb = breadcrumb[item] as Dictionary<string, object>;
            }
        }

        void Build(List<SearchTreeEntry> tree, IDictionary hierarchy, ICollection keys, int level)
        {
            var _hierarchy = hierarchy as Dictionary<string, object>;
            var _keys = keys as IEnumerable<string>;

            int nextLevel = level + 1;

            foreach (var _key in _keys)
            {
                if (_hierarchy[_key] is IDictionary d)
                {
                    tree.Add(
                        new SearchTreeGroupEntry(
                            new GUIContent(_key), nextLevel
                        )
                    );

                    Build(tree, d, d.Keys, nextLevel);

                    continue;
                }

                if (_hierarchy[_key] is SequenceMethodDefinition definition)
                {
                    tree.Add(
                        new SearchTreeEntry(
                            new GUIContent(
                                definition.menuPath.Replace("/", " → "),
                                icon
                            )
                        )
                        {
                            level = nextLevel,
                            userData = definition
                        }
                    );
                }
            }
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            onSelectEntry(SearchTreeEntry.userData as SequenceMethodDefinition);
            return true;
        }
    }
}
