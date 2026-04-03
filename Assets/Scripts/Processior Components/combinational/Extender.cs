public class Extender
{
    private const int IMM_SRC_TYPE_I = 0;
    private const int IMM_SRC_TYPE_S = 1;
    private const int IMM_SRC_TYPE_B = 2;
    private const int IMM_SRC_TYPE_J = 3;


    public static int Evaluate(int control, uint value)
    {
        switch (control)
        {
            case IMM_SRC_TYPE_I:
                return (int)value >> 20;

            case IMM_SRC_TYPE_S:
                int immS = (int)(((value >> 25) << 5) | ((value >> 7) & 0x1F));
                return SignExtend(immS, 12);

            case IMM_SRC_TYPE_B:
                return SignExtend(ExtendB(value), 13);

            case IMM_SRC_TYPE_J:
                return SignExtend(ExtendJ(value), 21);

            default:
                return 0;
        }
    }
   
    
    private static int ExtendB(uint val)
    {
        uint assembled = (((val >> 31) & 0x1) << 12) |           // бит 31 -> imm[12]
                     (((val >> 7) & 0x1) << 11) |    // бит 7  -> imm[11]
                     (((val >> 25) & 0x3F) << 5) |   // биты 30:25 -> imm[10:5]
                     (((val >> 8) & 0xF) << 1);      // биты 11:8  -> imm[4:1]

        return SignExtend((int)assembled, 13);
    }
    
    private static int ExtendJ(uint val)
    {
        uint assembled = ((val >> 31) << 20) |           // бит 31 -> imm[20]
                     (((val >> 12) & 0xFF) << 12) |  // биты 19:12 -> imm[19:12]
                     (((val >> 20) & 0x1) << 11) |   // бит 20 -> imm[11]
                     (((val >> 21) & 0x3FF) << 1);   // биты 30:21 -> imm[10:1]

        return SignExtend((int)assembled, 21);
    }

    private static int SignExtend(int value, int bitCount)
    {
        int mask = 1 << (bitCount - 1);
        if ((value & mask) != 0)
        {
            return value | (unchecked((int)0xFFFFFFFF) << bitCount);
        }
        return value;
    }
}