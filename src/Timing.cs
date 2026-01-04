public enum TimingMode {
    NTSC,
    PAL,
    Dendy
}

public class Timing {
    // CPU
    public int CpuFrequency { get; }

    // PPU
    public int PpuCyclesPerCpuCycle { get; }
    public int ScanlinesPerFrame { get; }
    public int DotsPerScanline { get; }
    public int VBlankScanline { get; }

    // Frame timing
    public int TargetFps { get; }
    public int CpuCyclesPerFrame { get; }

    // APU frame counter (in CPU cycles)
    public int FrameCounterStep1 { get; }
    public int FrameCounterStep2 { get; }
    public int FrameCounterStep3 { get; }
    public int FrameCounterStep4 { get; }
    public int FrameCounterStep5 { get; } // 5-step mode only

    // Audio
    public int AudioSampleRate { get; }

    private Timing(
        int cpuFrequency,
        int ppuCyclesPerCpuCycle,
        int scanlinesPerFrame,
        int dotsPerScanline,
        int vblankScanline,
        int targetFps,
        int frameCounterStep1,
        int frameCounterStep2,
        int frameCounterStep3,
        int frameCounterStep4,
        int frameCounterStep5,
        int audioSampleRate = 44100)
    {
        CpuFrequency = cpuFrequency;
        PpuCyclesPerCpuCycle = ppuCyclesPerCpuCycle;
        ScanlinesPerFrame = scanlinesPerFrame;
        DotsPerScanline = dotsPerScanline;
        VBlankScanline = vblankScanline;
        TargetFps = targetFps;
        CpuCyclesPerFrame = cpuFrequency / targetFps;

        FrameCounterStep1 = frameCounterStep1;
        FrameCounterStep2 = frameCounterStep2;
        FrameCounterStep3 = frameCounterStep3;
        FrameCounterStep4 = frameCounterStep4;
        FrameCounterStep5 = frameCounterStep5;

        AudioSampleRate = audioSampleRate;
    }

    // NTSC: 1.789773 MHz CPU, 60 fps, 262 scanlines
    public static readonly Timing NTSC = new Timing(
        cpuFrequency: 1789773,
        ppuCyclesPerCpuCycle: 3,
        scanlinesPerFrame: 262,
        dotsPerScanline: 341,
        vblankScanline: 241,
        targetFps: 60,
        // Frame counter steps in CPU cycles (APU cycles * 2)
        frameCounterStep1: 7458,
        frameCounterStep2: 14914,
        frameCounterStep3: 22372,
        frameCounterStep4: 29830,
        frameCounterStep5: 37282
    );

    // PAL: 1.662607 MHz CPU, 50 fps, 312 scanlines
    public static readonly Timing PAL = new Timing(
        cpuFrequency: 1662607,
        ppuCyclesPerCpuCycle: 3,
        scanlinesPerFrame: 312,
        dotsPerScanline: 341,
        vblankScanline: 241,
        targetFps: 50,
        // PAL frame counter runs slower
        frameCounterStep1: 8314,
        frameCounterStep2: 16628,
        frameCounterStep3: 24940,
        frameCounterStep4: 33254,
        frameCounterStep5: 41566
    );

    // Dendy: 1.773448 MHz CPU, 50 fps, 312 scanlines (Russian clone)
    public static readonly Timing Dendy = new Timing(
        cpuFrequency: 1773448,
        ppuCyclesPerCpuCycle: 3,
        scanlinesPerFrame: 312,
        dotsPerScanline: 341,
        vblankScanline: 241,
        targetFps: 50,
        // Dendy frame counter similar to PAL but slightly faster
        frameCounterStep1: 8230,
        frameCounterStep2: 16460,
        frameCounterStep3: 24690,
        frameCounterStep4: 32920,
        frameCounterStep5: 41150
    );

    public static Timing FromMode(TimingMode mode) => mode switch {
        TimingMode.NTSC => NTSC,
        TimingMode.PAL => PAL,
        TimingMode.Dendy => Dendy,
        _ => NTSC
    };
}
