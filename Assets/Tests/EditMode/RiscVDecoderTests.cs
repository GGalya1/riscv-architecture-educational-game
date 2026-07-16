using NUnit.Framework;

public class RiscVDecoderTests
{
    // The helpers below build real RISC-V instruction words from their fields,
    // independently of RiscVDecoder, so the tests check decoding against the
    // ISA encoding rather than against the decoder's own arithmetic.

    private static uint BuildRType(uint rd, uint rs1, uint rs2, uint funct3, uint funct7)
        => (funct7 << 25) | (rs2 << 20) | (rs1 << 15) | (funct3 << 12) | (rd << 7) | 0x33;

    private static uint BuildIType(int imm, uint rs1, uint funct3, uint rd, uint opcode = 0x13)
    {
        uint imm12 = (uint)imm & 0xFFF;
        return (imm12 << 20) | (rs1 << 15) | (funct3 << 12) | (rd << 7) | opcode;
    }

    private static uint BuildSType(int imm, uint rs2, uint rs1, uint funct3)
    {
        uint imm12 = (uint)imm & 0xFFF;
        uint imm11_5 = (imm12 >> 5) & 0x7F;
        uint imm4_0 = imm12 & 0x1F;
        return (imm11_5 << 25) | (rs2 << 20) | (rs1 << 15) | (funct3 << 12) | (imm4_0 << 7) | 0x23;
    }

    private static uint BuildBType(int imm, uint rs2, uint rs1, uint funct3)
    {
        uint imm13 = (uint)imm & 0x1FFF;
        uint bit12 = (imm13 >> 12) & 0x1;
        uint bit11 = (imm13 >> 11) & 0x1;
        uint bits10_5 = (imm13 >> 5) & 0x3F;
        uint bits4_1 = (imm13 >> 1) & 0xF;
        return (bit12 << 31) | (bits10_5 << 25) | (rs2 << 20) | (rs1 << 15) | (funct3 << 12) | (bits4_1 << 8) | (bit11 << 7) | 0x63;
    }

    private static uint BuildJType(int imm, uint rd)
    {
        uint imm21 = (uint)imm & 0x1FFFFF;
        uint bit20 = (imm21 >> 20) & 0x1;
        uint bits10_1 = (imm21 >> 1) & 0x3FF;
        uint bit11 = (imm21 >> 11) & 0x1;
        uint bits19_12 = (imm21 >> 12) & 0xFF;
        return (bit20 << 31) | (bits10_1 << 21) | (bit11 << 20) | (bits19_12 << 12) | (rd << 7) | 0x6F;
    }

    [Test]
    public void CommandBuilder_Add_ReturnsAddMnemonic()
    {
        var word = BuildRType(rd: 1, rs1: 2, rs2: 3, funct3: 0x0, funct7: 0x00);

        Assert.AreEqual("add x1, x2, x3", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Sub_ReturnsSubMnemonic()
    {
        var word = BuildRType(rd: 5, rs1: 6, rs2: 7, funct3: 0x0, funct7: 0x20);

        Assert.AreEqual("sub x5, x6, x7", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_And_ReturnsAndMnemonic()
    {
        var word = BuildRType(rd: 10, rs1: 11, rs2: 12, funct3: 0x7, funct7: 0x00);

        Assert.AreEqual("and x10, x11, x12", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_UnrecognizedRTypeFunctCodes_ReturnsUnknownR()
    {
        var word = BuildRType(rd: 1, rs1: 2, rs2: 3, funct3: 0x1, funct7: 0x00);

        Assert.AreEqual("unknown_R x1, x2, x3", RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(5, "addi x1, x2, 5")]
    [TestCase(-5, "addi x1, x2, -5")]
    public void CommandBuilder_Addi_ReturnsAddiMnemonicWithSignedImmediate(int imm, string expected)
    {
        var word = BuildIType(imm, rs1: 2, funct3: 0x0, rd: 1);

        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Slli_ReturnsShiftAmountRatherThanRawImmediate()
    {
        var word = BuildIType(4, rs1: 2, funct3: 0x1, rd: 1);

        Assert.AreEqual("slli x1, x2, 4", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Slti_ReturnsSltiMnemonic()
    {
        var word = BuildIType(10, rs1: 2, funct3: 0x2, rd: 1);

        Assert.AreEqual("slti x1, x2, 10", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Sltiu_TreatsImmediateAsUnsigned()
    {
        // imm = -1 sign-extends to 0xFFFFFFFF, which SLTIU must display as unsigned.
        var word = BuildIType(-1, rs1: 2, funct3: 0x3, rd: 1);

        Assert.AreEqual("sltiu x1, x2, 4294967295", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Xori_ReturnsXoriMnemonic()
    {
        var word = BuildIType(0xFF, rs1: 2, funct3: 0x4, rd: 1);

        Assert.AreEqual("xori x1, x2, 255", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Srli_WhenBit30Clear_IsLogicalShift()
    {
        var word = BuildIType(4, rs1: 2, funct3: 0x5, rd: 1);

        Assert.AreEqual("srli x1, x2, 4", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Srai_WhenBit30Set_IsArithmeticShift()
    {
        var word = BuildIType((1 << 10) | 4, rs1: 2, funct3: 0x5, rd: 1);

        Assert.AreEqual("srai x1, x2, 4", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Ori_ReturnsOriMnemonic()
    {
        var word = BuildIType(0x0F, rs1: 2, funct3: 0x6, rd: 1);

        Assert.AreEqual("ori x1, x2, 15", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Andi_ReturnsAndiMnemonic()
    {
        var word = BuildIType(0x0F, rs1: 2, funct3: 0x7, rd: 1);

        Assert.AreEqual("andi x1, x2, 15", RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(8, "lw x5, 8(x2)")]
    [TestCase(-4, "lw x5, -4(x2)")]
    public void CommandBuilder_Lw_ReturnsLoadMnemonic(int imm, string expected)
    {
        var word = BuildIType(imm, rs1: 2, funct3: 0x2, rd: 5, opcode: 0x03);

        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(8, "sw x5, 8(x2)")]
    [TestCase(-4, "sw x5, -4(x2)")]
    public void CommandBuilder_Sw_ReturnsStoreMnemonic(int imm, string expected)
    {
        var word = BuildSType(imm, rs2: 5, rs1: 2, funct3: 0x2);

        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(8, "beq x2, x3, 8")]
    [TestCase(-8, "beq x2, x3, -8")]
    public void CommandBuilder_Beq_ReturnsBranchMnemonic(int imm, string expected)
    {
        var word = BuildBType(imm, rs2: 3, rs1: 2, funct3: 0x0);

        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(100, "jal x1, 100")]
    [TestCase(-100, "jal x1, -100")]
    public void CommandBuilder_Jal_ReturnsJumpMnemonic(int imm, string expected)
    {
        var word = BuildJType(imm, rd: 1);

        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_Jalr_ReturnsJumpRegisterMnemonic()
    {
        var word = BuildIType(4, rs1: 1, funct3: 0x0, rd: 0, opcode: 0x67);

        Assert.AreEqual("jalr x0, 4(x1)", RiscVDecoder.CommandBuilder(word));
    }

    [Test]
    public void CommandBuilder_UnrecognizedOpcode_ReturnsUnknownWithHexValue()
    {
        var word = 0x0123407Fu; // opcode 0x7F is not handled by any case

        Assert.AreEqual("unknown (0x0123407F)", RiscVDecoder.CommandBuilder(word));
    }

    [TestCase(0u, "0")]
    [TestCase(500u, "500")]
    public void CommandBuilder_BelowMinValidInstruction_ReturnsPlainNumber(uint val, string expected)
    {
        Assert.AreEqual(expected, RiscVDecoder.CommandBuilder(val));
    }

    [Test]
    public void CommandBuilder_JustBelowMinValidInstruction_ReturnsPlainNumber()
    {
        uint val = GameConstants.MinValidInstruction - 1;

        Assert.AreEqual(val.ToString(), RiscVDecoder.CommandBuilder(val));
    }

    [Test]
    public void CommandBuilder_AtOrAboveMinValidInstruction_IsTreatedAsAnInstruction()
    {
        uint val = GameConstants.MinValidInstruction;

        var result = RiscVDecoder.CommandBuilder(val);

        Assert.AreNotEqual(val.ToString(), result);
    }
}
