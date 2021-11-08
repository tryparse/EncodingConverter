using System.Text;

namespace EncodingConverter.Core
{
    public class Converter : IEncodingConverter
    {
        public byte[] Convert(Encoding source, Encoding destination, byte[] data)
        {
            return Encoding.Convert(source, destination, data);
        }
    }
}
