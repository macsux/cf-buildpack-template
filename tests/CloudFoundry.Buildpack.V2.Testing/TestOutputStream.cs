using System.Text;

namespace CloudFoundry.Buildpack.V2.Testing;

public class TestOutputStream : Stream
{
    readonly ITestOutputHelper _out;

    readonly StringBuilder _sb = new();
    long _length = 0;


    public TestOutputStream(ITestOutputHelper outputHelper)
    {
        _out = outputHelper;
    }

    public override void Flush()
    {
        
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _length += count;
        Position += count;
        var newData = new Span<byte>(buffer, offset, count);
        for(int i=0;i<newData.Length;i++)
        {
            var c = newData[i];
            if((char)c == '\n')
            {
                var text = _sb.ToString();
                _out.WriteLine(text);
                _sb.Clear();
            }
            else if((char)c != '\r')
            {
                _sb.Append((char)c);
            }
        }

    }


    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;

    public override long Length => _length;

    public override long Position { get; set; }
}