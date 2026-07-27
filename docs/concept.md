# Concept & Teaching Approach

!!! warning "Draft"
    Test version of this page - content is basic for now and will be expanded.

## Learning by operating the hardware yourself

On every level, the player directly sets the control signals for that tick: which path a multiplexer should select, which operation the ALU should perform, whether a given register's write-enable is on or off. The game does **not** infer any of this automatically from the instruction - if you set a signal wrong, the computed result is wrong, and you see it immediately. 

!!! hint
    Immediate, concrete feedback loop is the core teaching mechanic.

For example, on a single-cycle level executing an `addi` instruction, the player has to recognize that the second ALU operand should come from the sign-extended immediate (not the register file), and set the corresponding multiplexer path accordingly - the game won't do that inference for you.

## Why this specific progression

1. **Isolated components first** - understand what a register, an ALU, or a multiplexer does on its own, with no other moving parts to worry about.
2. **Subsets of processor** - understand how components are combined into blocks and what functions they can perform.
3. **Single-cycle processor** - the same components, now wired together into something that actually executes instructions, but simplified so each instruction fully completes in one tick.
4. **Multi-cycle processor** - the same hardware, reused across multiple ticks per instruction - introduces the idea that hardware is a *shared, reusable* resource, not duplicated per instruction.
5. **Pipelined processor** - the payoff and the hardest section: multiple instructions overlap in flight, which is faster, but introduces real hazards that a naive pipeline gets wrong:
   - **Data hazards**, solved via *forwarding* - bypassing a not-yet-committed result directly to where it's needed, instead of waiting for it to reach the register file.
   - **Control hazards**, solved via *flushing* - when a branch is taken, the instructions that were speculatively fetched right behind it turn out to be wrong and have to be discarded.