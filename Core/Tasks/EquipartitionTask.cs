﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

using Trivial.Data;
using Trivial.Net;
using Trivial.Reflection;
using Trivial.Text;
using Trivial.Web;

namespace Trivial.Tasks
{
    /// <summary>
    /// Equipartition fragment task and its thread-safe resolver.
    /// </summary>
    public class EquipartitionTask : IReadOnlyList<EquipartitionTask.Fragment>
    {
        /// <summary>
        /// Task fragment states.
        /// </summary>
        public enum FragmentStates
        {
            /// <summary>
            /// Pending to assign.
            /// </summary>
            Pending = 0,

            /// <summary>
            /// Working on.
            /// </summary>
            Working = 1,

            /// <summary>
            /// Run succeeded.
            /// </summary>
            Success = 2,

            /// <summary>
            /// Run failed.
            /// </summary>
            Failure = 3,

            /// <summary>
            /// Retrying to process.
            /// </summary>
            Retrying = 4,

            /// <summary>
            /// Fatal.
            /// </summary>
            Fatal = 5,

            /// <summary>
            /// Canceled or aborted.
            /// </summary>
            Ignored = 6
        }

        /// <summary>
        /// Task fragment model for serialization only.
        /// </summary>
        [DataContract]
        internal class FragmentModel
        {
            /// <summary>
            /// Gets or sets the task fragment identifier.
            /// </summary>
            [DataMember(Name = "id")]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the fragment index in the task.
            /// </summary>
            [DataMember(Name = "index")]
            public int? Index { get; set; }

            /// <summary>
            /// Gets or sets the task fragment state.
            /// </summary>
            [DataMember(Name = "state")]
            public string State { get; set; }

            /// <summary>
            /// Gets or sets the tag.
            /// </summary>
            [DataMember(Name = "tag", EmitDefaultValue = false)]
            public string Tag { get; set; }

            /// <summary>
            /// Gets or sets the creation date time tick.
            /// </summary>
            [DataMember(Name = "creation")]
            public long? Creation { get; set; }

            /// <summary>
            /// Gets or sets the latest modification date time tick.
            /// </summary>
            [DataMember(Name = "update")]
            public long? Modification { get; set; }
        }

        /// <summary>
        /// Task fragment.
        /// </summary>
        public class Fragment : ISerializable
        {
            /// <summary>
            /// Initializes a new instance of the EquipartitionTask.Fragment class.
            /// </summary>
            /// <param name="index">The fragment index in the task.</param>
            public Fragment(int index)
            {
                Id = Guid.NewGuid().ToString("n");
                Index = index;
            }

            /// <summary>
            /// Initializes a new instance of the EquipartitionTask.Fragment class.
            /// </summary>
            /// <param name="id">The task fragment identifier.</param>
            /// <param name="index">The task fragment index in the task.</param>
            /// <param name="state">The task fragemnt state.</param>
            /// <param name="creation">The creation date time.</param>
            /// <param name="modification">The latest modification date time.</param>
            /// <param name="tag">The tag.</param>
            public Fragment(string id, int index, FragmentStates state, DateTime? creation, DateTime? modification, string tag = null)
            {
                Id = id ?? Guid.NewGuid().ToString("n");
                Index = index;
                Tag = tag;
                State = state;
                if (creation.HasValue) Creation = creation.Value;
                if (modification.HasValue) Modification = modification.Value;
            }

            /// <summary>
            /// Initializes a new instance of the Fragment class.
            /// </summary>
            /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
            /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
            protected Fragment(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    Id = Guid.NewGuid().ToString("n");
                    return;
                }

                try
                {
                    var id = info.GetString(nameof(Id));
                    if (id != null) Id = id;
                }
                catch (SerializationException)
                {
                }

                try
                {
                    Index = info.GetInt32(nameof(Index));
                }
                catch (SerializationException)
                {
                }

                try
                {
                    var stateStr = info.GetString(nameof(State));
                    if (!string.IsNullOrWhiteSpace(stateStr) && Enum.TryParse(stateStr, true, out FragmentStates state)) State = state;
                }
                catch (SerializationException)
                {
                }

                try
                {
                    Tag = info.GetString(nameof(Tag));
                }
                catch (SerializationException)
                {
                }

                try
                {
                    Creation = WebFormat.ParseDate(info.GetInt64(nameof(Creation)));
                }
                catch (SerializationException)
                {
                }

                try
                {
                    Modification = WebFormat.ParseDate(info.GetInt64(nameof(Modification)));
                }
                catch (SerializationException)
                {
                }
            }

            /// <summary>
            /// Gets the task fragment identifier.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Gets the fragment index in the task.
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// Gets the task fragment state.
            /// </summary>
            public FragmentStates State { get; internal set; } = FragmentStates.Pending;

            /// <summary>
            /// Gets a value indicating whether this task fragment is working or retrying.
            /// </summary>
            public bool IsProcessing => State == FragmentStates.Working || State == FragmentStates.Retrying;

            /// <summary>
            /// Gets a value indicating whether this task fragment is done (even if it is any of success, fatal or ignored).
            /// </summary>
            public bool IsDone => State == FragmentStates.Success || State == FragmentStates.Fatal || State == FragmentStates.Ignored;

            /// <summary>
            /// Gets a value indicating whether this task fragment is pending or failure.
            /// </summary>
            public bool IsWaiting => State == FragmentStates.Pending || State == FragmentStates.Failure;

            /// <summary>
            /// Gets a value indicating whether this task fragment is fatal or failure (even if it is either waiting or done).
            /// </summary>
            public bool IsError => State == FragmentStates.Failure || State == FragmentStates.Fatal;

            /// <summary>
            /// Gets a value indicating whether this task fragment is done but not successful (even if it is either waiting or done).
            /// </summary>
            public bool IsErrorOrIgnored => State == FragmentStates.Failure || State == FragmentStates.Fatal || State == FragmentStates.Ignored;

            /// <summary>
            /// Gets the tag.
            /// </summary>
            public string Tag { get; internal set; }

            /// <summary>
            /// Gets the creation date time.
            /// </summary>
            public DateTime Creation { get; } = DateTime.Now;

            /// <summary>
            /// Gets the latest modification date time.
            /// </summary>
            public DateTime Modification { get; internal set; } = DateTime.Now;

            /// <summary>
            /// Sets the System.Runtime.Serialization.SerializationInfo object with the parameter name and additional exception information.
            /// </summary>
            /// <param name="info">The object that holds the serialized object data.</param>
            /// <param name="context">The contextual information about the source or destination.</param>
            public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null) return;
                info.AddValue(nameof(Id), Id, typeof(string));
                info.AddValue(nameof(Index), Index, typeof(int));
                info.AddValue(nameof(State), State.ToString().ToLowerInvariant(), typeof(string));
                if (Tag != null) info.AddValue(nameof(Tag), Tag, typeof(string));
                info.AddValue(nameof(Creation), WebFormat.ParseDate(Creation), typeof(long));
                info.AddValue(nameof(Modification), WebFormat.ParseDate(Modification), typeof(long));
            }

            /// <summary>
            /// Converts to JSON format string.
            /// </summary>
            /// <returns>A JSON format string.</returns>
            public virtual string ToJsonString()
            {
                return ToJsonString(null as JsonSerializerOptions);
            }

            /// <summary>
            /// Converts to query data.
            /// </summary>
            /// <returns>A query data instance.</returns>
            public virtual QueryData ToQueryData()
            {
                var q = new QueryData
                {
                    [nameof(Id)] = Id,
                    [nameof(Index)] = Index.ToString(CultureInfo.InvariantCulture),
                    [nameof(State)] = State.ToString().ToLowerInvariant()
                };
                if (Tag != null) q[nameof(Tag)] = Tag;
                q[nameof(Creation)] = WebFormat.ParseDate(Creation).ToString(CultureInfo.InvariantCulture);
                q[nameof(Modification)] = WebFormat.ParseDate(Modification).ToString(CultureInfo.InvariantCulture);
                return q;
            }

            /// <summary>
            /// Converts to JSON format string.
            /// </summary>
            /// <param name="options">The data contract serializer settings.</param>
            /// <returns>A JSON format string.</returns>
            public virtual string ToJsonString(JsonSerializerOptions options)
            {
                var m = new FragmentModel
                {
                    Id = Id,
                    Index = Index,
                    State = State.ToString().ToLowerInvariant(),
                    Tag = Tag,
                    Creation = WebFormat.ParseDate(Creation),
                    Modification = WebFormat.ParseDate(Modification)
                };
                return StringExtensions.ToJson(m, options);
            }

            /// <summary>
            /// Converts to JSON format string.
            /// </summary>
            /// <param name="options">The data contract serializer settings.</param>
            /// <returns>A JSON format string.</returns>
            public virtual string ToJsonString(DataContractJsonSerializerSettings options)
            {
                var m = new FragmentModel
                {
                    Id = Id,
                    Index = Index,
                    State = State.ToString().ToLowerInvariant(),
                    Tag = Tag,
                    Creation = WebFormat.ParseDate(Creation),
                    Modification = WebFormat.ParseDate(Modification)
                };
                return StringExtensions.ToJson(m, options);
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                return string.Format("#{0} [{1}] {2}", Index.ToString(), State.ToString(), Id);
            }

            /// <summary>
            /// Parses from a JSON string.
            /// </summary>
            /// <param name="s">The string to parse.</param>
            public static Fragment Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(nameof(s), "str should not be null or empty.");
                s = s.Trim();
                if (s.IndexOf("<") == 0)
                {
                    var xml = XElement.Parse(s);
                    string id = null;
                    var index = 0;
                    string stateStr = null;
                    string tag = null;
                    long? creation = null;
                    long? modification = null;
                    foreach (var ele in xml.Elements())
                    {
                        if (string.IsNullOrWhiteSpace(ele?.Value)) continue;
                        switch (ele.Name?.LocalName?.ToLowerInvariant())
                        {
                            case "id":
                                id = ele.Value;
                                break;
                            case "index":
                                int.TryParse(ele.Value, out index);
                                break;
                            case "state":
                                stateStr = ele.Value;
                                break;
                            case "tag":
                                tag = ele.Value;
                                break;
                            case "creation":
                                if (long.TryParse(ele.Value, out var creationV)) creation = creationV;
                                break;
                            case "update":
                            case "modification":
                                if (long.TryParse(ele.Value, out var modificationV)) modification = modificationV;
                                break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(stateStr) || !Enum.TryParse(stateStr, true, out FragmentStates state2)) state2 = FragmentStates.Pending;
                    return new Fragment(id, index, state2, WebFormat.ParseDate(creation), WebFormat.ParseDate(modification), tag);
                }

                if (s.IndexOfAny(new[] { '\"', '{' }) < 0)
                {
                    var q = QueryData.Parse(s);
                    var stateStr = q["State"] ?? q["state"];
                    if (!int.TryParse(q["Index"] ?? q["index"], out var index)) index = 0;
                    if (string.IsNullOrWhiteSpace(stateStr) || !Enum.TryParse(stateStr, true, out FragmentStates state2)) state2 = FragmentStates.Pending;
                    long? creation = null;
                    long? modification = null;
                    if (long.TryParse(q["Creation"] ?? q["creation"], out var creationV)) creation = creationV;
                    if (long.TryParse(q["Modification"] ?? q["modification"] ?? q["Update"] ?? q["update"], out var modificationV)) modification = modificationV;
                    return new Fragment(
                        q["Id"] ?? q["ID"] ?? q["id"],
                        index,
                        state2,
                        WebFormat.ParseDate(creation),
                        WebFormat.ParseDate(modification),
                        q["Tag"] ?? q["tag"]
                    );
                }

                var m = StringExtensions.FromJson<FragmentModel>(s);
                if (string.IsNullOrWhiteSpace(m.State) || !Enum.TryParse(m.State, true, out FragmentStates state)) state = FragmentStates.Pending;
                return new Fragment(m.Id, m.Index ?? 0, state, WebFormat.ParseDate(m.Creation), WebFormat.ParseDate(m.Modification), m.Tag);
            }
        }

        /// <summary>
        /// The fragment state changed event arguments.
        /// </summary>
        public class FragmentStateEventArgs : ChangeEventArgs<FragmentStates>
        {
            /// <summary>
            /// Initializes a new instance of the EquipartitionTask.FragmentStateEventArgs class.
            /// </summary>
            /// <param name="instance">The task fragment instance.</param>
            /// <param name="oldState">The old task fragment state.</param>
            public FragmentStateEventArgs(Fragment instance, FragmentStates oldState)
                : base(oldState, instance?.State ?? oldState, instance == null || oldState == instance.State ? ChangeMethods.Same : ChangeMethods.Update, "State")
            {
                Source = instance;
            }

            /// <summary>
            /// Gets the fragment identifier.
            /// </summary>
            public string Id => Source?.Id;

            /// <summary>
            /// Gets the task fragment instance.
            /// </summary>
            public Fragment Source { get; }
        }

        private readonly object locker = new object();
        private readonly List<Fragment> fragments = new List<Fragment>();
        private string desc;

        /// <summary>
        /// Initializes a new instance of the EquipartitionTask class.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="count">The fragment count.</param>
        public EquipartitionTask(string jobId, int count)
        {
            JobId = jobId;
            for (var i = 0; i < count; i++)
            {
                fragments.Add(new Fragment(i));
            }
        }

        /// <summary>
        /// Initializes a new instance of the EquipartitionTask class.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="children">The fragments.</param>
        /// <param name="creation">The creation date time.</param>
        /// <param name="description">The description.</param>
        public EquipartitionTask(string id, string jobId, IEnumerable<Fragment> children, DateTime? creation = null, string description = null)
        {
            if (!string.IsNullOrWhiteSpace(id)) Id = id;
            JobId = jobId;
            desc = description;
            if (creation.HasValue) Creation = creation.Value;
            if (children != null) fragments.AddRange(children.Where(ele => ele != null && !string.IsNullOrWhiteSpace(ele.Id)));
        }

        /// <summary>
        /// Adds or removes the event occurs when the task fragment state or tag is changed.
        /// </summary>
        public event EventHandler<FragmentStateEventArgs> FragmentStateChanged;

        /// <summary>
        /// Adds or removes the event occurs when the task is done.
        /// </summary>
        public event EventHandler HasBeenDone;

        /// <summary>
        /// Adds or removes the event occurs when the description is changed.
        /// </summary>
        public event ChangeEventHandler<string> DescriptionChanged;

        /// <summary>
        /// Gets the task identifier.
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString("n");

        /// <summary>
        /// Gets the job identifier.
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Gets or sets the description of the task.
        /// </summary>
        public string Description
        {
            get
            {
                return desc;
            }

            set
            {
                var oldValue = desc;
                desc = value;
                DescriptionChanged?.Invoke(this, new ChangeEventArgs<string>(oldValue, value, "Description", true));
            }
        }

        /// <summary>
        /// Gets the task fragment count.
        /// </summary>
        public int Count => fragments.Count;

        /// <summary>
        /// Gets a value indicating whether the task is done.
        /// </summary>
        public bool IsDone
        {
            get
            {
                return fragments.FirstOrDefault(ele =>
                {
                    return !ele.IsDone;
                }) == null;
            }
        }

        /// <summary>
        /// Gets the creation date time.
        /// </summary>
        public DateTime Creation { get; } = DateTime.Now;

        /// <summary>
        /// Gets the latest modification date time.
        /// </summary>
        public DateTime Modification
        {
            get
            {
                return fragments.Max(ele => ele.Modification);
            }
        }

        /// <summary>
        /// Gets the specific task fragment.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The fragment instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0, or equals to or greater than the length of fragments.</exception>
        public Fragment this[int index] => fragments[index];

        /// <summary>
        /// Gets the specific task fragment.
        /// </summary>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <returns>The fragment instance.</returns>
        /// <exception cref="ArgumentNullException">fragmentId was null.</exception>
        /// <exception cref="InvalidOperationException">No element matched.</exception>
        public Fragment this[string fragmentId] => fragments.First(ele => ele.Id == fragmentId);

        /// <summary>
        /// Tests if it contains such fragment identifier.
        /// </summary>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool ContainsId(string fragmentId) => fragments.FirstOrDefault(ele => ele.Id == fragmentId) != null;

        /// <summary>
        /// Gets the specific task fragment; or null if no such element.
        /// </summary>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <returns>The fragment instance.</returns>
        public Fragment TryGetByFragmentId(string fragmentId) => fragments.FirstOrDefault(ele => ele.Id == fragmentId);

        /// <summary>
        /// Gets the specific task fragment; or null if no such element.
        /// </summary>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <param name="value">The fragment instance.</param>
        /// <returns>true if contains; otherwise, false.</returns>
        public bool TryGetByFragmentId(string fragmentId, out Fragment value)
        {
            value = TryGetByFragmentId(fragmentId);
            return value != null;
        }

        /// <summary>
        /// Gets the collection of the task fragment by its index.
        /// </summary>
        public IEnumerable<Fragment> GetByFragmentIndex(int index) => fragments.Where(ele => ele.Index == index);

        /// <summary>
        /// Gets the collection of the task fragment which is working or retrying.
        /// </summary>
        public IEnumerable<Fragment> GetProcessingFragments() => fragments.Where(ele => ele.IsProcessing);

        /// <summary>
        /// Gets the collection of the task fragment which is not done yet.
        /// </summary>
        public IEnumerable<Fragment> GetWaitingOrProcessingFragments() => fragments.Where(ele => !ele.IsDone);

        /// <summary>
        /// Gets the collection of the task fragment which is done (even if it is any of success, fatal or ignored).
        /// </summary>
        public IEnumerable<Fragment> GetDoneFragments() => fragments.Where(ele => ele.IsDone);

        /// <summary>
        /// Gets the collection of the task fragment which is pending or failure.
        /// </summary>
        public IEnumerable<Fragment> GetWaitingFragments() => fragments.Where(ele => ele.IsWaiting);

        /// <summary>
        /// Gets the collection of the task fragment which is fatal or failure (even if it is either waiting or done).
        /// </summary>
        public IEnumerable<Fragment> GetErrorFragments() => fragments.Where(ele => ele.IsError);

        /// <summary>
        /// Gets the collection of the task fragment which is done but not successful (even if it is either waiting or done).
        /// </summary>
        public IEnumerable<Fragment> GetErrorOrIngoredFragments() => fragments.Where(ele => ele.IsErrorOrIgnored);

        /// <summary>
        /// Filters a sequence of values with specific state.
        /// </summary>
        /// <param name="state">Specific state.</param>
        /// <param name="states">Additional state grouped by OR boolean operation.</param>
        /// <returns>An collection that contains elements from the fragments that satisfy the condition.</returns>
        public IEnumerable<Fragment> Where(FragmentStates state, params FragmentStates[] states)
        {
            return states == null || states.Length == 0
                ? fragments.Where(ele => ele.State == state)
                : fragments.Where(ele => ele.State == state || states.Contains(ele.State));
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="state">The task fragment state expected.</param>
        /// <returns>A fragment.</returns>
        public Fragment Pick(FragmentStates state) => Pick(null, state, null);

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A fragment.</returns>
        public Fragment Pick(IEnumerable<string> except = null, bool onlyPending = false)
        {
            return onlyPending ? Pick(except, FragmentStates.Pending, null) : Pick(except, null, null);
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="tag">New tag.</param>
        /// <param name="state">The task fragment state expected.</param>
        /// <returns>A fragment.</returns>
        public Fragment Pick(string tag, FragmentStates state) => Pick(null, state, fragment => fragment.Tag = tag);

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="tag">New tag.</param>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A fragment.</returns>
        public Fragment Pick(string tag, IEnumerable<string> except = null, bool onlyPending = false)
        {
            return onlyPending ? Pick(except, FragmentStates.Pending, fragment => fragment.Tag = tag) : Pick(except, null, fragment => fragment.Tag = tag);
        }

        /// <summary>
        /// Updates the task fragment.
        /// </summary>
        /// <param name="id">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <param name="tag">The new tag.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public bool UpdateFragment(string id, FragmentStates? state, string tag) => UpdateFragment(id, state, f =>
        {
            f.Tag = tag;
        });

        /// <summary>
        /// Updates the task fragment.
        /// </summary>
        /// <param name="id">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public bool UpdateFragment(string id, FragmentStates state) => UpdateFragment(id, state, null as Action<Fragment>);

        /// <summary>
        /// Updates the task fragment.
        /// </summary>
        /// <param name="fragment">The task fragment instance.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <param name="tag">The new tag.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public bool UpdateFragment(Fragment fragment, FragmentStates? state, string tag) => UpdateFragment(fragment?.Id, state, f =>
        {
            f.Tag = tag;
            if (fragment == f) return;
            fragment.Modification = f.Modification;
            fragment.State = f.State;
            fragment.Tag = f.Tag;
        });

        /// <summary>
        /// Updates the task fragment.
        /// </summary>
        /// <param name="fragment">The task fragment instance.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public bool UpdateFragment(Fragment fragment, FragmentStates state) => UpdateFragment(fragment?.Id, state, f =>
        {
            if (fragment == f) return;
            fragment.Modification = f.Modification;
            fragment.State = f.State;
            fragment.Tag = f.Tag;
        });

        /// <summary>
        /// Cancels.
        /// </summary>
        public void Cancel()
        {
            List<(Fragment, FragmentStates)> col;
            lock (locker)
            {
                col = GetWaitingOrProcessingFragments().Select(ele => (ele, ele.State)).ToList();
                if (col.Count == 0) return;
                foreach (var fragment in col)
                {
                    fragment.Item1.State = FragmentStates.Ignored;
                    fragment.Item1.Modification = DateTime.Now;
                }
            }

            foreach (var fragment in col)
            {
                FragmentStateChanged?.Invoke(this, new FragmentStateEventArgs(fragment.Item1, fragment.Item2));
            }

            HasBeenDone?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the JSON format string serialized of this object.
        /// </summary>
        /// <returns>A JSON format string.</returns>
        public string ToJsonString() => ToJsonString(true);

        /// <summary>
        /// Gets the JSON format string.
        /// </summary>
        /// <param name="containFragments">true if output all the fragments; otherwise, false.</param>
        /// <param name="additionalFragment">The additional fragment to output.</param>
        /// <param name="additionalProperties">The additional JSON string properties to output.</param>
        /// <returns>A JSON format string.</returns>
        public string ToJsonString(bool containFragments, Fragment additionalFragment = null, IDictionary<string, string> additionalProperties = null)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat(
                CultureInfo.InvariantCulture,
                "\"id\":{0},\"job\":{1},\"creation\":{2},\"update\":{3}\"done\":{4}",
                JsonStringValue.ToJson(Id),
                JsonStringValue.ToJson(JobId),
                WebFormat.ParseDate(Creation),
                WebFormat.ParseDate(Modification),
                IsDone);
            if (desc != null) sb.Append($",\"desc\":{JsonStringValue.ToJson(desc)}");
            if (additionalFragment != null) sb.Append($",\"fragment\":{additionalFragment.ToJsonString()}");
            if (containFragments)
            {
                var jArr = new List<string>();
                foreach (var fragment in fragments)
                {
                    if (fragment != null) jArr.Add(fragment.ToJsonString());
                }

                sb.AppendFormat(",\"fragments\":[{0}]", string.Join(",", jArr));
            }

            if (additionalProperties != null)
            {
                foreach (var kvp in additionalProperties)
                {
                    sb.AppendFormat(",{0}:{1}", JsonStringValue.ToJson(kvp.Key), JsonStringValue.ToJson(kvp.Value));
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("Task ID: {0}. Job ID: {1}. Fragment count: {2}.", Id, JobId, Count);
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="except">The except task fragment identifier.</param>
        /// <param name="state">The task fragment state expected.</param>
        /// <param name="callback">The success callback.</param>
        /// <returns>A fragment.</returns>
        private Fragment Pick(IEnumerable<string> except, FragmentStates? state, Action<Fragment> callback)
        {
            Fragment fragment = null;
            FragmentStates oldState = FragmentStates.Pending;
            var wasDone = IsDone;
            lock (locker)
            {
                IEnumerable<Fragment> list = fragments;
                if (except != null) list = list.Where(ele =>
                {
                    return !except.Contains(ele.Id);
                });
                fragment = state.HasValue ? list.FirstOrDefault(ele =>
                {
                    return ele.State == state;
                }) : list.FirstOrDefault(ele =>
                {
                    return ele.State == FragmentStates.Pending || ele.State == FragmentStates.Failure;
                });
                if (fragment == null) return null;
                switch (oldState = fragment.State)
                {
                    case FragmentStates.Pending:
                        fragment.State = FragmentStates.Working;
                        fragment.Modification = DateTime.Now;
                        break;
                    case FragmentStates.Failure:
                        fragment.State = FragmentStates.Retrying;
                        fragment.Modification = DateTime.Now;
                        break;
                    default:
                        if (state.HasValue) break;
                        return null;
                }

                if (callback != null)
                {
                    fragment.Modification = DateTime.Now;
                    callback(fragment);
                }
            }

            NotifyChange(fragment, oldState, wasDone);
            return fragment;
        }

        /// <summary>
        /// Updates the task fragment.
        /// </summary>
        /// <param name="id">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <param name="callback">The success callback.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        private bool UpdateFragment(string id, FragmentStates? state, Action<Fragment> callback)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            var fragment = TryGetByFragmentId(id);
            if (fragment == null) return false;
            var oldState = fragment.State;
            if (!state.HasValue)
            {
                if (callback != null)
                {
                    fragment.Modification = DateTime.Now;
                    callback(fragment);
                    FragmentStateChanged?.Invoke(this, new FragmentStateEventArgs(fragment, oldState));
                }

                return true;
            }

            var wasDone = IsDone;
            lock (locker)
            {
                if (state != oldState)
                {
                    if (oldState == FragmentStates.Success || oldState == FragmentStates.Fatal || oldState == FragmentStates.Ignored) return false;
                    switch (state.Value)
                    {
                        case FragmentStates.Pending:
                            if (oldState == FragmentStates.Working) break;
                            return false;
                        case FragmentStates.Working:
                        case FragmentStates.Retrying:
                            state = oldState == FragmentStates.Pending ? FragmentStates.Working : FragmentStates.Retrying;
                            break;
                    }

                    fragment.State = state.Value;
                    fragment.Modification = DateTime.Now;
                }
                else
                {
                    fragment.Modification = DateTime.Now;
                }

                callback?.Invoke(fragment);
            }

            NotifyChange(fragment, oldState, wasDone);
            return true;
        }

        private void NotifyChange(Fragment fragment, FragmentStates oldState, bool wasDone)
        {
            if (fragment == null) return;
            FragmentStateChanged?.Invoke(this, new FragmentStateEventArgs(fragment, oldState));
            if (!wasDone && IsDone) HasBeenDone?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns an enumerator that iterates through this instance.
        /// </summary>
        /// <returns>An enumerator for this instance.</returns>
        public IEnumerator<Fragment> GetEnumerator()
        {
            return fragments.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through this instance.
        /// </summary>
        /// <returns>An enumerator for this instance.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return fragments.GetEnumerator();
        }
    }

    /// <summary>
    /// Equipartition task thread-safe container with grouping.
    /// </summary>
    public class EquipartitionTaskContainer
    {
        private readonly object locker = new object();
        private readonly Dictionary<string, List<EquipartitionTask>> cache = new Dictionary<string, List<EquipartitionTask>>();

        /// <summary>
        /// Adds or removes an event handler when create a new task.
        /// </summary>
        public event ChangeEventHandler<EquipartitionTask> Created;

        /// <summary>
        /// Gets the equipartition task list.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <returns>A list of the equipartition task.</returns>
        public IReadOnlyList<EquipartitionTask> this[string group] => (cache.TryGetValue(group, out var list) ? list : new List<EquipartitionTask>()).AsReadOnly();

        /// <summary>
        /// Gets the equipartition task list.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="taskId">The task identifier.</param>
        /// <returns>A list of the equipartition task.</returns>
        public EquipartitionTask this[string group, string taskId] => this[group].First(t => t.Id == taskId);

        /// <summary>
        /// Gets the group identifier list in cache.
        /// </summary>
        public IReadOnlyCollection<string> GroupIds => cache.Keys;

        /// <summary>
        /// Gets all the equipartition tasks available.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="count">The count to take.</param>
        /// <returns>A collection of equipartition task.</returns>
        public IEnumerable<EquipartitionTask> List(string group, int? count = null)
        {
            return List(group, null, count);
        }

        /// <summary>
        /// Gets all the equipartition tasks available.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="count">The count to take.</param>
        /// <returns>A collection of equipartition task.</returns>
        public IEnumerable<EquipartitionTask> List(string group, Func<EquipartitionTask, bool> predicate, int? count = null)
        {
            if (!cache.TryGetValue(group, out var list)) return new List<EquipartitionTask>().AsReadOnly();
            var col = list.Where(ele =>
            {
                return !ele.IsDone;
            });
            if (predicate != null) col = col.Where(predicate);
            return count.HasValue ? col.Take(count.Value) : col;
        }

        /// <summary>
        /// Gets the first equipartition tasks if available.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An equipartition task; or null, if no one available.</returns>
        public EquipartitionTask TryGetFirst(string group, Func<EquipartitionTask, bool> predicate = null)
        {
            return List(group, predicate).FirstOrDefault();
        }

        /// <summary>
        /// Gets the last equipartition tasks if available.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An equipartition task; or null, if no one available.</returns>
        public EquipartitionTask TryGetLast(string group, Func<EquipartitionTask, bool> predicate = null)
        {
            return List(group, predicate).LastOrDefault();
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="pick">A handler to pick.</param>
        /// <returns>A task and fragment.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(string group, Func<EquipartitionTask, EquipartitionTask.Fragment> pick = null)
        {
            return Pick(group, null, pick);
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A task and fragment.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(string group, string tag, IEnumerable<string> except = null, bool onlyPending = false)
        {
            return Pick(group, null, task => task.Pick(tag, except, onlyPending));
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="pick">A handler to pick.</param>
        /// <returns>A task and fragment.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(string group, Func<EquipartitionTask, bool> predicate, Func<EquipartitionTask, EquipartitionTask.Fragment> pick = null)
        {
            var col = List(group, predicate);
            if (pick == null) pick = task => task.Pick();
            foreach (var item in col)
            {
                var f = pick(item);
                if (f == null) continue;
                return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>(item, f);
            }

            return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A task and fragment.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(string group, Func<EquipartitionTask, bool> predicate, string tag, IEnumerable<string> except = null, bool onlyPending = false)
        {
            return Pick(group, predicate, task => task.Pick(tag, except, onlyPending));
        }

        /// <summary>
        /// Creates a new equipartition tasks.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="count">The task fragment count.</param>
        /// <param name="autoRemove">true if remove the item all done automatically; otherwise, false.</param>
        /// <returns>A new equipartition tasks created.</returns>
        public EquipartitionTask Create(string group, string jobId, int count, bool autoRemove = true)
        {
            return Create(group, jobId, count, null, autoRemove);
        }

        /// <summary>
        /// Creates a new equipartition tasks.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="count">The task fragment count.</param>
        /// <param name="description">The description.</param>
        /// <param name="autoRemove">true if remove the item all done automatically; otherwise, false.</param>
        /// <returns>A new equipartition tasks created.</returns>
        public virtual EquipartitionTask Create(string group, string jobId, int count, string description, bool autoRemove = true)
        {
            var task = Create(group, new EquipartitionTask(jobId, count)
            {
                Description = description
            }, autoRemove);
            Created?.Invoke(this, new ChangeEventArgs<EquipartitionTask>(null, task, ChangeMethods.Add, group));
            return task;
        }

        /// <summary>
        /// Creates a new equipartition tasks.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="task">The task to create.</param>
        /// <param name="autoRemove">true if remove the item all done automatically; otherwise, false.</param>
        /// <returns>A new equipartition tasks created.</returns>
        public EquipartitionTask Create(string group, EquipartitionTask task, bool autoRemove = true)
        {
            return Create(group, new[] { task }, autoRemove) > 0 ? task : null;
        }

        /// <summary>
        /// Creates a new equipartition tasks.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="tasks">The tasks to create.</param>
        /// <param name="autoRemove">true if remove the item all done automatically; otherwise, false.</param>
        /// <returns>A new equipartition tasks created.</returns>
        public virtual int Create(string group, IEnumerable<EquipartitionTask> tasks, bool autoRemove = true)
        {
            if (!cache.ContainsKey(group))
            {
                lock (locker)
                {
                    if (!cache.ContainsKey(group))
                    {
                        cache[group] = new List<EquipartitionTask>();
                    }
                }
            }

            var list = cache[group];
            var count = 0;
            tasks = tasks.Where(task =>
            {
                if (task == null) return false;
                count++;
                return true;
            });
            if (autoRemove)
            {
                foreach (var task in tasks)
                {
                    task.HasBeenDone += (object sender, EventArgs eventArgs) =>
                    {
                        list.Remove(task);
                    };
                    list.Add(task);
                }
            }
            else
            {
                list.AddRange(tasks);
            }

            return count;
        }

        /// <summary>
        /// Gets a task fragment.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> GetFragment(string group, string fragmentId)
        {
            if (!cache.TryGetValue(group, out var list)) return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
            foreach (var task in list)
            {
                var fragment = task.TryGetByFragmentId(fragmentId);
                if (fragment == null) continue;
                return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>(task, fragment);
            }

            return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
        }

        /// <summary>
        /// Updates a specific task fragment.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <param name="tag">The new tag.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> UpdateFragment(string group, string fragmentId, EquipartitionTask.FragmentStates? state, string tag)
        {
            var info = GetFragment(group, fragmentId);
            if (info.IsSelected) info.Parent.UpdateFragment(info.ItemSelected, state, tag);
            return info;
        }

        /// <summary>
        /// Updates a specific task fragment.
        /// </summary>
        /// <param name="group">The group identifier.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> UpdateFragment(string group, string fragmentId, EquipartitionTask.FragmentStates state)
        {
            var info = GetFragment(group, fragmentId);
            if (info.IsSelected) info.Parent.UpdateFragment(info.ItemSelected, state);
            return info;
        }
    }

    /// <summary>
    /// The equipartition task collection.
    /// </summary>
    public static class EquipartitionTaskExtensions
    {
        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="pick">A handler to pick.</param>
        /// <returns>A task and fragment.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(this IEnumerable<EquipartitionTask> col, Func<EquipartitionTask, EquipartitionTask.Fragment> pick = null)
        {
            return Pick(col, null, pick);
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A task and fragment.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(this IEnumerable<EquipartitionTask> col, string tag, IEnumerable<string> except = null, bool onlyPending = false)
        {
            return Pick(col, null, task => task.Pick(tag, except, onlyPending));
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="pick">A handler to pick.</param>
        /// <returns>A task and fragment.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(this IEnumerable<EquipartitionTask> col, Func<EquipartitionTask, bool> predicate, Func<EquipartitionTask, EquipartitionTask.Fragment> pick = null)
        {
            if (col == null) return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
            if (predicate != null) col = col.Where(predicate);
            if (pick == null) pick = task => task.Pick();
            foreach (var item in col)
            {
                var f = pick(item);
                if (f == null) continue;
                return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>(item, f);
            }

            return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
        }

        /// <summary>
        /// Picks one and start.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="except">The fragment identifier except.</param>
        /// <param name="onlyPending">true if get only pending one; otherwise, false.</param>
        /// <returns>A task and fragment.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> Pick(this IEnumerable<EquipartitionTask> col, Func<EquipartitionTask, bool> predicate, string tag, IEnumerable<string> except = null, bool onlyPending = false)
        {
            return Pick(col, predicate, task => task.Pick(tag, except, onlyPending));
        }

        /// <summary>
        /// Gets a task fragment.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> GetFragment(this IEnumerable<EquipartitionTask> col, string fragmentId)
        {
            if (col == null) return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
            foreach (var task in col)
            {
                var fragment = task.TryGetByFragmentId(fragmentId);
                if (fragment == null) continue;
                return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>(task, fragment);
            }

            return new SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment>();
        }

        /// <summary>
        /// Updates a specific task fragment.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <param name="tag">The new tag.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> UpdateFragment(this IEnumerable<EquipartitionTask> col, string fragmentId, EquipartitionTask.FragmentStates? state, string tag)
        {
            var info = GetFragment(col, fragmentId);
            if (info.IsSelected) info.Parent.UpdateFragment(info.ItemSelected, state, tag);
            return info;
        }

        /// <summary>
        /// Updates a specific task fragment.
        /// </summary>
        /// <param name="col">The equipartition task collection.</param>
        /// <param name="fragmentId">The task fragment identifier.</param>
        /// <param name="state">The new state; or null if no change.</param>
        /// <returns>true if update succeeded; otherwise, false.</returns>
        public static SelectionRelationship<EquipartitionTask, EquipartitionTask.Fragment> UpdateFragment(this IEnumerable<EquipartitionTask> col, string fragmentId, EquipartitionTask.FragmentStates state)
        {
            var info = GetFragment(col, fragmentId);
            if (info.IsSelected) info.Parent.UpdateFragment(info.ItemSelected, state);
            return info;
        }
    }
}
