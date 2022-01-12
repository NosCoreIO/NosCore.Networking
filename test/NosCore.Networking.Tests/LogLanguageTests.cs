using System;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Networking.Resource;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class LogLanguageTests
    {
        private readonly LogLanguageLocalizer<LogLanguageKey, LocalizedResources> _logLanguageLocalizer;

        public LogLanguageTests()
        {
            var factory =
                new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()),
                    new LoggerFactory());
            _logLanguageLocalizer = new LogLanguageLocalizer<LogLanguageKey, LocalizedResources>(
                new StringLocalizer<LocalizedResources>(factory));

        }

        [TestCategory("OPTIONAL-TEST")]
        [DataTestMethod]
        [DataRow(RegionType.EN)]
        [DataRow(RegionType.CS)]
        [DataRow(RegionType.DE)]
        [DataRow(RegionType.ES)]
        [DataRow(RegionType.FR)]
        [DataRow(RegionType.IT)]
        [DataRow(RegionType.PL)]
        [DataRow(RegionType.TR)]
        [DataRow(RegionType.RU)]
        public void CheckEveryLanguageValueSet(RegionType type)
        {
            CultureInfo.CurrentCulture = new CultureInfo(type.ToString());

            var result = string.Join(Environment.NewLine, I18NTestHelpers
                .GetKeysWithMissingTranslations(_logLanguageLocalizer)
                .Select(x => $"value {x} not defined"));

            if (result.Length != 0)
            {
                Assert.Fail(result);
            }
        }

        [DataTestMethod]
        [DataRow(RegionType.EN)]
        [DataRow(RegionType.CS)]
        [DataRow(RegionType.DE)]
        [DataRow(RegionType.ES)]
        [DataRow(RegionType.FR)]
        [DataRow(RegionType.IT)]
        [DataRow(RegionType.PL)]
        [DataRow(RegionType.TR)]
        [DataRow(RegionType.RU)]
        public void CheckEveryLanguageAreUsefull(RegionType type)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(type.ToString());

            var result = string.Join(Environment.NewLine,
                I18NTestHelpers.GetUselessTranslations(_logLanguageLocalizer, Enum.GetValues(typeof(LogLanguageKey))
                        .OfType<LogLanguageKey>().Select(s => s.ToString()).ToList())
                    .Select(x => $"key {x} is useless"));

            if (result.Length != 0)
            {
                Assert.Fail(result);
            }
        }


        [TestMethod]
        public void CheckLanguageUsage()
        {
            var result = string.Join(Environment.NewLine,
                I18NTestHelpers.GetUselessLanguageKeys<LogLanguageKey>().Cast<Enum>()
                    .Select(x => $"{x.GetType().Name} {x} is not used!"));

            if (result.Length != 0)
            {
                Assert.Fail(result);
            }
        }

    }
}