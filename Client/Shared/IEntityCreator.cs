using PondSharp.UserScripts;

namespace PondSharp.Client.Shared;

public interface IEntityCreator
{
    T CreateEntity<T>() where T : Entity;
    Entity CreateEntity(string fullyQualifiedName);
    Entity CreateSelectedEntity();
}