#pragma warning disable CS1591
namespace DaLion.Shared.Integrations.SpaceCore;

#region using directives

using System.Reflection;

#endregion using directives

/// <summary>The API provided by SpaceCore.</summary>
public interface ISpaceCoreApi
{
    string[] GetCustomSkills();

    int GetLevelForCustomSkill(Farmer farmer, string skill);

    void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);

    int GetProfessionId(string skill, string profession);

    void AddEventCommand(string command, MethodInfo info);

    void RegisterSerializerType(Type type);

    void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter);

    void RegisterCustomLocationContext(string name, Func<Random, LocationWeather> getLocationWeatherForTomorrowFunc);
}
