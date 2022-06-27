using PondSharp.UserScripts;

namespace PondSharp.Client;

public interface IEntityCreator
{
    T CreateEntity<T>() where T : Entity;
    Entity CreateEntity(string fullyQualifiedName);
    Entity CreateSelectedEntity();
}