using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    /// <summary>
    /// Base Entity Interface
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Id of the entity
        /// </summary>
        int Id { get; }
        
        /// <summary>
        /// Current World X Position
        /// </summary>
        int X { get; }
        
        /// <summary>
        /// Current World Y Position
        /// </summary>
        int Y { get; }
        
        /// <summary>
        /// Current Color
        /// </summary>
        int Color { get; }
        
        /// <summary>
        /// Distance the entity can see other entities
        /// </summary>
        int ViewDistance { get; }
        
        /// <summary>
        /// Minimum X of World
        /// </summary>
        int WorldMinX { get; }
        
        /// <summary>
        /// Maximum X of World
        /// </summary>
        int WorldMaxX { get; }
        
        /// <summary>
        /// Minimum Y of World
        /// </summary>
        int WorldMinY { get; }
        
        /// <summary>
        /// Maximum Y of World
        /// </summary>
        int WorldMaxY { get; }
        
        /// <summary>
        /// Entities which are within the ViewDistance
        /// </summary>
        /// <see cref="ViewDistance"/>
        IEnumerable<IEntity> VisibleEntities { get; }
        
        /// <summary>
        /// Not to be called except by the engine. Will likely fail.
        /// <exception cref="AlreadyInitializedException">Entity was already initialized.</exception>
        /// </summary>
        void Initialize(int id, IEngine engine, int x = 0, int y = 0, int color = 0xFFFFFF, int viewDistance = 0);
        
        /// <summary>
        /// Checks if the entity can move to a World Position
        /// </summary>
        /// <param name="x">World X Position</param>
        /// <param name="y">World Y Position</param>
        /// <returns>If the entity can move to the World Position</returns>
        bool CanMoveTo(int x, int y);
        
        /// <summary>
        /// Tries to move the entity to the World Position
        /// </summary>
        /// <param name="x">World X Position</param>
        /// <param name="y">World Y Position</param>
        /// <returns>If the entity moved to the World Position</returns>
        bool MoveTo(int x, int y);
        
        /// <summary>
        /// Checks if entity can change its color to the specified color.
        /// </summary>
        /// <param name="color">Desired Color</param>
        /// <returns>If the entity can change its color to the specified color.</returns>
        bool CanChangeColorTo(int color);
        
        /// <summary>
        /// Tries to change the entity's color to the specified color.
        /// </summary>
        /// <param name="color">Desired Color</param>
        /// <returns>If the entity's color was changed to the specified color.</returns>
        bool ChangeColor(int color);

        /// <summary>
        /// Checks if entity can changes its view distance to the specified distance.
        /// </summary>
        /// <param name="distance">Distance that visible entities will use.</param>
        /// <returns>If the entity can change its view distance to the specified distance.</returns>
        /// 
        bool CanChangeViewDistance(int distance);
        
        /// <summary>
        /// Tries to change the entity's view distance to the specified distance.
        /// </summary>
        /// <param name="distance">Distance that visible entities will use.</param>
        /// <returns>If the entity's view distance was change the specified distance.</returns>
        bool ChangeViewDistance(int distance);
        
        /// <summary>
        /// Called immediately after the Entity is created and loaded into the world.
        ///
        /// Useful for initialization tasks.
        /// </summary>
        void OnCreated();
        
        /// <summary>
        /// Called before the Entity is destroyed and removed from the world.
        /// </summary>
        void OnDestroy();
        
        /// <summary>
        /// Called on every tick.
        /// </summary>
        void Tick();
    }

    /// <summary>
    /// Entity is already intialized.
    /// </summary>
    public class AlreadyInitializedException : InvalidOperationException
    {
    }
}