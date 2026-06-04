public static class Multiplexer
{
    public static int Select2To1(int a, int b, bool control) {
        return control ? a : b;
    }

    public static int SelectNto1(int[] inputs, int control) {
        // if(control < 0 || control >= inputs.Length) 
        //    Debug.LogError($"Multiplexer Error: {control} is not in range of inputs for MUL_N.");
        return inputs[control];
    }
}
