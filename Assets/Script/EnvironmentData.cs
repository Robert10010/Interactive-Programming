using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

//Stores one environment state. The lowercase fields are serialized (Inspector / JSON),
//the capitalized properties are what UI Toolkit data binding reads and writes.
[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/EnvironmentData")]
public class EnvironmentData : ScriptableObject, INotifyBindablePropertyChanged
{
    [SerializeField] bool rock = true;
    [SerializeField] bool grass = true;
    [SerializeField] bool flower = true;
    [SerializeField] bool campFire = true;
    [SerializeField] bool fence = true;
    [SerializeField] bool plant = true;
    [SerializeField] bool wood = true;
    [SerializeField] float fireSize = 1f;
    [SerializeField] float fireIntensity = 1f;
    [SerializeField] float sunIntensity = 0.78f;
    [SerializeField] float sunRotation = 18.5f;
    [SerializeField] string skyTheme = "Day";

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    [CreateProperty] public bool Rock { get => rock; set => setField(ref rock, value); }
    [CreateProperty] public bool Grass { get => grass; set => setField(ref grass, value); }
    [CreateProperty] public bool Flower { get => flower; set => setField(ref flower, value); }
    [CreateProperty] public bool CampFire { get => campFire; set => setField(ref campFire, value); }
    [CreateProperty] public bool Fence { get => fence; set => setField(ref fence, value); }
    [CreateProperty] public bool Plant { get => plant; set => setField(ref plant, value); }
    [CreateProperty] public bool Wood { get => wood; set => setField(ref wood, value); }
    [CreateProperty] public float FireSize { get => fireSize; set => setField(ref fireSize, value); }
    [CreateProperty] public float FireIntensity { get => fireIntensity; set => setField(ref fireIntensity, value); }
    [CreateProperty] public float SunIntensity { get => sunIntensity; set => setField(ref sunIntensity, value); }
    [CreateProperty] public float SunRotation { get => sunRotation; set => setField(ref sunRotation, value); }
    [CreateProperty] public string SkyTheme { get => skyTheme; set => setField(ref skyTheme, value); }

    void setField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value))
            return;
        field = value;
        notify(propertyName);
    }

    void notify(string propertyName)
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(propertyName));
    }

    void notifyAll()
    {
        string[] names = { nameof(Rock), nameof(Grass), nameof(Flower), nameof(CampFire), nameof(Fence), nameof(Plant), nameof(Wood),
            nameof(FireSize), nameof(FireIntensity), nameof(SunIntensity), nameof(SunRotation), nameof(SkyTheme) };
        foreach (string n in names)
            notify(n);
    }

    //Copy every value from another EnvironmentData
    public void setData(EnvironmentData data)
    {
        loadFromJson(data.getSaveJson());
    }

    public string getSaveJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void loadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
        notifyAll();
    }
}
