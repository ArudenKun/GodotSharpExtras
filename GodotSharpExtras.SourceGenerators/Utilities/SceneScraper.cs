// #nullable disable
//
// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Diagnostics.CodeAnalysis;
// using System.IO;
// using System.Linq;
// using System.Text.RegularExpressions;
// using GodotSharpExtras.SourceGenerators.Extensions;
// using GodotSharpExtras.SourceGenerators.Models;
// using Microsoft.CodeAnalysis;
//
// namespace GodotSharpExtras.SourceGenerators.Utilities;
//
// [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
// [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
// [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
// internal static class SceneScraper
// {
//     private const string SectionRegexStr =
//         """^\[(?<Name>node|editable|ext_resource)( (?<Key>\S+?)=("(?<Value>.+?)"|(?<Value>.+?)))*]$""";
//
//     private const string ValueRegexStr =
//         """^(?<Key>script|unique_name_in_owner) = "?(?<Value>.+?)"?$""";
//
//     private const string ResIdRegexStr = """^ExtResource\([ "]?(?<Id>.+?)[ "]?\)$""";
//     private const string ResPathRegexStr = "^res:/(?<Path>.+?)$";
//
//     private static readonly Regex SectionRegex =
//         new(SectionRegexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
//
//     private static readonly Regex ValueRegex =
//         new(ValueRegexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
//
//     private static readonly Regex ResIdRegex =
//         new(ResIdRegexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
//
//     private static readonly Regex ResPathRegex =
//         new(ResPathRegexStr, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
//
//     private static string _resPath;
//     private static readonly ConcurrentDictionary<string, Tree<SceneTreeNode>> SceneTreeCache = [];
//
//     public static (Tree<SceneTreeNode> SceneTree, List<SceneTreeNode> UniqueNodes) GetNodes(
//         Compilation compilation,
//         string tscnFile,
//         bool traverseInstancedScenes
//     )
//     {
//         tscnFile = tscnFile.Replace("\\", "/");
//
//         var valueMatch = false;
//         SceneTreeNode curNode = null;
//         Tree<SceneTreeNode> sceneTree = null;
//         List<SceneTreeNode> uniqueNodes = [];
//         var resources = new Dictionary<string, string>();
//         var nodeLookup = new Dictionary<string, TreeNode<SceneTreeNode>>();
//
//         foreach (var line in File.ReadLines(tscnFile).Skip(2))
//         {
//             if (line is "")
//                 valueMatch = false;
//             else if (valueMatch)
//                 ValueMatch();
//             else
//                 SectionMatch();
//             continue;
//
//             void SectionMatch()
//             {
//                 var match = SectionRegex.Match(line);
//                 if (!match.Success)
//                     return;
//                 var name = match.Groups["Name"].Value;
//                 var values = match.Groups["Key"].ToDictionary(match.Groups["Value"]);
//
//                 switch (name)
//                 {
//                     case "node":
//                         NodeMatch();
//                         valueMatch = true;
//                         break;
//                     case "editable":
//                         EditableMatch();
//                         break;
//                     case "ext_resource":
//                         ExtResourceMatch();
//                         break;
//                 }
//
//                 return;
//
//                 void NodeMatch()
//                 {
//                     var nodeName = values.Get("name");
//                     var nodeType = values.Get("type");
//                     var parentPath = values.Get("parent");
//                     var resourceId = values.Get("instance");
//                     if (values.Get("instance_placeholder") is not null)
//                         nodeType = "InstancePlaceholder";
//                     else if (nodeType is not null)
//                         nodeType = compilation.ValidateTypeIgnoreCase(
//                             "GodotSharp",
//                             "Godot",
//                             values.Get("type")
//                         );
//                     if (resourceId is not null)
//                         resourceId = ResIdRegex.Match(resourceId).Groups["Id"].Value;
//
//                     var nodePath = GetNodePath();
//                     var safeNodeName = nodeName.Replace("-", "_").Replace(" ", "");
//
//                     AddNode(safeNodeName, nodePath);
//                     return;
//
//                     bool IsRootNode() => parentPath is null;
//
//                     bool IsChildNode() => parentPath is ".";
//
//                     string GetNodePath() =>
//                         IsRootNode() ? ""
//                         : IsChildNode() ? nodeName
//                         : $"{parentPath}/{nodeName}";
//
//                     bool HasResource() => resourceId is not null;
//
//                     string GetResource()
//                     {
//                         var resource = resources[resourceId];
//                         return GetResPath(resource) + resource;
//
//                         string GetResPath(string resourceResPath)
//                         {
//                             return _resPath is null || !tscnFile.StartsWith(_resPath)
//                                 ? _resPath = TryGetFromSceneCache() ?? TryGetFromFileSystem()
//                                 : _resPath;
//
//                             string TryGetFromSceneCache() =>
//                                 SceneTreeCache
//                                     .Keys.FirstOrDefault(x => x.EndsWith(resourceResPath))
//                                     ?[..^resourceResPath.Length];
//
//                             string TryGetFromFileSystem()
//                             {
//                                 const string godotProjectFile = "project.godot";
//                                 var tscnFolder = Path.GetDirectoryName(tscnFile);
//
//                                 while (tscnFolder is not null)
//                                 {
//                                     if (File.Exists($"{tscnFolder}/{godotProjectFile}"))
//                                         return tscnFolder;
//
//                                     tscnFolder = Path.GetDirectoryName(tscnFolder);
//                                 }
//
//                                 throw new Exception(
//                                     $"Could not find {godotProjectFile} in path {Path.GetDirectoryName(tscnFile)}"
//                                 );
//                             }
//                         }
//                     }
//
//                     void AddNode(string nodeNameAdd, string nodePathAdd)
//                     {
//                         if (IsRootNode())
//                         {
//                             if (HasResource())
//                                 AddInheritedScene();
//                             else
//                                 AddRootNode();
//                         }
//                         else
//                         {
//                             if (HasResource())
//                                 AddInstancedScene();
//                             else
//                                 AddChildNode();
//                         }
//
//                         return;
//
//                         void AddInheritedScene()
//                         {
//                             var resource = GetResource();
//                             var inheritedScene = GetCachedScene(resource);
//
//                             inheritedScene.Traverse(x =>
//                             {
//                                 if (x.IsRoot)
//                                 {
//                                     curNode = new SceneTreeNode(
//                                         nodeNameAdd,
//                                         x.Value.Type,
//                                         x.Value.Path
//                                     );
//                                     AddNode(curNode);
//                                 }
//                                 else
//                                 {
//                                     var node = new SceneTreeNode(
//                                         x.Value.Name,
//                                         x.Value.Type,
//                                         x.Value.Path
//                                     );
//                                     var parent = x.Parent.IsRoot ? "." : x.Parent.Value.Path;
//                                     AddNode(node, nodeLookup[parent]);
//                                 }
//                             });
//                         }
//
//                         void AddInstancedScene()
//                         {
//                             var resource = GetResource();
//                             var instancedScene = GetCachedScene(resource);
//
//                             instancedScene.Traverse(x =>
//                             {
//                                 if (x.IsRoot)
//                                 {
//                                     curNode = new SceneTreeNode(
//                                         nodeNameAdd,
//                                         x.Value.Type,
//                                         nodePathAdd
//                                     );
//                                     AddNode(curNode, nodeLookup[parentPath]);
//                                 }
//                                 else
//                                 {
//                                     var node = new SceneTreeNode(
//                                         x.Value.Name,
//                                         x.Value.Type,
//                                         $"{nodePathAdd}/{x.Value.Path}",
//                                         traverseInstancedScenes
//                                     );
//                                     var parent = x.Parent.IsRoot
//                                         ? nodePathAdd
//                                         : $"{nodePathAdd}/{x.Parent.Value.Path}";
//                                     AddNode(node, nodeLookup[parent]);
//                                 }
//                             });
//                         }
//
//                         void AddRootNode()
//                         {
//                             curNode = new SceneTreeNode(
//                                 nodeNameAdd,
//                                 $"Godot.{nodeType}",
//                                 nodePathAdd
//                             );
//                             Debug.Assert(parentPath is null);
//                             AddNode(curNode);
//                         }
//
//                         void AddChildNode()
//                         {
//                             var parent = nodeLookup.Get(parentPath);
//
//                             if (UnsupportedParent())
//                             {
//                                 curNode = null;
//                                 return;
//                             }
//
//                             if (ParentOverride())
//                             {
//                                 curNode = nodeLookup.Get(nodePathAdd)?.Value;
//
//                                 return;
//                             }
//
//                             curNode = new SceneTreeNode(
//                                 nodeNameAdd,
//                                 $"Godot.{nodeType}",
//                                 nodePathAdd
//                             );
//                             AddNode(curNode, parent);
//                             return;
//
//                             bool UnsupportedParent() => parent is null;
//
//                             bool ParentOverride() => nodeType is null;
//                         }
//
//                         void AddNode(SceneTreeNode node, TreeNode<SceneTreeNode> parent = null)
//                         {
//                             if (sceneTree is null) // Root
//                             {
//                                 Debug.Assert(parent is null);
//                                 nodeLookup.Add(".", sceneTree = new(node));
//                             }
//                             else
//                             {
//                                 Debug.Assert(parent is not null);
//                                 nodeLookup.Add(node.Path, parent.Add(node));
//                             }
//                         }
//
//                         Tree<SceneTreeNode> GetCachedScene(string resource)
//                         {
//                             if (!SceneTreeCache.TryGetValue(resource, out var scene))
//                             {
//                                 if (resource.EndsWith(".tscn"))
//                                 {
//                                     scene = GetNodes(
//                                         compilation,
//                                         resource,
//                                         traverseInstancedScenes
//                                     ).SceneTree;
//                                 }
//                                 else
//                                 {
//                                     scene = new(new(nodeNameAdd, "Godot.Node", ""));
//                                 }
//                             }
//
//                             return scene;
//                         }
//                     }
//                 }
//
//                 void EditableMatch()
//                 {
//                     var path = values.Get("path");
//                     var node = nodeLookup[path];
//                     node.Value.Visible = true;
//                 }
//
//                 void ExtResourceMatch()
//                 {
//                     var type = values.Get("type");
//                     if (type is "Script" or "PackedScene")
//                     {
//                         var id = values.Get("id");
//                         var path = values.Get("path");
//                         if (path is not null)
//                             path = ResPathRegex.Match(path).Groups["Path"].Value;
//                         resources.Add(id, path);
//                     }
//                 }
//             }
//
//             void ValueMatch()
//             {
//                 var match = ValueRegex.Match(line);
//                 if (!match.Success)
//                     return;
//                 var key = match.Groups["Key"].Value;
//                 var value = match.Groups["Value"].Value;
//
//                 switch (key)
//                 {
//                     case "script":
//                         ScriptMatch();
//                         break;
//                     case "unique_name_in_owner":
//                         UniqueNameMatch();
//                         break;
//                 }
//
//                 return;
//
//                 void ScriptMatch()
//                 {
//                     if (value is "null")
//                         return;
//                     if (curNode is null)
//                         return;
//                     var match = ResIdRegex.Match(value);
//                     if (!match.Success)
//                         return;
//                     var resourceId = match.Groups["Id"].Value;
//                     var resource = resources[resourceId];
//                     if (!resource.EndsWith(".cs"))
//                         return;
//                     var name = Path.GetFileNameWithoutExtension(resource);
//                     curNode.Type = compilation.GetFullName(name, resource)!;
//                     Debug.Assert(curNode.Type is not null);
//                 }
//
//                 void UniqueNameMatch()
//                 {
//                     if (curNode is null)
//                         return;
//                     if (bool.TryParse(value, out var result) && result)
//                     {
//                         uniqueNodes.Add(curNode);
//                     }
//                 }
//             }
//         }
//
//         SceneTreeCache[tscnFile] = sceneTree;
//         return (sceneTree, uniqueNodes);
//     }
// }
