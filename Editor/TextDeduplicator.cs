using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class TextDeduplicator : EditorWindow
    {
        private string filePath = "";
        private bool showResult = false;
        private string originalText = "";
        private string processedText = "";
        private int originalCharCount = 0;
        private int processedCharCount = 0;

        [MenuItem("Tools/文本去重工具")]
        public static void ShowWindow()
        {
            GetWindow<TextDeduplicator>("文本去重工具");
        }

        void OnGUI()
        {
            GUILayout.Label("文本文件去重工具", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 文件路径输入
            GUILayout.BeginHorizontal();
            GUILayout.Label("文件路径:", GUILayout.Width(60));
            filePath = EditorGUILayout.TextField(filePath);
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("选择文本文件", "", "txt");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    filePath = selectedPath;
                    showResult = false; // 重置结果显示
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 处理按钮
            GUI.enabled = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
            if (GUILayout.Button("去除重复字符", GUILayout.Height(30)))
            {
                ProcessFile();
            }
            GUI.enabled = true;

            // 文件不存在提示
            if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
            {
                EditorGUILayout.HelpBox("文件不存在，请检查路径是否正确", MessageType.Warning);
            }

            // 显示处理结果
            if (showResult)
            {
                GUILayout.Space(20);
                GUILayout.Label("处理结果", EditorStyles.boldLabel);
            
                EditorGUILayout.HelpBox($"原文字符数: {originalCharCount}\n" +
                                        $"去重后字符数: {processedCharCount}\n" +
                                        $"去除重复字符: {originalCharCount - processedCharCount} 个", 
                    MessageType.Info);

                GUILayout.Space(10);

                // 原文本预览
                GUILayout.Label("原文本预览 (前200字符):", EditorStyles.boldLabel);
                string originalPreview = originalText.Length > 200 ? originalText.Substring(0, 200) + "..." : originalText;
                EditorGUILayout.TextArea(originalPreview, GUILayout.Height(80));

                GUILayout.Space(10);

                // 处理后文本预览
                GUILayout.Label("处理后预览 (前200字符):", EditorStyles.boldLabel);
                string processedPreview = processedText.Length > 200 ? processedText.Substring(0, 200) + "..." : processedText;
                EditorGUILayout.TextArea(processedPreview, GUILayout.Height(80));
            }
        }

        void ProcessFile()
        {
            try
            {
                // 读取文件内容
                originalText = File.ReadAllText(filePath, Encoding.UTF8);
                originalCharCount = originalText.Length;

                // 去除重复字符，保持首次出现顺序
                HashSet<char> seen = new HashSet<char>();
                StringBuilder result = new StringBuilder();

                foreach (char c in originalText)
                {
                    if (!seen.Contains(c))
                    {
                        seen.Add(c);
                        result.Append(c);
                    }
                }

                processedText = result.ToString();
                processedCharCount = processedText.Length;

                // 写回文件
                File.WriteAllText(filePath, processedText, Encoding.UTF8);

                showResult = true;
            
                Debug.Log($"文本去重完成！原文件: {originalCharCount} 字符，处理后: {processedCharCount} 字符");
                EditorUtility.DisplayDialog("处理完成", 
                    $"文本去重完成！\n原文件: {originalCharCount} 字符\n处理后: {processedCharCount} 字符\n已保存到原文件", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"处理文件时发生错误：\n{e.Message}", "确定");
                Debug.LogError($"文本去重出错: {e.Message}");
            }
        }
    }
}