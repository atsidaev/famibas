using Raylib_cs;

namespace NET_NES.peripheral.keyboard;

public class FamilyBasicKeyboard : IKeyboard
{
    static int ksrow;
    static int kscol;
    static bool ksenable;
    
    public static readonly KeyboardKey[][][] Matrix =
    {
        new KeyboardKey[][]
        {
            [KeyboardKey.F8, KeyboardKey.Enter, KeyboardKey.LeftBracket, KeyboardKey.RightBracket],
            [KeyboardKey.KpEqual, KeyboardKey.RightShift, KeyboardKey.Backslash, KeyboardKey.Pause]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F7, KeyboardKey.F9, KeyboardKey.Semicolon, KeyboardKey.Semicolon],
            [KeyboardKey.KpSubtract, KeyboardKey.Slash, KeyboardKey.Minus, KeyboardKey.Grave]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F6, KeyboardKey.O, KeyboardKey.L, KeyboardKey.K],
            [KeyboardKey.Period, KeyboardKey.Comma, KeyboardKey.P, KeyboardKey.Zero]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F5, KeyboardKey.I, KeyboardKey.U, KeyboardKey.J],
            [KeyboardKey.M, KeyboardKey.N, KeyboardKey.Nine, KeyboardKey.Eight]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F4, KeyboardKey.Y, KeyboardKey.G, KeyboardKey.H],
            [KeyboardKey.B, KeyboardKey.V, KeyboardKey.Seven, KeyboardKey.Six]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F3, KeyboardKey.T, KeyboardKey.R, KeyboardKey.D],
            [KeyboardKey.F, KeyboardKey.C, KeyboardKey.Five, KeyboardKey.Four]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F2, KeyboardKey.W, KeyboardKey.S, KeyboardKey.A],
            [KeyboardKey.X, KeyboardKey.Z, KeyboardKey.E, KeyboardKey.Three]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.F1, KeyboardKey.Escape, KeyboardKey.Q, KeyboardKey.LeftControl],
            [KeyboardKey.LeftShift, KeyboardKey.LeftSuper, KeyboardKey.One, KeyboardKey.Two]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Delete, KeyboardKey.Up, KeyboardKey.Right, KeyboardKey.Left],
            [KeyboardKey.Down, KeyboardKey.Space, KeyboardKey.Backspace, KeyboardKey.Insert]
        }
    };
    
    public void Write(byte value) {
        byte col = (byte)((value & 2) >> 1);
        ksenable = (value & 4) != 0;
        if (ksenable) {
            if ((value & 1) != 0) {
                ksrow = 0;
            } else if (kscol != 0 && col == 0) {
                ksrow = (ksrow + 1) % 10;
            }
            kscol = col;
        }
    }
    
    public byte Read()
    {
        int state = 0;
        int x;

        byte result = 0;
        if (ksrow == 9) return result;
        for (x = 0; x < 4; x++)
            if (Raylib.IsKeyDown(Matrix[ksrow][kscol][x]))
                state |= 1 << (x + 1);
        return (byte)(result | ((ksenable ? (state ^ 0x1E) : 0)));
    }
}