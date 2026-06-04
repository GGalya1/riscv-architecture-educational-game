using UnityEngine;

public static class Alu
{
    public static (int result, bool zero) Calculate(int a, int b, AluOperation operation) {
        var result = 0;
        
        switch (operation)
        {
            case AluOperation.ADD:
                result = a + b;
                break;
            case AluOperation.SUB:
                result = a - b;
                break;
            case AluOperation.AND:
                result = a & b;
                break;
            case AluOperation.OR:
                result = a | b;
                break;

            default:
                Debug.LogError($"ALU Error: Unknown operation code {operation}.");
                break;
        }

        var zero = result == 0;
        return (result, zero);
    }

    public static int Calculate(int a, int b, int operation)
    {
        var result = operation switch
        {
            0 => a + b,
            1 => a - b,
            2 => a & b,
            3 => a | b,
            _ => 0
        };

        return result;
    }
}
