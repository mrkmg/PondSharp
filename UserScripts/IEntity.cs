
// ReSharper disable UnusedMemberInSuper.Global

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
    }
}