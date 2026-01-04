namespace NET_NES;

using NAudio.Wave;

public class PcmPlayer : IDisposable
{
    private readonly WaveOutEvent output;
    private readonly BufferedWaveProvider buffer;

    public PcmPlayer(
        int sampleRate,
        int channels,
        int bitsPerSample)
    {
        var format = new WaveFormat(sampleRate, bitsPerSample, channels);
        buffer = new BufferedWaveProvider(format)
        {
            BufferDuration = TimeSpan.FromMilliseconds(200),
            DiscardOnBufferOverflow = true
        };

        output = new WaveOutEvent
        {
            DesiredLatency = 80,
            NumberOfBuffers = 3
        };
        output.Init(buffer);
        output.Play();
    }

    public void Play(byte[] pcm)
    {
        buffer.AddSamples(pcm, 0, pcm.Length);
    }

    public void Dispose()
    {
        output.Stop();
        output.Dispose();
    }
}