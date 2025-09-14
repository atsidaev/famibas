using Raylib_cs;

namespace NET_NES.peripheral.keyboard;

public class SonicRec9388Keyboard: IKeyboard
{
    public static readonly KeyboardKey[][] Matrix =
    {
        [
            KeyboardKey.LeftShift, KeyboardKey.Tab, KeyboardKey.Grave, KeyboardKey.LeftControl,
            KeyboardKey.CapsLock, KeyboardKey.LeftAlt, KeyboardKey.Space, KeyboardKey.Escape
        ],
        [
            KeyboardKey.F3, KeyboardKey.F1, KeyboardKey.F2, KeyboardKey.F8,
            KeyboardKey.F4, KeyboardKey.F5, KeyboardKey.F7, KeyboardKey.F6
        ],
        [
            KeyboardKey.Z, KeyboardKey.Q, KeyboardKey.One, KeyboardKey.KpEnter,
            KeyboardKey.A, KeyboardKey.KpDecimal, KeyboardKey.Kp0, KeyboardKey.KpAdd
        ],
        [
            KeyboardKey.X, KeyboardKey.W, KeyboardKey.Two, KeyboardKey.Kp9,
            KeyboardKey.S, KeyboardKey.Kp6, KeyboardKey.Kp3, KeyboardKey.KpMultiply
        ],
        [
            KeyboardKey.C, KeyboardKey.E, KeyboardKey.Three, KeyboardKey.Kp8,
            KeyboardKey.D, KeyboardKey.Kp5, KeyboardKey.Kp2, KeyboardKey.KpDivide
        ],
        [
            KeyboardKey.V, KeyboardKey.R, KeyboardKey.Four, KeyboardKey.Kp7,
            KeyboardKey.F, KeyboardKey.Kp4, KeyboardKey.Kp1, KeyboardKey.NumLock
        ],
        [
            KeyboardKey.B, KeyboardKey.T, KeyboardKey.Five, KeyboardKey.RightBracket,
            KeyboardKey.G, KeyboardKey.Enter, KeyboardKey.Backslash, KeyboardKey.Backspace
        ],
        [
            KeyboardKey.Comma, KeyboardKey.I, KeyboardKey.Eight, KeyboardKey.O,
            KeyboardKey.K, KeyboardKey.L, KeyboardKey.Period, KeyboardKey.Nine
        ],
        [
            KeyboardKey.M, KeyboardKey.U, KeyboardKey.Seven, KeyboardKey.P,
            KeyboardKey.J, KeyboardKey.Semicolon, KeyboardKey.Slash, KeyboardKey.Zero
        ],
        [
            KeyboardKey.N, KeyboardKey.Y, KeyboardKey.Six, KeyboardKey.LeftBracket,
            KeyboardKey.H, KeyboardKey.Apostrophe, KeyboardKey.Equal, KeyboardKey.Minus
        ],
        [
            0, 0, KeyboardKey.F9, KeyboardKey.KpSubtract,
            0, KeyboardKey.F10, KeyboardKey.F12, KeyboardKey.F11
        ]
    };

    private int _rowNum;
    private int _columnNum;
    
    public byte Read()
    {
        if (_rowNum >= Matrix.Length || _columnNum > 7)
            return 0x00;

        if (Raylib.IsKeyDown(Matrix[_rowNum][_columnNum++]))
            return (1 << 6);
        
        // Read triggers shift register 4021 
        // Result is bit 6
        return 0x00;
    }

    public void Write(byte value)
    {
        // Bit 0 - latch 4021
        if ((value & 0x01) == 0x1)
            _columnNum = 0;
        
        // Bit 1 (inv) - clear 74161, reset to first row
        if ((value & 0x02) == 0)
            _rowNum = 0;

        // Bit 2 - clock rows
        if ((value & 0x04) == 0x04)
            _rowNum = (_rowNum + 1) % 16;
    }
}