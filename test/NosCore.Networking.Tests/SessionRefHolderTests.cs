//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Networking.SessionRef;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class SessionRefHolderTests
    {
        [TestMethod]
        public void SessionIdsAreNeverTheNoSessionMarker()
        {
            var holder = new SessionRefHolder();

            var ids = Enumerable.Range(0, 10000).Select(_ => holder.GenerateSessionId()).ToList();

            Assert.IsTrue(ids.All(id => id > 0));
        }

        [TestMethod]
        public void TheNextSessionIdCannotBeWorkedOutFromTheLast()
        {
            var holder = new SessionRefHolder();

            var ids = Enumerable.Range(0, 1000).Select(_ => holder.GenerateSessionId()).ToList();
            var steps = ids.Zip(ids.Skip(1), (previous, next) => next - previous).Distinct().Count();

            Assert.IsTrue(steps > 900, $"only {steps} distinct steps between consecutive ids");
        }

        [TestMethod]
        public void SessionIdsDoNotRepeat()
        {
            var holder = new SessionRefHolder();

            var ids = Enumerable.Range(0, 200000).Select(_ => holder.GenerateSessionId()).ToList();

            Assert.AreEqual(ids.Count, ids.Distinct().Count());
        }

        [TestMethod]
        public void TwoServersDoNotHandOutTheSameSequence()
        {
            var first = new SessionRefHolder();
            var second = new SessionRefHolder();

            var firstIds = Enumerable.Range(0, 100).Select(_ => first.GenerateSessionId()).ToList();
            var secondIds = Enumerable.Range(0, 100).Select(_ => second.GenerateSessionId()).ToList();

            Assert.IsFalse(firstIds.SequenceEqual(secondIds));
        }

        [TestMethod]
        public void SessionIdsSpreadOverTheWholeRange()
        {
            var holder = new SessionRefHolder();

            var ids = Enumerable.Range(0, 1000).Select(_ => holder.GenerateSessionId()).ToList();

            Assert.IsTrue(ids.Max() > (1 << 30) / 2);
            Assert.IsTrue(ids.Min() < (1 << 30) / 2);
        }
    }
}
