using System.Numerics;
using System.Text.RegularExpressions;

namespace InfoImp_Reversi; 

public static class Util {

    private static readonly Regex IntRegex = new Regex("^\\d*$");
    
    public static bool IsValidInt(string s) {
        if (!IntRegex.IsMatch(s)) return false;

        BigInteger bi = BigInteger.Parse(s);
        return bi <= int.MaxValue;
    }
}