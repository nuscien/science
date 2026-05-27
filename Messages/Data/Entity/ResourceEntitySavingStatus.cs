using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trivial.Reflection;

namespace Trivial.Data;

/// <summary>
/// The state of saving resource entity.
/// </summary>
public class ResourceEntitySavingStatus : BaseObservableProperties
{
    /// <summary>
    /// Initializes a new instance of the ResourceEntitySavingStatus class.
    /// </summary>
    internal protected ResourceEntitySavingStatus()
    {
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public ResourceEntitySavingStates State
    {
        get => GetCurrentProperty<ResourceEntitySavingStates>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the date time of this saving status.
    /// </summary>
    public DateTime Time
    {
        get => GetCurrentProperty<DateTime>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets the additional message about this saving status.
    /// </summary>
    public string Message
    {
        get => GetCurrentProperty<string>();
        private set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Updates the saving status of this entity.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <param name="message">The additional message about this saving status.</param>
    public virtual void Update(ResourceEntitySavingStates state, string message = null)
    {
        State = state;
        Message = message?.Trim();
        Time = DateTime.Now;
    }
}
