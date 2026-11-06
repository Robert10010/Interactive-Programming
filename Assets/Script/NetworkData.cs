using System;

//Open-Meteo current weather: https://open-meteo.com/en/docs
[Serializable]
public class OpenMeteoCurrentData
{
    public string time;
    public float temperature_2m;
    public int is_day;
    public int weather_code;
    public int cloud_cover;
    public float wind_speed_10m;
}

[Serializable]
public class OpenMeteoData
{
    public float latitude;
    public float longitude;
    public int utc_offset_seconds;
    public OpenMeteoCurrentData current;
}

//Sunrise-Sunset: https://sunrise-sunset.org/api (formatted=0 returns ISO 8601 times in UTC)
[Serializable]
public class SunTimeResults
{
    public string sunrise;
    public string sunset;
    public string solar_noon;
}

[Serializable]
public class SunTimeData
{
    public SunTimeResults results;
    public string status;
}
