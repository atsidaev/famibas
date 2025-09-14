public interface IKeyboard
{
    // Read data from keyboard (port $4017)
    public byte Read();
    
    // Write data to keyboard (port $4016)
    public void Write(byte value);
}