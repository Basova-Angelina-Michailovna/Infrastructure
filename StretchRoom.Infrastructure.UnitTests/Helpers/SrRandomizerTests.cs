using FluentAssertions;
using StretchRoom.Infrastructure.Helpers;

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
}