namespace Domorama.PictureFrame.ImageProcessing.Geocoding.Model;

public record GeoCoordinates(double Latitude, double Longitude)
{
    /// <summary>
    /// Converts a decimal degree angle into its corresponding DMS (degrees-minutes-seconds) representation as a string,
    /// of format:
    /// <c>-1° 23' 4.56"</c>
    /// </summary>
    public static string DecimalToDegreesMinutesSecondsString(double value)
    {
        var dms = DecimalToDegreesMinutesSeconds(value);
        return $"{dms[0]:0.##}\u00b0 {dms[1]:0.##}' {dms[2]:0.##}\"";
    }

    /// <summary>
    /// Converts a decimal degree angle into its corresponding DMS (degrees-minutes-seconds) component values, as
    /// a double array.
    /// </summary>
    public static double[] DecimalToDegreesMinutesSeconds(double value)
    {
        var d = (int)value;
        var m = Math.Abs((value % 1) * 60);
        var s = (m % 1) * 60;
        return [d, (int)m, s];
    }


    /// <returns>
    /// Returns a string representation of this object, of format:
    /// <c>1.23, 4.56</c>
    /// </returns>
    public override string ToString() => Latitude + ", " + Longitude;

    /// <returns>
    /// a string representation of this location, of format:
    /// <c>-1° 23' 4.56", 54° 32' 1.92"</c>
    /// </returns>
    public string ToDmsString() => DecimalToDegreesMinutesSecondsString(Latitude) + ", " + DecimalToDegreesMinutesSecondsString(Longitude);
}