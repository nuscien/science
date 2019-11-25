﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Text;

namespace Trivial.Reflection
{
    /// <summary>
    /// Base model with observable properties.
    /// </summary>
    public abstract class BaseObservableProperties : INotifyPropertyChanged
    {
        /// <summary>
        /// Data cache.
        /// </summary>
        private readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        /// <summary>
        /// Adds or removes the event handler raised on property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="key">The additional key.</param>
        /// <returns>A property value.</returns>
        protected T GetCurrentProperty<T>(T defaultValue = default, [CallerMemberName] string key = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return defaultValue;
            try
            {
                return cache.TryGetValue(key, out var v) ? (T)v : defaultValue;
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ArgumentException)
            {
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="key">The additional key.</param>
        /// <returns>true if set succeeded; otherwise, false.</returns>
        protected bool SetCurrentProperty(object value, [CallerMemberName] string key = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (cache.TryGetValue(key, out var v) && v == value) return false;
            cache[key] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            return true;
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A property value.</returns>
        protected T GetProperty<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(key)) return defaultValue;
            try
            {
                return cache.TryGetValue(key, out var v) ? (T)v : defaultValue;
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
            catch (ArgumentException)
            {
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if set succeeded; otherwise, false.</returns>
        protected bool SetProperty(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            if (cache.TryGetValue(key, out var v) && v == value) return false;
            cache[key] = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            return true;
        }

        /// <summary>
        /// Removes a property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false.</returns>
        protected bool RemoveProperty(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            var result = cache.Remove(key);
            if (result) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
            return result;
        }

        /// <summary>
        /// Forces rasing the property changed notification.
        /// </summary>
        /// <param name="key">The property key.</param>
        protected void ForceNotify(string key)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }
    }

    /// <summary>
    /// The model with observable name and value.
    /// </summary>
    [DataContract]
    public class NameValueObservableProperties<T> : BaseObservableProperties
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonPropertyName("name")]
        public string Name
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [DataMember(Name = "value")]
        [JsonPropertyName("value")]
        public T Value
        {
            get => GetCurrentProperty<T>();
            set => SetCurrentProperty(value);
        }
    }
}
