using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

//Gets real-world data from two Web APIs and maps it onto the EnvironmentData:
//  Open-Meteo     -> day/night, cloud cover, wind, temperature, weather code
//  Sunrise-Sunset -> sun position between sunrise and sunset
public class WeatherController : MonoBehaviour
{
    [Serializable]
    public class Location
    {
        public string name;
        public float latitude;
        public float longitude;
    }

    public UIDocument uiDocument;
    public NetworkManager networkManager;
    public EnvUIController envUIController;
    public float refreshSeconds = 600f;
    public Location[] locations =
    {
        new Location { name = "Taipei", latitude = 25.05f, longitude = 121.53f },
        new Location { name = "Tokyo", latitude = 35.6895f, longitude = 139.6917f },
        new Location { name = "Reykjavik", latitude = 64.1466f, longitude = -21.9426f },
        new Location { name = "Wellington", latitude = -41.2866f, longitude = 174.7756f },
    };

    DropdownField locationDropdown;
    Label weatherLabel, temperatureLabel, cloudLabel, windLabel, sunLabel, infoLabel;
    Toggle useWeatherToggle, autoRefreshToggle;
    Button refreshButton;

    Location currentLocation;
    SunTimeData sunData;
    float nextRefreshTime;
    bool isRequesting;

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;

        locationDropdown = root.Q<DropdownField>("LocationDropdown");
        weatherLabel = root.Q<Label>("WeatherLabel");
        temperatureLabel = root.Q<Label>("TemperatureLabel");
        cloudLabel = root.Q<Label>("CloudLabel");
        windLabel = root.Q<Label>("WindLabel");
        sunLabel = root.Q<Label>("SunLabel");
        infoLabel = root.Q<Label>("InfoLabel");
        useWeatherToggle = root.Q<Toggle>("UseWeatherToggle");
        autoRefreshToggle = root.Q<Toggle>("AutoRefreshToggle");
        refreshButton = root.Q<Button>("RefreshButton");

        List<string> names = new List<string>();
        foreach (Location l in locations)
            names.Add(l.name);
        locationDropdown.choices = names;
        locationDropdown.SetValueWithoutNotify(names[0]);
        currentLocation = locations[0];

        locationDropdown.RegisterValueChangedCallback(evt =>
        {
            currentLocation = locations[locationDropdown.index];
            refresh();
        });
        useWeatherToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
                refresh();
        });
        refreshButton.clicked += refresh;

        refresh();
    }

    void Update()
    {
        if (autoRefreshToggle.value && Time.time >= nextRefreshTime)
            refresh();
    }

    public void refresh()
    {
        nextRefreshTime = Time.time + refreshSeconds;
        if (!isRequesting)
            StartCoroutine(requestAll());
    }

    IEnumerator requestAll()
    {
        isRequesting = true;
        Location location = currentLocation;
        infoLabel.text = "Loading " + location.name + "...";

        string lat = location.latitude.ToString(CultureInfo.InvariantCulture);
        string lng = location.longitude.ToString(CultureInfo.InvariantCulture);

        //1. Sunrise-Sunset first, so the weather mapping can use the sun position
        sunData = null;
        string sunUrl = "https://api.sunrise-sunset.org/json?lat=" + lat + "&lng=" + lng + "&formatted=0";
        yield return networkManager.SendRequest(sunUrl,
            json => sunData = JsonUtility.FromJson<SunTimeData>(json),
            error => Debug.LogWarning("Sun API error: " + error));

        //2. Open-Meteo current weather
        string weatherUrl = "https://api.open-meteo.com/v1/forecast?latitude=" + lat + "&longitude=" + lng +
            "&current=temperature_2m,is_day,weather_code,cloud_cover,wind_speed_10m&timezone=auto";
        OpenMeteoData weather = null;
        string weatherError = null;
        yield return networkManager.SendRequest(weatherUrl,
            json => weather = JsonUtility.FromJson<OpenMeteoData>(json),
            error => weatherError = error);

        isRequesting = false;

        if (weather == null || weather.current == null)
        {
            infoLabel.text = "Weather API error: " + weatherError + "\n(Try another network if this keeps failing.)";
            yield break;
        }

        showWeather(weather);
        if (useWeatherToggle.value)
            applyWeather(weather);

        infoLabel.text = "Updated " + DateTime.Now.ToString("HH:mm:ss") + " (" + location.name + ")";
    }

    void showWeather(OpenMeteoData data)
    {
        OpenMeteoCurrentData c = data.current;
        weatherLabel.text = "Weather: " + describeWeather(c.weather_code) + (c.is_day == 1 ? " (Day)" : " (Night)");
        temperatureLabel.text = "Temperature: " + c.temperature_2m.ToString("0.0") + " °C";
        cloudLabel.text = "Cloud: " + c.cloud_cover + " %";
        windLabel.text = "Wind: " + c.wind_speed_10m.ToString("0.0") + " km/h";

        if (tryGetSunTimes(out DateTime sunrise, out DateTime sunset))
        {
            //Show the times in the location's own time zone
            TimeSpan offset = TimeSpan.FromSeconds(data.utc_offset_seconds);
            sunLabel.text = "Sun: " + (sunrise + offset).ToString("HH:mm") + " - " + (sunset + offset).ToString("HH:mm");
        }
        else
        {
            sunLabel.text = "Sun: -";
        }
    }

    //Data mapping: what the real-world data means for our environment
    void applyWeather(OpenMeteoData data)
    {
        OpenMeteoCurrentData c = data.current;
        EnvironmentData env = envUIController.CurrentEnvData;
        float cloud = c.cloud_cover / 100f;

        //Sunrise / Sunset -> sun rotation (0 at sunrise, 180 at sunset)
        float dayProgress = -1f;
        if (tryGetSunTimes(out DateTime sunrise, out DateTime sunset))
        {
            dayProgress = (float)((DateTime.UtcNow - sunrise).TotalSeconds / (sunset - sunrise).TotalSeconds);
            env.SunRotation = Mathf.Clamp01(dayProgress) * 180f;
        }

        if (c.is_day == 0)
        {
            //Night -> night sky and weak light
            env.SkyTheme = "Night";
            env.SunIntensity = 0.1f;
        }
        else
        {
            //The last 10% of the day counts as sunset
            env.SkyTheme = dayProgress > 0.9f ? "Sunset" : "Day";
            //Cloud cover -> sun intensity (more cloud, darker)
            env.SunIntensity = Mathf.Lerp(1.3f, 0.3f, cloud);
        }

        //Rain, snow or thunderstorm (WMO code 51 and above) puts the camp fire out;
        //otherwise it burns at night or when it is cool
        bool wet = c.weather_code >= 51;
        env.CampFire = !wet && (c.is_day == 0 || c.temperature_2m < 20f);

        //Wind -> fire intensity, temperature -> fire size (colder, bigger fire)
        env.FireIntensity = Mathf.Clamp(0.5f + c.wind_speed_10m / 10f, 0.5f, 5f);
        env.FireSize = Mathf.Lerp(2.5f, 0.8f, Mathf.InverseLerp(0f, 30f, c.temperature_2m));
    }

    bool tryGetSunTimes(out DateTime sunrise, out DateTime sunset)
    {
        sunrise = sunset = default;
        if (sunData == null || sunData.results == null || sunData.status != "OK")
            return false;

        DateTimeStyles style = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
        return DateTime.TryParse(sunData.results.sunrise, CultureInfo.InvariantCulture, style, out sunrise)
            && DateTime.TryParse(sunData.results.sunset, CultureInfo.InvariantCulture, style, out sunset);
    }

    //WMO weather interpretation codes used by Open-Meteo
    static string describeWeather(int code)
    {
        if (code == 0) return "Clear";
        if (code <= 3) return "Cloudy";
        if (code <= 48) return "Fog";
        if (code <= 57) return "Drizzle";
        if (code <= 67) return "Rain";
        if (code <= 77) return "Snow";
        if (code <= 82) return "Showers";
        if (code <= 86) return "Snow showers";
        return "Thunderstorm";
    }
}
