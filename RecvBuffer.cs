class RecvBuffer
{
    byte[] _buffer;

    int _readPos;
    int _writePos;

    int _dataSize => _writePos - _readPos;
    int _freeSize => _buffer.Length - _writePos;

    public ArraySegment<byte> ReadSegment => new(_buffer, _readPos, _dataSize);
    public ArraySegment<byte> WriteSegment
    {
        get
        {
            if (_freeSize < 1 << 10)
                Clean();

            return new(_buffer, _writePos, _freeSize);
        }
    }

    public RecvBuffer(int bufferSize = 1 << 16)
    {
        _buffer = new byte[bufferSize];
        _readPos = _writePos = 0;
    }

    public bool OnRead(int readCount)
    {
        if (_dataSize < readCount)
            return false;
        _readPos += readCount;
        return true;
    }

    public bool OnWrite(int writeCount)
    {
        if (_freeSize < writeCount)
            return false;
        _writePos += writeCount;
        return true;
    }

    void Clean()
    {
        int dataSize = _dataSize;

        if (dataSize == 0)
        {
            _readPos = _writePos = 0;
        }
        else
        {
            Array.Copy(_buffer, _readPos, _buffer, 0, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }
}