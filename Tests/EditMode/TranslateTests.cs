using System.Collections.Generic;
using JTLStudio.SDK.Localization.Editor;
using NUnit.Framework;

namespace JTLStudio.SDK.Localization.Tests
{
    public class TranslateTests
    {
        private readonly TranslateService _service = new TranslateService();

        [Test]
        public void OfficialAnswerIsRead()
        {
            string response = "{\"data\":{\"translations\":[{\"translatedText\":\"Играть\"},{\"translatedText\":\"Настройки\"}]}}";

            List<string> translations = _service.Read(response, true, 2);

            Assert.AreEqual(2, translations.Count);
            Assert.AreEqual("Играть", translations[0]);
            Assert.AreEqual("Настройки", translations[1]);
        }

        [Test]
        public void FreeAnswerIsRead()
        {
            string response = "[[\"Играть\"],[\"Настройки\"]]";

            List<string> translations = _service.Read(response, false, 2);

            Assert.AreEqual(2, translations.Count);
            Assert.AreEqual("Играть", translations[0]);
            Assert.AreEqual("Настройки", translations[1]);
        }

        [Test]
        public void ExtraTranslationsAreDropped()
        {
            string response = "{\"data\":{\"translations\":[{\"translatedText\":\"Играть\"},{\"translatedText\":\"Лишнее\"}]}}";

            Assert.AreEqual(1, _service.Read(response, true, 1).Count);
        }

        [Test]
        public void RequestCarriesTheKeyAndLanguages()
        {
            List<string> texts = new List<string> { "Play now" };

            using (UnityEngine.Networking.UnityWebRequest request = _service.CreateRequest(texts, Language.English, Language.Russian, "secret"))
            {
                StringAssert.Contains("key=secret", request.url);
                StringAssert.Contains("source=en", request.url);
                StringAssert.Contains("target=ru", request.url);
                StringAssert.Contains("q=Play", request.url);
            }
        }

        [Test]
        public void RequestWithoutKeyGoesToTheFreeEndpoint()
        {
            List<string> texts = new List<string> { "Play" };

            using (UnityEngine.Networking.UnityWebRequest request = _service.CreateRequest(texts, Language.English, Language.Turkish, ""))
            {
                StringAssert.StartsWith(TranslateService.FreeEndpoint, request.url);
                StringAssert.Contains("tl=tr", request.url);
            }
        }
    }
}
