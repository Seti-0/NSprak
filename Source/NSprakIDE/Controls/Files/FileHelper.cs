using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace NSprakIDE.Controls.Files
{
    public static class FileHelper
    {
        public static void AddNewFolder(string parentPath)
        {
            string newPath = GetUniquePath(Path.Combine(parentPath, "New Folder"));
            Directory.CreateDirectory(newPath);
        }

        public static void AddComputer(string parentPath)
        {
            string newPath = GetUniquePath(Path.Combine(parentPath, "New File.sprak"));
            File.WriteAllText(newPath, "");
        }

        public static void OpenInFileExplorer(string fullPath)
        {
            Process.Start("explorer", fullPath);
        }

        public static void Delete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            else if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }

        public static void Rename(string path, string newName)
        {
            string parent = Path.GetDirectoryName(path);
            string newPath = Path.Combine(parent, newName);

            if (path == newPath)
                return;

            if (File.Exists(path))
                File.Move(path, newPath);

            else if (Directory.Exists(path))
                Directory.Move(path, newPath);
        }

        public static void Move(string oldPath, string newParentPath)
        {
            string name = Path.GetFileName(oldPath);
            string newPath = Path.Combine(newParentPath, name);

            if (File.Exists(oldPath))
                File.Move(oldPath, newPath);

            else if (Directory.Exists(oldPath))
                Directory.Move(oldPath, newPath);
        }

        private static string GetUniquePath(string basePath)
        {
            // Does something like this exist in the system libraries?

            bool Exists(string path)
            {
                return Directory.Exists(path) || File.Exists(path);
            }

            if (!Exists(basePath))
                return basePath;

            string parent = Path.GetDirectoryName(basePath);
            string name = Path.GetFileNameWithoutExtension(basePath);
            string extension = Path.GetExtension(basePath);

            string result;
            int count = 1;

            do
            {
                result = Path.Combine(parent, $"{name} ({count}){extension}");
                count++;
            }
            while (Exists(result));

            return result;
        }
    }
}
