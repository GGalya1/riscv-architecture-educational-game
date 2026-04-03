public enum AluOperation
    {
        // Arithmetic
        ADD = 0,    // Addition (example, R-type: ADD, I-type: ADDI)
        SUB = 1,    // Substitution (example, R-type: SUB, B-type: BEQ/BNE)

        // Logic
        AND = 2,    // Logical AND
        OR = 3,     // Logical OR
    }