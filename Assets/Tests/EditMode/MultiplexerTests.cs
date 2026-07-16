using NUnit.Framework;

public class MultiplexerTests
{
    [Test]
    public void Select2To1_ControlTrue_ReturnsFirstInput()
    {
        var result = Multiplexer.Select2To1(1, 2, true);

        Assert.AreEqual(1, result);
    }

    [Test]
    public void Select2To1_ControlFalse_ReturnsSecondInput()
    {
        var result = Multiplexer.Select2To1(1, 2, false);

        Assert.AreEqual(2, result);
    }

    [TestCase(0, 10)]
    [TestCase(1, 20)]
    [TestCase(2, 30)]
    public void SelectNto1_ReturnsInputAtControlIndex(int control, int expected)
    {
        var inputs = new[] { 10, 20, 30 };

        var result = Multiplexer.SelectNto1(inputs, control);

        Assert.AreEqual(expected, result);
    }
}