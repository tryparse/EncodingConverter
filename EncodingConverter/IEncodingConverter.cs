using System.Text;

namespace EncodingConverter.Core
{
    public interface IEncodingConverter
    {
        byte[] Convert(Encoding source, Encoding destination, byte[] data);
    }
}
