using NET_NES;

public class NES : IDisposable {
    Cartridge cartridge;
    Bus bus;
    private PcmPlayer? audioPlayer;

    public NES() {
        cartridge = new Cartridge(Helper.romPath);
        bus = new Bus(cartridge);

        bus.cpu.Reset();
        
        // Initialize audio player
        try {
            audioPlayer = new PcmPlayer(44100, 1, 16); // 44.1kHz, mono, 16-bit
        } catch (Exception ex) {
            Console.WriteLine($"Failed to initialize audio: {ex.Message}");
            audioPlayer = null;
        }
        
        Console.WriteLine("NES");
    }

    public void Run() {
        int cycles = 0;

        bus.input.UpdateController();

        while (cycles < 29828) {
            int used = bus.cpu.ExecuteInstruction();
            cycles += used;
            bus.ppu.Step(used * 3);
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