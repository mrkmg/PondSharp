using System;

namespace PondSharp.UserScripts
{
    /// <summary>
    /// Defines default parameters for the entities creation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PondUserSpawnableAttribute : Attribute
    {
        /// <summary>
        /// Default count for the "Create Entity" button on screen.
        /// </summary>
        public int NewCount = 1;
        
        /// <summary>
        /// Number of entities to create at simulation start.
        /// </summary>
        public int InitialCount = 0;

        /// <summary>
        /// Override the name displayed in the entity selection list.
        /// </summary>
        public string Name;
    }
}