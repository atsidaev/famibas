public class APU {
    private Bus bus;

    // Pulse Channel 1 ($4000-$4003)
    private byte pulse1Duty;
    private bool pulse1LengthHalt;
    private bool pulse1ConstantVolume;
    private byte pulse1Volume;
    private bool pulse1SweepEnabled;
    private byte pulse1SweepPeriod;
    private bool pulse1SweepNegate;
    private byte pulse1SweepShift;
    private ushort pulse1Timer;
    private ushort pulse1TimerReload;
    private byte pulse1DutySequence;
    private byte pulse1DutyIndex;
    private byte pulse1LengthCounter;
    private byte pulse1EnvelopeCounter;
    private byte pulse1EnvelopeValue;
    private bool pulse1EnvelopeStart;
    private byte pulse1SweepCounter;
    private bool pulse1SweepReload;

    // Pulse Channel 2 ($4004-$4007)
    private byte pulse2Duty;
    private bool pulse2LengthHalt;
    private bool pulse2ConstantVolume;
    private byte pulse2Volume;
    private bool pulse2SweepEnabled;
    private byte pulse2SweepPeriod;
    private bool pulse2SweepNegate;
    private byte pulse2SweepShift;
    private ushort pulse2Timer;
    private ushort pulse2TimerReload;
    private byte pulse2DutySequence;
    private byte pulse2DutyIndex;
    private byte pulse2LengthCounter;
    private byte pulse2EnvelopeCounter;
    private byte pulse2EnvelopeValue;
    private bool pulse2EnvelopeStart;
    private byte pulse2SweepCounter;
    private bool pulse2SweepReload;

    // Triangle Channel ($4008-$400B)
    private bool triangleLinearCounterHalt;
    private byte triangleLinearCounterReload;
    private byte triangleLinearCounter;
    private ushort triangleTimer;
    private ushort triangleTimerReload;
    private byte triangleLengthCounter;
    private byte triangleSequenceIndex;
    private bool triangleLinearCounterReloadFlag;

    // Noise Channel ($400C-$400F)
    private bool noiseLengthHalt;
    private bool noiseConstantVolume;
    private byte noiseVolume;
    private bool noiseShortMode;
    private ushort noiseShiftRegister;
    private ushort noiseTimer;
    private ushort noiseTimerReload;
    private byte noiseLengthCounter;
    private byte noiseEnvelopeCounter;
    private byte noiseEnvelopeValue;
    private bool noiseEnvelopeStart;

    // DMC Channel ($4010-$4013)
    private bool dmcIRQEnabled;
    private bool dmcLoop;
    private byte dmcRate;
    private byte dmcDirectLoad;
    private ushort dmcSampleAddress;
    private ushort dmcSampleLength;
    private ushort dmcCurrentAddress;
    private ushort dmcBytesRemaining;
    private byte dmcShiftRegister;
    private byte dmcBitsRemaining;
    private byte dmcOutputLevel;
    private bool dmcSilence;
    private bool dmcIRQPending;
    private int dmcTimer;
    private byte dmcSampleBuffer;
    private bool dmcSampleBufferEmpty;

    // Status Register ($4015)
    private bool statusPulse1Enabled;
    private bool statusPulse2Enabled;
    private bool statusTriangleEnabled;
    private bool statusNoiseEnabled;
    private bool statusDMCEnabled;

    // Frame Counter ($4017)
    private bool frameCounterMode; // false = 4-step, true = 5-step
    private bool frameCounterIRQInhibit;

    // Internal state
    private int cycleCounter;
    private int apuCycleAccumulator; // For tracking APU half-cycles (pulse/noise tick every 2 CPU cycles)

    // Audio output
    private const int SampleRate = 44100;
    private const int CpuFrequency = 1789773; // NES CPU frequency
    private int cyclesPerSample;
    private int sampleCounter;
    private int cyclesAccumulated;
    
    // Sample buffer for frame rendering
    private List<float> sampleBuffer;

    // High-pass filter state for DC removal
    private float prevInput = 0;
    private float prevOutput = 0;
    private const float HighPassAlpha = 0.995f; // Cutoff ~10Hz at 44100Hz

    public APU(Bus bus) {
        this.bus = bus;

        // Initialize all channels
        Reset();

        cyclesPerSample = CpuFrequency / SampleRate;
        sampleCounter = 0;
        cyclesAccumulated = 0;
        sampleBuffer = new List<float>();

        Console.WriteLine("APU init");
    }

    public void Reset() {
        // Pulse 1
        pulse1Duty = 0;
        pulse1LengthHalt = false;
        pulse1ConstantVolume = false;
        pulse1Volume = 0;
        pulse1SweepEnabled = false;
        pulse1SweepPeriod = 0;
        pulse1SweepNegate = false;
        pulse1SweepShift = 0;
        pulse1Timer = 0;
        pulse1TimerReload = 0;
        pulse1DutySequence = 0;
        pulse1DutyIndex = 0;
        pulse1LengthCounter = 0;
        pulse1EnvelopeCounter = 0;
        pulse1EnvelopeValue = 0;
        pulse1EnvelopeStart = false;
        pulse1SweepCounter = 0;
        pulse1SweepReload = false;

        // Pulse 2
        pulse2Duty = 0;
        pulse2LengthHalt = false;
        pulse2ConstantVolume = false;
        pulse2Volume = 0;
        pulse2SweepEnabled = false;
        pulse2SweepPeriod = 0;
        pulse2SweepNegate = false;
        pulse2SweepShift = 0;
        pulse2Timer = 0;
        pulse2TimerReload = 0;
        pulse2DutySequence = 0;
        pulse2DutyIndex = 0;
        pulse2LengthCounter = 0;
        pulse2EnvelopeCounter = 0;
        pulse2EnvelopeValue = 0;
        pulse2EnvelopeStart = false;
        pulse2SweepCounter = 0;
        pulse2SweepReload = false;

        // Triangle
        triangleLinearCounterHalt = false;
        triangleLinearCounterReload = 0;
        triangleLinearCounter = 0;
        triangleTimer = 0;
        triangleTimerReload = 0;
        triangleLengthCounter = 0;
        triangleSequenceIndex = 0;
        triangleLinearCounterReloadFlag = false;

        // Noise
        noiseLengthHalt = false;
        noiseConstantVolume = false;
        noiseVolume = 0;
        noiseShortMode = false;
        noiseShiftRegister = 1;
        noiseTimer = 0;
        noiseTimerReload = 0;
        noiseLengthCounter = 0;
        noiseEnvelopeCounter = 0;
        noiseEnvelopeValue = 0;
        noiseEnvelopeStart = false;

        // DMC
        dmcIRQEnabled = false;
        dmcLoop = false;
        dmcRate = 0;
        dmcDirectLoad = 0;
        dmcSampleAddress = 0;
        dmcSampleLength = 0;
        dmcCurrentAddress = 0;
        dmcBytesRemaining = 0;
        dmcShiftRegister = 0;
        dmcBitsRemaining = 0;
        dmcOutputLevel = 0;
        dmcSilence = true;
        dmcIRQPending = false;
        dmcTimer = 0;
        dmcSampleBuffer = 0;
        dmcSampleBufferEmpty = true;

        // Status
        statusPulse1Enabled = false;
        statusPulse2Enabled = false;
        statusTriangleEnabled = false;
        statusNoiseEnabled = false;
        statusDMCEnabled = false;

        // Frame counter
        frameCounterMode = false;
        frameCounterIRQInhibit = false;
        cycleCounter = 0;
        cyclesAccumulated = 0;
        apuCycleAccumulator = 0;
        sampleBuffer?.Clear();
        prevInput = 0;
        prevOutput = 0;
    }

    public byte Read(ushort address) {
        if (address == 0x4015) {
            byte result = 0;
            if (pulse1LengthCounter > 0) result |= 0x01;
            if (pulse2LengthCounter > 0) result |= 0x02;
            if (triangleLengthCounter > 0) result |= 0x04;
            if (noiseLengthCounter > 0) result |= 0x08;
            if (dmcBytesRemaining > 0) result |= 0x10;
            if (dmcIRQPending) result |= 0x80;
            dmcIRQPending = false;
            return result;
        }
        return 0;
    }

    public void Write(ushort address, byte value) {
        switch (address) {
            // Pulse 1
            case 0x4000:
                pulse1Duty = (byte)((value >> 6) & 0x03);
                pulse1LengthHalt = (value & 0x20) != 0;
                pulse1ConstantVolume = (value & 0x10) != 0;
                pulse1Volume = (byte)(value & 0x0F);
                break;
            case 0x4001:
                pulse1SweepEnabled = (value & 0x80) != 0;
                pulse1SweepPeriod = (byte)((value >> 4) & 0x07);
                pulse1SweepNegate = (value & 0x08) != 0;
                pulse1SweepShift = (byte)(value & 0x07);
                pulse1SweepReload = true;
                break;
            case 0x4002:
                pulse1TimerReload = (ushort)((pulse1TimerReload & 0xFF00) | value);
                break;
            case 0x4003:
                pulse1TimerReload = (ushort)((pulse1TimerReload & 0x00FF) | ((value & 0x07) << 8));
                pulse1Timer = pulse1TimerReload;
                pulse1DutyIndex = 0;
                if (statusPulse1Enabled) {
                    pulse1LengthCounter = LengthCounterTable[(value >> 3) & 0x1F];
                }
                pulse1EnvelopeStart = true;
                break;

            // Pulse 2
            case 0x4004:
                pulse2Duty = (byte)((value >> 6) & 0x03);
                pulse2LengthHalt = (value & 0x20) != 0;
                pulse2ConstantVolume = (value & 0x10) != 0;
                pulse2Volume = (byte)(value & 0x0F);
                break;
            case 0x4005:
                pulse2SweepEnabled = (value & 0x80) != 0;
                pulse2SweepPeriod = (byte)((value >> 4) & 0x07);
                pulse2SweepNegate = (value & 0x08) != 0;
                pulse2SweepShift = (byte)(value & 0x07);
                pulse2SweepReload = true;
                break;
            case 0x4006:
                pulse2TimerReload = (ushort)((pulse2TimerReload & 0xFF00) | value);
                break;
            case 0x4007:
                pulse2TimerReload = (ushort)((pulse2TimerReload & 0x00FF) | ((value & 0x07) << 8));
                pulse2Timer = pulse2TimerReload;
                pulse2DutyIndex = 0;
                if (statusPulse2Enabled) {
                    pulse2LengthCounter = LengthCounterTable[(value >> 3) & 0x1F];
                }
                pulse2EnvelopeStart = true;
                break;

            // Triangle
            case 0x4008:
                triangleLinearCounterHalt = (value & 0x80) != 0;
                triangleLinearCounterReload = (byte)(value & 0x7F);
                break;
            case 0x400A:
                triangleTimerReload = (ushort)((triangleTimerReload & 0xFF00) | value);
                break;
            case 0x400B:
                triangleTimerReload = (ushort)((triangleTimerReload & 0x00FF) | ((value & 0x07) << 8));
                triangleTimer = triangleTimerReload;
                if (statusTriangleEnabled) {
                    triangleLengthCounter = LengthCounterTable[(value >> 3) & 0x1F];
                }
                triangleLinearCounterReloadFlag = true;
                break;

            // Noise
            case 0x400C:
                noiseLengthHalt = (value & 0x20) != 0;
                noiseConstantVolume = (value & 0x10) != 0;
                noiseVolume = (byte)(value & 0x0F);
                break;
            case 0x400E:
                noiseShortMode = (value & 0x80) != 0;
                noiseTimerReload = NoisePeriodTable[value & 0x0F];
                break;
            case 0x400F:
                if (statusNoiseEnabled) {
                    noiseLengthCounter = LengthCounterTable[(value >> 3) & 0x1F];
                }
                noiseEnvelopeStart = true;
                break;

            // DMC
            case 0x4010:
                dmcIRQEnabled = (value & 0x80) != 0;
                dmcLoop = (value & 0x40) != 0;
                dmcRate = (byte)(value & 0x0F);
                if (!dmcIRQEnabled) {
                    dmcIRQPending = false;
                }
                break;
            case 0x4011:
                dmcDirectLoad = (byte)(value & 0x7F);
                dmcOutputLevel = dmcDirectLoad;
                break;
            case 0x4012:
                dmcSampleAddress = (ushort)(0xC000 + (value * 64));
                break;
            case 0x4013:
                dmcSampleLength = (ushort)((value * 16) + 1);
                break;

            // Status
            case 0x4015:
                statusPulse1Enabled = (value & 0x01) != 0;
                statusPulse2Enabled = (value & 0x02) != 0;
                statusTriangleEnabled = (value & 0x04) != 0;
                statusNoiseEnabled = (value & 0x08) != 0;
                statusDMCEnabled = (value & 0x10) != 0;

                if (!statusPulse1Enabled) {
                    pulse1LengthCounter = 0;
                }
                if (!statusPulse2Enabled) {
                    pulse2LengthCounter = 0;
                }
                if (!statusTriangleEnabled) {
                    triangleLengthCounter = 0;
                }
                if (!statusNoiseEnabled) {
                    noiseLengthCounter = 0;
                }
                if (!statusDMCEnabled) {
                    dmcBytesRemaining = 0;
                    dmcSilence = true;
                } else {
                    if (dmcBytesRemaining == 0) {
                        dmcCurrentAddress = dmcSampleAddress;
                        dmcBytesRemaining = dmcSampleLength;
                        dmcSampleBufferEmpty = true;
                    }
                }
                break;

            // Frame Counter
            case 0x4017:
                frameCounterMode = (value & 0x80) != 0;
                frameCounterIRQInhibit = (value & 0x40) != 0;
                if (frameCounterIRQInhibit) {
                    dmcIRQPending = false;
                }
                cycleCounter = 0;
                if (frameCounterMode) {
                    QuarterFrame();
                    HalfFrame();
                }
                break;
        }
    }

    public void Step(int cycles) {
        cycleCounter += cycles;
        cyclesAccumulated += cycles;

        // Pulse and noise channels tick at APU frequency (CPU/2)
        // Triangle ticks at CPU frequency
        apuCycleAccumulator += cycles;
        int apuCycles = apuCycleAccumulator / 2;
        apuCycleAccumulator %= 2;

        // Step channels
        StepPulse1(apuCycles);
        StepPulse2(apuCycles);
        StepTriangle(cycles); // Triangle runs at CPU speed
        StepNoise(apuCycles);
        StepDMC(cycles);
        
        // Generate audio samples
        while (cyclesAccumulated >= cyclesPerSample) {
            cyclesAccumulated -= cyclesPerSample;
            float sample = GetSample();
            sampleBuffer.Add(sample);
        }

        // Frame counter - thresholds in CPU cycles (APU cycles * 2)
        // APU runs at CPU/2, original thresholds were in APU cycles
        int oldCounter = cycleCounter - cycles;

        if (!frameCounterMode) {
            // 4-step mode (CPU cycles: 7458, 14914, 22372, 29830)
            if (oldCounter < 7458 && cycleCounter >= 7458) {
                QuarterFrame();
            }
            if (oldCounter < 14914 && cycleCounter >= 14914) {
                QuarterFrame();
            }
            if (oldCounter < 22372 && cycleCounter >= 22372) {
                QuarterFrame();
            }
            if (oldCounter < 29830 && cycleCounter >= 29830) {
                QuarterFrame();
                HalfFrame();
                cycleCounter -= 29830;
            }
        } else {
            // 5-step mode (CPU cycles: 7458, 14914, 22372, 29830, 37282)
            if (oldCounter < 7458 && cycleCounter >= 7458) {
                QuarterFrame();
            }
            if (oldCounter < 14914 && cycleCounter >= 14914) {
                QuarterFrame();
            }
            if (oldCounter < 22372 && cycleCounter >= 22372) {
                QuarterFrame();
            }
            if (oldCounter < 29830 && cycleCounter >= 29830) {
                QuarterFrame();
                HalfFrame();
            }
            if (oldCounter < 37282 && cycleCounter >= 37282) {
                QuarterFrame();
                HalfFrame();
                cycleCounter -= 37282;
            }
        }
    }

    private void StepPulse1(int cycles) {
        if (pulse1Timer == 0) return;

        for (int i = 0; i < cycles; i++) {
            pulse1Timer--;
            if (pulse1Timer == 0) {
                pulse1Timer = pulse1TimerReload;
                pulse1DutyIndex = (byte)((pulse1DutyIndex + 1) % 8);
            }
        }
    }

    private void StepPulse2(int cycles) {
        if (pulse2Timer == 0) return;

        for (int i = 0; i < cycles; i++) {
            pulse2Timer--;
            if (pulse2Timer == 0) {
                pulse2Timer = pulse2TimerReload;
                pulse2DutyIndex = (byte)((pulse2DutyIndex + 1) % 8);
            }
        }
    }

    private void StepTriangle(int cycles) {
        if (triangleTimer == 0) return;

        for (int i = 0; i < cycles; i++) {
            triangleTimer--;
            if (triangleTimer == 0) {
                triangleTimer = triangleTimerReload;
                if (triangleLengthCounter > 0 && triangleLinearCounter > 0) {
                    triangleSequenceIndex = (byte)((triangleSequenceIndex + 1) % 32);
                }
            }
        }
    }

    private void StepNoise(int cycles) {
        if (noiseTimer == 0) return;

        for (int i = 0; i < cycles; i++) {
            noiseTimer--;
            if (noiseTimer == 0) {
                noiseTimer = noiseTimerReload;
                bool bit0 = (noiseShiftRegister & 0x01) != 0;
                bool bit1 = noiseShortMode ? ((noiseShiftRegister & 0x40) != 0) : ((noiseShiftRegister & 0x02) != 0);
                bool feedback = bit0 ^ bit1;
                noiseShiftRegister = (ushort)((noiseShiftRegister >> 1) | (feedback ? 0x4000 : 0));
            }
        }
    }

    private void StepDMC(int cycles) {
        if (!statusDMCEnabled || dmcBytesRemaining == 0) {
            dmcSilence = true;
            return;
        }

        dmcTimer -= cycles;
        if (dmcTimer <= 0) {
            dmcTimer += DMCPeriodTable[dmcRate & 0x0F];

            if (!dmcSilence) {
                if ((dmcShiftRegister & 0x01) != 0) {
                    if (dmcOutputLevel <= 125) {
                        dmcOutputLevel += 2;
                    }
                } else {
                    if (dmcOutputLevel >= 2) {
                        dmcOutputLevel -= 2;
                    }
                }
                dmcShiftRegister >>= 1;
                dmcBitsRemaining--;
            }

            if (dmcBitsRemaining == 0) {
                dmcBitsRemaining = 8;
                if (dmcSampleBufferEmpty) {
                    dmcSilence = true;
                } else {
                    dmcSilence = false;
                    dmcShiftRegister = dmcSampleBuffer;
                    dmcSampleBufferEmpty = true;
                }
            }

            // Load sample if buffer is empty
            if (dmcSampleBufferEmpty && dmcBytesRemaining > 0) {
                dmcSampleBuffer = bus.Read(dmcCurrentAddress);
                dmcSampleBufferEmpty = false;
                dmcCurrentAddress++;
                if (dmcCurrentAddress == 0xFFFF) {
                    dmcCurrentAddress = 0x8000;
                }
                dmcBytesRemaining--;

                if (dmcBytesRemaining == 0) {
                    if (dmcLoop) {
                        dmcCurrentAddress = dmcSampleAddress;
                        dmcBytesRemaining = dmcSampleLength;
                    } else {
                        if (dmcIRQEnabled) {
                            dmcIRQPending = true;
                        }
                    }
                }
            }
        }
    }

    private void QuarterFrame() {
        // Envelope and linear counter
        if (pulse1EnvelopeStart) {
            pulse1EnvelopeValue = 15;
            pulse1EnvelopeCounter = pulse1Volume;
            pulse1EnvelopeStart = false;
        } else if (pulse1EnvelopeCounter > 0) {
            pulse1EnvelopeCounter--;
        } else {
            pulse1EnvelopeCounter = pulse1Volume;
            if (pulse1EnvelopeValue > 0) {
                pulse1EnvelopeValue--;
            } else if (pulse1LengthHalt) {
                pulse1EnvelopeValue = 15;
            }
        }

        if (pulse2EnvelopeStart) {
            pulse2EnvelopeValue = 15;
            pulse2EnvelopeCounter = pulse2Volume;
            pulse2EnvelopeStart = false;
        } else if (pulse2EnvelopeCounter > 0) {
            pulse2EnvelopeCounter--;
        } else {
            pulse2EnvelopeCounter = pulse2Volume;
            if (pulse2EnvelopeValue > 0) {
                pulse2EnvelopeValue--;
            } else if (pulse2LengthHalt) {
                pulse2EnvelopeValue = 15;
            }
        }

        if (noiseEnvelopeStart) {
            noiseEnvelopeValue = 15;
            noiseEnvelopeCounter = noiseVolume;
            noiseEnvelopeStart = false;
        } else if (noiseEnvelopeCounter > 0) {
            noiseEnvelopeCounter--;
        } else {
            noiseEnvelopeCounter = noiseVolume;
            if (noiseEnvelopeValue > 0) {
                noiseEnvelopeValue--;
            } else if (noiseLengthHalt) {
                noiseEnvelopeValue = 15;
            }
        }

        if (triangleLinearCounterReloadFlag) {
            triangleLinearCounter = triangleLinearCounterReload;
        } else if (triangleLinearCounter > 0) {
            triangleLinearCounter--;
        }
        if (triangleLinearCounterHalt) {
            triangleLinearCounterReloadFlag = false;
        }
    }

    private void HalfFrame() {
        // Length counters and sweep units
        if (!pulse1LengthHalt && pulse1LengthCounter > 0) {
            pulse1LengthCounter--;
        }

        if (!pulse2LengthHalt && pulse2LengthCounter > 0) {
            pulse2LengthCounter--;
        }

        if (!triangleLinearCounterHalt && triangleLengthCounter > 0) {
            triangleLengthCounter--;
        }

        if (!noiseLengthHalt && noiseLengthCounter > 0) {
            noiseLengthCounter--;
        }

        // Sweep units
        if (pulse1SweepReload) {
            pulse1SweepCounter = pulse1SweepPeriod;
            pulse1SweepReload = false;
        } else if (pulse1SweepCounter > 0) {
            pulse1SweepCounter--;
        } else {
            pulse1SweepCounter = pulse1SweepPeriod;
            if (pulse1SweepEnabled && pulse1LengthCounter > 0) {
                ushort change = (ushort)(pulse1TimerReload >> pulse1SweepShift);
                if (pulse1SweepNegate) {
                    pulse1TimerReload = (ushort)(pulse1TimerReload - change - 1);
                } else {
                    pulse1TimerReload = (ushort)(pulse1TimerReload + change);
                }
                if (pulse1TimerReload < 8) {
                    pulse1LengthCounter = 0;
                }
            }
        }

        if (pulse2SweepReload) {
            pulse2SweepCounter = pulse2SweepPeriod;
            pulse2SweepReload = false;
        } else if (pulse2SweepCounter > 0) {
            pulse2SweepCounter--;
        } else {
            pulse2SweepCounter = pulse2SweepPeriod;
            if (pulse2SweepEnabled && pulse2LengthCounter > 0) {
                ushort change = (ushort)(pulse2TimerReload >> pulse2SweepShift);
                if (pulse2SweepNegate) {
                    pulse2TimerReload = (ushort)(pulse2TimerReload - change - 1);
                } else {
                    pulse2TimerReload = (ushort)(pulse2TimerReload + change);
                }
                if (pulse2TimerReload < 8) {
                    pulse2LengthCounter = 0;
                }
            }
        }
    }

    public float GetSample() {
        float pulse1Output = GetPulse1Output();
        float pulse2Output = GetPulse2Output();
        float triangleOutput = GetTriangleOutput();
        float noiseOutput = GetNoiseOutput();
        float dmcOutput = GetDMCOutput();

        // Mix channels using NES-style mixing ratios
        float output = pulse1Output * 0.20f + pulse2Output * 0.20f +
                      triangleOutput * 0.20f + noiseOutput * 0.15f +
                      dmcOutput * 0.10f;

        return output;
    }

    private float GetPulse1Output() {
        if (pulse1LengthCounter == 0 || pulse1TimerReload < 8) return 0;
        byte dutyValue = DutySequences[pulse1Duty, pulse1DutyIndex];
        if (dutyValue == 0) return 0;
        byte volume = pulse1ConstantVolume ? pulse1Volume : pulse1EnvelopeValue;
        return volume / 15.0f;
    }

    private float GetPulse2Output() {
        if (pulse2LengthCounter == 0 || pulse2TimerReload < 8) return 0;
        byte dutyValue = DutySequences[pulse2Duty, pulse2DutyIndex];
        if (dutyValue == 0) return 0;
        byte volume = pulse2ConstantVolume ? pulse2Volume : pulse2EnvelopeValue;
        return volume / 15.0f;
    }

    private float GetTriangleOutput() {
        if (triangleLengthCounter == 0 || triangleLinearCounter == 0) return 0;
        return TriangleSequence[triangleSequenceIndex] / 15.0f;
    }

    private float GetNoiseOutput() {
        if (noiseLengthCounter == 0) return 0;
        if ((noiseShiftRegister & 0x01) != 0) return 0;
        byte volume = noiseConstantVolume ? noiseVolume : noiseEnvelopeValue;
        return volume / 15.0f;
    }

    private float GetDMCOutput() {
        if (dmcSilence) return 0;
        return dmcOutputLevel / 127.0f;
    }

    // Lookup tables
    private static readonly byte[] LengthCounterTable = new byte[] {
        10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
    };

    private static readonly ushort[] NoisePeriodTable = new ushort[] {
        4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068
    };

    private static readonly byte[,] DutySequences = new byte[,] {
        { 0, 1, 0, 0, 0, 0, 0, 0 }, // 12.5%
        { 0, 1, 1, 0, 0, 0, 0, 0 }, // 25%
        { 0, 1, 1, 1, 1, 0, 0, 0 }, // 50%
        { 1, 0, 0, 1, 1, 1, 1, 1 }  // 75% (negated)
    };

    private static readonly byte[] TriangleSequence = new byte[] {
        15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    };

    private static readonly int[] DMCPeriodTable = new int[] {
        428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106, 84, 72, 54
    };
    
    public byte[] GetAudioSamples() {
        if (sampleBuffer.Count == 0) {
            return Array.Empty<byte>();
        }

        byte[] pcm = new byte[sampleBuffer.Count * 2];
        for (int i = 0; i < sampleBuffer.Count; i++) {
            float input = sampleBuffer[i];

            // Apply high-pass filter to remove DC offset
            float output = HighPassAlpha * (prevOutput + input - prevInput);
            prevInput = input;
            prevOutput = output;

            // Clamp and convert to 16-bit signed PCM
            output = Math.Max(-1.0f, Math.Min(1.0f, output));
            short sample16 = (short)(output * 32000.0f);

            // Little-endian byte order
            pcm[i * 2] = (byte)(sample16 & 0xFF);
            pcm[i * 2 + 1] = (byte)((sample16 >> 8) & 0xFF);
        }

        sampleBuffer.Clear();
        return pcm;
    }
}
