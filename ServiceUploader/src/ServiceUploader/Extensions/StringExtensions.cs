using System.Security.Cryptography;
using System.Text;

namespace ServiceUploader.Extensions;

public static class StringExtensions
{
    public static string ToHash(this string str)
    {
        return Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(str)));
    }
}