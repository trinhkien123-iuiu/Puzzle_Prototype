#if UNITY_2017_1_OR_NEWER
using NUnit.Framework;
using Puzzle.Application.Progress;
using Puzzle.Presentation.Persistence;
using Puzzle.Tests.Application;

namespace Puzzle.Tests.Presentation
{
    /// <summary>Kho PlayerPrefs chạy cùng bộ hợp đồng với kho giả trong bộ nhớ. Dùng khoá riêng và xoá sau mỗi test.</summary>
    public class PlayerPrefsProgressStoreTests : ProgressStoreContractTests
    {
        private const string TestKey = "puzzle.tests.progress";

        protected override IProgressStore CreateStore() => new PlayerPrefsProgressStore(TestKey);

        [SetUp]
        [TearDown]
        public void ClearTestKey() => new PlayerPrefsProgressStore(TestKey).Clear();
    }
}
#endif
