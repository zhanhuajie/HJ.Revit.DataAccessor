namespace HJ.Revit
{
    internal static class GuidHelper
    {
        public static Guid Xor(this Guid @this, Guid guid)
        {
            var gb1 = @this.ToByteArray();
            var gb2 = guid.ToByteArray();
            var gb3 = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                gb3[i] = (byte)(gb1[i] ^ gb2[i]);
            }
            return new Guid(gb3);
        }
    }
}
