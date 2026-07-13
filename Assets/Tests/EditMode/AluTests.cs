using NUnit.Framework;

public class AluTests
{
    [Test]
    public void Add_ReturnsSum()
    {
        var (result, zero) = Alu.Calculate(2, 3, AluOperation.ADD);

        Assert.AreEqual(5, result);
        Assert.IsFalse(zero);
    }

    [Test]
    public void Sub_WhenOperandsEqual_ZeroFlagIsSet()
    {
        var (result, zero) = Alu.Calculate(4, 4, AluOperation.SUB);

        Assert.AreEqual(0, result);
        Assert.IsTrue(zero);
    }

    [Test]
    public void And_ReturnsBitwiseAnd()
    {
        var (result, _) = Alu.Calculate(0b1100, 0b1010, AluOperation.AND);

        Assert.AreEqual(0b1000, result);
    }

    [Test]
    public void Or_ReturnsBitwiseOr()
    {
        var (result, _) = Alu.Calculate(0b1100, 0b0010, AluOperation.OR);

        Assert.AreEqual(0b1110, result);
    }

    [TestCase(0, 2, 3, 5)]              // ADD
    [TestCase(1, 5, 3, 2)]              // SUB
    [TestCase(2, 0b110, 0b011, 0b010)]  // AND
    [TestCase(3, 0b110, 0b011, 0b111)]  // OR
    public void IntOverload_MatchesEnumOverload(int opCode, int a, int b, int expected)
    {
        int result = Alu.Calculate(a, b, opCode);

        Assert.AreEqual(expected, result);
    }
}
