//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Net;
using Castle.Core.Logging;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NodaTime;
using NodaTime.Testing;
using NosCore.Networking.Encoding.Filter;
using NosCore.Networking.Resource;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class SpanRequestFilterTest
    {
        [TestMethod]
        public void FirstRequestNeverFilterOut()
        {
            var ctx = new Mock<IChannelHandlerContext>();
            ctx.SetupGet(x => x.Channel.RemoteAddress).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));
            var clock = new FakeClock(Instant.FromUtc(2022, 01, 01, 01, 01, 1));
            var spamFilter = new SpamRequestFilter(clock, new Mock<ILogger<SpamRequestFilter>>().Object, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object);
            var output = spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, output);
        }

        [TestMethod]
        public void DifferentIpFirstRequestNeverFilterOut()
        {
            var ctx = new Mock<IChannelHandlerContext>();
            ctx.SetupGet(x => x.Channel.RemoteAddress).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));

            var ctx2 = new Mock<IChannelHandlerContext>();
            ctx2.SetupGet(x => x.Channel.RemoteAddress).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.2"), 123));

            var clock = new FakeClock(Instant.FromUtc(2022, 01, 01, 01, 01, 1));
            var spamFilter = new SpamRequestFilter(clock, new Mock<ILogger<SpamRequestFilter>>().Object, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object);
            spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });
            var output = spamFilter.Filter(ctx2.Object, new byte[] { 1, 2, 3 });
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, output);
        }

        [TestMethod]
        public void SameIpIsFiltering()
        {
            var ctx = new Mock<IChannelHandlerContext>();
            ctx.SetupGet(x => x.Channel.RemoteAddress).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));

            var clock = new FakeClock(Instant.FromUtc(2022, 01, 01, 01, 01, 1));
            var spamFilter = new SpamRequestFilter(clock, new Mock<ILogger<SpamRequestFilter>>().Object, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object);
            spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });

            var output = spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });
            Assert.IsNull(output);
        }

        [TestMethod]
        public void SameIpAfterOneSecondIsNotFiltering()
        {
            var ctx = new Mock<IChannelHandlerContext>();
            ctx.SetupGet(x => x.Channel.RemoteAddress).Returns(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 123));

            var clock = new FakeClock(Instant.FromUtc(2022, 01, 01, 01, 01, 1));
            var spamFilter = new SpamRequestFilter(clock, new Mock<ILogger<SpamRequestFilter>>().Object, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object);
            spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });
            clock.AdvanceSeconds(1);
            var output = spamFilter.Filter(ctx.Object, new byte[] { 1, 2, 3 });
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, output);
        }
    }
}
