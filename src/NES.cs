using NET_NES;

public class NES : IDisposable {
    Cartridge cartridge;
    Bus bus;
    private PcmPlayer? audioPlayer;
    private Timing timing;

    public NES() {
        timing = Helper.timing;

        cartridge = new Cartridge(Helper.romPath);
        bus = new Bus(cartridge, timing);

        bus.cpu.Reset();

        // Initialize audio player
        try {
            audioPlayer = new PcmPlayer(timing.AudioSampleRate, 1, 16);
        } catch (Exception ex) {
            Console.WriteLine($"Failed to initialize audio: {ex.Message}");
            audioPlayer = null;
        }

        Console.WriteLine($"NES ({Helper.timingMode})");
    }

    public void Run() {
        int cycles = 0;

        bus.input.UpdateController();

        while (cycles < timing.CpuCyclesPerFrame) {
            int used = bus.cpu.ExecuteInstruction();
            cycles += used;
            bus.ppu.Step(used * timing.PpuCyclesPerCpuCycle);
            bus.apu.Step(used);
        }

        bus.ppu.DrawFrame(Helper.scale);

        // Play audio samples
        if (audioPlayer != null) {
            byte[] audioSamples = bus.apu.GetAudioSamples();
            if (audioSamples.Length > 0) {
                audioPlayer.Play(audioSamples);
            }
        }
    }

    public void Dispose() {
        audioPlayer?.Dispose();
    }
}