using System.Collections.Generic;
using JTLStudio.SDK.Localization.Editor;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Localization.Tests
{
    public class LocalizationTests
    {
        private LocalizationTable _table;

        [SetUp]
        public void SetUp()
        {
            _table = ScriptableObject.CreateInstance<LocalizationTable>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_table);
        }

        [Test]
        public void TableReturnsTheTranslationOfTheLanguage()
        {
            LocalizationEntry entry = _table.Add("menu.play");
            entry.Set(Language.English, "Play");
            entry.Set(Language.Russian, "Играть");

            Assert.IsTrue(_table.TryGet("menu.play", Language.Russian, out string text));
            Assert.AreEqual("Играть", text);
        }

        [Test]
        public void EmptyTranslationCountsAsMissing()
        {
            LocalizationEntry entry = _table.Add("menu.play");
            entry.Set(Language.English, "Play");
            entry.Set(Language.Russian, "");

            Assert.IsFalse(_table.TryGet("menu.play", Language.Russian, out string _));
        }

        [Test]
        public void KeyIsAddedOnceAndRenamed()
        {
            _table.Add("menu.play");
            _table.Add("menu.play");
            _table.Rename("menu.play", "menu.start");

            Assert.AreEqual(1, _table.Entries.Count);
            Assert.IsTrue(_table.Contains("menu.start"));
            Assert.IsFalse(_table.Contains("menu.play"));
        }

        [Test]
        public void CsvKeepsTranslationsWithCommasAndQuotes()
        {
            LocalizationEntry entry = _table.Add("dialog.hello");
            entry.Set(Language.English, "Hello, \"friend\"");
            entry.Set(Language.Russian, "Привет,\nдруг");

            List<Language> languages = new List<Language> { Language.English, Language.Russian };
            string csv = LocalizationCsv.Export(_table, languages);

            LocalizationTable imported = ScriptableObject.CreateInstance<LocalizationTable>();
            int changed = LocalizationCsv.Import(imported, csv);

            Assert.AreEqual(2, changed);
            Assert.IsTrue(imported.TryGet("dialog.hello", Language.English, out string english));
            Assert.AreEqual("Hello, \"friend\"", english);
            Assert.IsTrue(imported.TryGet("dialog.hello", Language.Russian, out string russian));
            Assert.AreEqual("Привет,\nдруг", russian);
            Object.DestroyImmediate(imported);
        }

        [Test]
        public void CsvSkipsColumnsThatAreNotLanguages()
        {
            LocalizationTable imported = ScriptableObject.CreateInstance<LocalizationTable>();
            LocalizationCsv.Import(imported, "key,English,Comment\nmenu.play,Play,for the translator\n");

            Assert.IsTrue(imported.TryGet("menu.play", Language.English, out string text));
            Assert.AreEqual("Play", text);
            Object.DestroyImmediate(imported);
        }

        [Test]
        public void TranslationFallsBackToTheDefaultLanguage()
        {
            LocalizationEntry entry = _table.Add("menu.play");
            entry.Set(Language.English, "Play");

            Assert.AreEqual("Play", Localization.Translate(_table, "menu.play", Language.Turkish));
        }

        [Test]
        public void SearchLooksAtKeysAndTranslations()
        {
            LocalizationEntry entry = _table.Add("menu.play");
            entry.Set(Language.English, "Play now");
            entry.Set(Language.Russian, "Играть");

            List<Language> languages = new List<Language> { Language.English, Language.Russian };

            Assert.IsTrue(LocalizationWindow.Matches(entry, "", languages));
            Assert.IsTrue(LocalizationWindow.Matches(entry, "MENU", languages));
            Assert.IsTrue(LocalizationWindow.Matches(entry, "now", languages));
            Assert.IsTrue(LocalizationWindow.Matches(entry, "играть", languages));
            Assert.IsFalse(LocalizationWindow.Matches(entry, "settings", languages));
        }

        [Test]
        public void SuggestedKeyIsMadeOfTheText()
        {
            GameObject host = new GameObject("Play Button");

            Assert.AreEqual("play.now", SceneTextScanner.SuggestKey(host, "Play now!"));
            Assert.AreEqual("play.button", SceneTextScanner.SuggestKey(host, "Играть"));
            Object.DestroyImmediate(host);
        }

        [Test]
        public void ComponentTakesTheTranslationFromItsTable()
        {
            LocalizationEntry entry = _table.Add("menu.play");
            entry.Set(Language.English, "Play");
            entry.Set(Language.Russian, "Играть");

            GameObject host = new GameObject("Text");
            LocalizedText component = host.AddComponent<LocalizedText>();
            component.Table = _table;
            component.Key = "menu.play";

            Assert.AreEqual("Play", component.Text(Language.English));
            Assert.AreEqual("Играть", component.Text(Language.Russian));
            Object.DestroyImmediate(host);
        }
    }
}
