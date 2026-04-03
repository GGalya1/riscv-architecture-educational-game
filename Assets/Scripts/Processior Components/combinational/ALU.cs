using UnityEngine;

public static class ALU
{
    public static (int result, bool zero) calculate(int A, int B, AluOperation operation) {
        int result = 0;
        
        switch (operation)
        {
            case AluOperation.ADD:
                result = A + B;
                break;
            case AluOperation.SUB:
                result = A - B;
                break;
            case AluOperation.AND:
                result = A & B;
                break;
            case AluOperation.OR:
                result = A | B;
                break;

            default:
                Debug.LogError($"ALU Error: Unknown operation code {operation}.");
                break;
        }

        bool zero = result == 0;
        return (result, zero);
    }

    public static int calculate(int A, int B, int operation)
    {
        int result = 0;

        switch (operation)
        {
            case 0:
                result = A + B;
                break;
            case 1:
                result = A - B;
                break;
            case 2:
                result = A & B;
                break;
            case 3:
                result = A | B;
                break;

            default:
                Debug.LogError($"ALU Error: Unknown operation code {operation}.");
                break;
        }

        return result;
    }
}
