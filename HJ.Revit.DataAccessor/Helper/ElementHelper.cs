namespace HJ.Revit
{
    internal static class ElementHelper
    {
        public static Guid GetGuid(this Element elem)
        {
            var g1 = new Guid(MD5Helper.GetMD5Hash(elem.Document.GetHashCode().ToString()));
            var g2 = new Guid(MD5Helper.GetMD5Hash(elem.UniqueId));
            return g1.Xor(g2);
        }
    }
}
