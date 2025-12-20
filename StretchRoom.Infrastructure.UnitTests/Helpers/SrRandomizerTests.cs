using AwesomeAssertions;
using StretchRoom.Tests.Infrastructure;
using StretchRoom.Tests.Infrastructure.Helpers;

namespace StretchRoom.Infrastructure.UnitTests.Helpers;

public class SrRandomizerTests
{
    private const int Seed = 128439018;
    private SrRandomizer _randomizer;

    [SetUp]
    public void Setup()
    {
        _randomizer = new SrRandomizer(Seed);
    }

    [TestCase(10U)]
    public void When_RandomString_With_SpecifiedNumOfAttempts_Result_ExpectedLength(uint num)
    {
        var prevString = string.Empty;
        for (var i = 1U; i <= num; i++)
        {
            var str = _randomizer.String(i);
            str.Length.Should().Be((int)i);
            str.Should().NotBe(prevString);
            prevString = str;
        }
    }

    [TestCase(int.MinValue, int.MaxValue)]
    [TestCase(0, int.MaxValue)]
    [TestCase(1231, 181685)]
    public void When_RandomInt_With_SpecifiedLimits_Result_ValueInRange(int min, int max)
    {
        var val = _randomizer.Int(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(double.MinValue / 2, double.MaxValue / 2)]
    [TestCase(0.0, double.MaxValue)]
    [TestCase(1231.0, 181685.0)]
    public void When_RandomDouble_With_SpecifiedLimits_Result_ValueInRange(double min, double max)
    {
        var val = _randomizer.Double(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(long.MinValue, long.MaxValue)]
    [TestCase(0, long.MaxValue)]
    [TestCase(1231, 181685)]
    public void When_RandomLong_With_SpecifiedLimits_Result_ValueInRange(long min, long max)
    {
        var val = _randomizer.Long(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCase(uint.MinValue, uint.MaxValue)]
    [TestCase(0U, uint.MaxValue)]
    [TestCase(1231U, 181685U)]
    public void When_RandomUInt_With_SpecifiedLimits_Result_ValueInRange(uint min, uint max)
    {
        var val = _randomizer.UInt(min, max);
        val.Should()
            .BeGreaterThanOrEqualTo(min)
            .And
            .BeLessThanOrEqualTo(max);
    }

    [TestCaseSource(nameof(GetTestDates))]
    public void When_RandomDateTime_With_SpecifiedLimits_Result_ValueInRange(DateTime min, DateTime max)
    {
        var val = _randomizer.DateTime(min, max);
        val.Should()
            .BeOnOrAfter(min)
            .And
            .BeOnOrBefore(max);
    }

    private static IEnumerable<TestCaseData<DateTime, DateTime>> GetTestDates()
    {
        yield return new TestCaseData<DateTime, DateTime>(DateTime.UnixEpoch, DateTime.MaxValue);
        yield return new TestCaseData<DateTime, DateTime>(new DateTime(1999, 11, 23),
            new DateTime(2452, 6, 5, 10, 42, 12));
    }

    [TestCaseSource(nameof(GetTestOffsetDates))]
    public void When_RandomDateTimeOffset_With_SpecifiedLimits_Result_ValueInRange(DateTimeOffset min,
        DateTimeOffset max)
    {
        var val = _randomizer.DateTimeOffset(min, max);
        val.Should()
            .BeOnOrAfter(min)
            .And
            .BeOnOrBefore(max);
    }

    private static IEnumerable<TestCaseData<DateTimeOffset, DateTimeOffset>> GetTestOffsetDates()
    {
        yield return
            new TestCaseData<DateTimeOffset, DateTimeOffset>(DateTimeOffset.UnixEpoch, DateTimeOffset.MaxValue);
        yield return new TestCaseData<DateTimeOffset, DateTimeOffset>(new DateTime(1999, 11, 23),
            new DateTime(2452, 6, 5, 10, 42, 12));
    }

    [TestCase(50U)]
    public void When_RandomStringArray_With_SpecifiedNumOfAttempts_Result_ExpectedLength(uint num)
    {
        for (var i = 1U; i <= num; i++)
        {
            var array = _randomizer.StringArray(i);
            array.Length.Should().Be((int)i);
        }
    }

    [TestCase(10U, 64U)]
    public void When_RandomStringArray_WithSpecifiedLimits_Result_ValueInRange(uint arrayLength, uint maxStringLength)
    {
        var array = _randomizer.StringArray(arrayLength, maxStringLength);

        array.Length.Should().Be((int)arrayLength);
        array.Aggregate(true, (b, s) => b & (s.Length <= maxStringLength)).Should().BeTrue();
    }

    [TestCase(10U)]
    public void When_RandomIntArray_WithSpecifiedLimits_Result_ValueInRange(uint arrayLength)
    {
        var array = _randomizer.Array<int?>((_, rand) => rand.Int(), arrayLength);

        array.Length.Should().Be((int)arrayLength);
        array.Aggregate(true, (b, i) => b & (i != null)).Should().BeTrue();
    }

    [TestCase(10U, 5U)]
    [TestCase(10U, 11U)]
    public void When_RandomElementsOfCollection_With_SpecifiedCollectionLimits_Result_SpecifiedNumOfElementsWereTaken(
        uint length, uint toTake)
    {
        var randomArray = _randomizer.IntArray(length);

        var act = () => _randomizer.RandomElements(randomArray, toTake);
        if (toTake > length)
        {
            act.Should().Throw<ArgumentOutOfRangeException>();
            return;
        }

        var elements = act().ToArray();
        randomArray.Should().Contain(elements);
        elements.Should().HaveCount((int)toTake);
    }

    [TestCase(10U)]
    [TestCase(100U)]
    public void When_RandomElementOfCollection_With_SpecifiedCollectionLimits_Result_ElementFromCollection(uint length)
    {
        var randomArray = _randomizer.IntArray(length);

        var element = _randomizer.RandomElement(randomArray);

        randomArray.Should().Contain(element);
    }

    [TestCase(10)]
    [TestCase(100)]
    public void When_RandomEnumValue_With_SpecifiedNumOfIterations_Result_ValueIsInEnum(int numOfIterations)
    {
        for (var i = 0; i < numOfIterations; i++)
        {
            var val = _randomizer.Enum<TestEnum>();

            Enum.IsDefined(val).Should().BeTrue();
        }
    }

    [TestCase(5U)]
    public void When_RandomEnumValue_With_Result_ValueIsInEnum(uint numOfElementsToTake)
    {
        var enums = Enum.GetValues<TestEnum>();

        var val = _randomizer.Enums<TestEnum>(numOfElementsToTake);

        enums.Should().Contain(val);
    }
}

public enum TestEnum
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9
}