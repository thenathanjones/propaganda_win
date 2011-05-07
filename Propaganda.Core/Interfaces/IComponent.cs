using System;
namespace Propaganda.Core.Interfaces
{
    /// <summary>
    /// Interface describing the basic functionality of a <b>Propaganda</b> component
    /// </summary>
    public interface IComponent : IDisposable
    {
        /// <summary>
        /// Return the name used to identify this component
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        /// Start this component
        /// </summary>
        void Initialise();
    }
}