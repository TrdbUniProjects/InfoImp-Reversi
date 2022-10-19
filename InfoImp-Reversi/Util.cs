using System.Numerics;
using System.Text.RegularExpressions;

namespace InfoImp_Reversi; 

public static class Util {

    private static readonly Regex IntRegex = new Regex("^\\d*$");
    
    public static bool IsValidInt(String s) {
        if (!IntRegex.IsMatch(s)) return false;

        BigInteger bi = BigInteger.Parse(s);
        // Keeping the if statement to improve readablitity
        if (bi > int.MaxValue) return false;

        return true;
    }
}