using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Tests
{
    public class PackageMetaTests
    {
        private static readonly string[] Ignored = { ".gitignore", ".DS_Store" };

        [Test]
        public void EveryFileOfThePackageHasAMeta()
        {
            PackageInfo package = PackageInfo.FindForAssetPath("Packages/com.jtlstudio.sdk.localization");

            if (package == null)
            {
                Assert.Ignore("Пакет подключён не как пакет.");
            }

            List<string> missing = new List<string>();

            foreach (string path in Directory.GetFiles(package.resolvedPath, "*", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(path);

                if (name.EndsWith(".meta") || System.Array.IndexOf(Ignored, name) >= 0 || path.Contains("/.git/") || path.Contains("~/"))
                {
                    continue;
                }

                if (File.Exists(path + ".meta") == false)
                {
                    missing.Add(path.Substring(package.resolvedPath.Length + 1));
                }
            }

            Assert.IsEmpty(missing, "Без мета-файлов Unity игнорирует ассеты в git-пакете: " + string.Join(", ", missing));
        }
    }
}
