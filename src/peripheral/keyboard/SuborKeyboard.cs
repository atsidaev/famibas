using Raylib_cs;

namespace NET_NES.peripheral.keyboard;

public class SuborKeyboard : IKeyboard
{
    public static readonly KeyboardKey[][][] Matrix =
    {
        new KeyboardKey[][]
        {
            [KeyboardKey.Four, KeyboardKey.G, KeyboardKey.F, KeyboardKey.C],
            [KeyboardKey.F2, KeyboardKey.E, KeyboardKey.Five, KeyboardKey.V]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Two, KeyboardKey.D, KeyboardKey.S, KeyboardKey.End],
            [KeyboardKey.F1, KeyboardKey.W, KeyboardKey.Three, KeyboardKey.X]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Insert, KeyboardKey.Backspace, KeyboardKey.PageDown, KeyboardKey.Right],
            [KeyboardKey.F8, KeyboardKey.PageUp, KeyboardKey.Delete, KeyboardKey.Home]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Nine, KeyboardKey.I, KeyboardKey.L, KeyboardKey.Comma],
            [KeyboardKey.F5, KeyboardKey.O, KeyboardKey.Zero, KeyboardKey.Period]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.RightBracket, KeyboardKey.Enter, KeyboardKey.Up, KeyboardKey.Left],
            [KeyboardKey.F7, KeyboardKey.LeftBracket, KeyboardKey.Backslash, KeyboardKey.Down]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Q, KeyboardKey.CapsLock, KeyboardKey.Z, KeyboardKey.Tab],
            [KeyboardKey.Escape, KeyboardKey.A, KeyboardKey.One, KeyboardKey.LeftControl]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Seven, KeyboardKey.Y, KeyboardKey.K, KeyboardKey.M],
            [KeyboardKey.F4, KeyboardKey.U, KeyboardKey.Eight, KeyboardKey.J]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Minus, KeyboardKey.Semicolon, KeyboardKey.Apostrophe, KeyboardKey.Slash],
            [KeyboardKey.F6, KeyboardKey.P, KeyboardKey.Equal, KeyboardKey.LeftShift]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.T, KeyboardKey.H, KeyboardKey.N, KeyboardKey.Space],
            [KeyboardKey.F3, KeyboardKey.R, KeyboardKey.Six, KeyboardKey.B]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Kp6, KeyboardKey.Enter, KeyboardKey.Kp4, KeyboardKey.Kp8],
            [KeyboardKey.Kp2, 0, 0, 0]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.LeftAlt, KeyboardKey.Kp4, KeyboardKey.Kp7, KeyboardKey.F11],
            [KeyboardKey.F12, KeyboardKey.Kp1, KeyboardKey.Kp2, KeyboardKey.Kp8]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.KpSubtract, KeyboardKey.KpAdd, KeyboardKey.KpMultiply, KeyboardKey.Kp9],
            [KeyboardKey.F10, KeyboardKey.Kp5, KeyboardKey.KpDivide, KeyboardKey.NumLock]
        },
        new KeyboardKey[][]
        {
            [KeyboardKey.Grave, KeyboardKey.Kp6, KeyboardKey.Pause, KeyboardKey.Space],
            [KeyboardKey.F9, KeyboardKey.Kp3, KeyboardKey.KpDecimal, KeyboardKey.Kp0]
        }
    };
    
    byte ksmode;
    int ksindex;
    
    public void Write(byte v) {
        if ((v & 1) != 0)
        {
            ksmode = 0;
            ksindex = 0;
        }

        v >>= 1;
        if ((v & 2) != 0) {
            if ((ksmode & 1) != 0 && (v & 1) == 0)
                ksindex = (ksindex + 1) % 13;
        }
        ksmode = v;
    }

    public byte Read()
    {
        int ret = 0;
        for (int x = 0; x < 4; x++)
            if (Raylib.IsKeyDown(Matrix[ksindex][ksmode & 1][x]))
                ret |= 1 << (x + 1);
        ret ^= 0x1E;
        return (byte)ret;
    }
}