using System;
using System.Collections.Generic;

namespace TheRealJournalRando.IC
{
    public class ParametricFsmEditBuilder<TKey>
    {
        private readonly Dictionary<TKey, Action<PlayMakerFSM>> editLookup = new();
        private readonly Func<TKey, Action<PlayMakerFSM>> editGenerator;

        public ParametricFsmEditBuilder(Func<TKey, Action<PlayMakerFSM>> editGenerator)
        {
            this.editGenerator = editGenerator;
        }

        public Action<PlayMakerFSM> this[TKey key]
        {
            get => editLookup[key];
        }

        public Action<PlayMakerFSM> GetOrAddEdit(TKey key)
        {
            if (!editLookup.ContainsKey(key))
            {
                editLookup.Add(key, editGenerator(key));
            }
            return editLookup[key];
        }
    }

    public class ParametricFsmEditBuilder<TData, TKey> where TData : class
    {
        private readonly Dictionary<TKey, Action<PlayMakerFSM>> editLookup = new();
        private readonly Func<TData, TKey> keySelector;
        private readonly Func<TData, Action<PlayMakerFSM>> editGenerator;

        public ParametricFsmEditBuilder(Func<TData, TKey> keySelector, Func<TData, Action<PlayMakerFSM>> editGenerator)
        {
            this.keySelector = keySelector;
            this.editGenerator = editGenerator;
        }

        public Action<PlayMakerFSM> this[TKey key]
        {
            get => editLookup[key];
        }

        public Action<PlayMakerFSM> this[TData data]
        {
            get => editLookup[keySelector(data)];
        }

        public Action<PlayMakerFSM> GetOrAddEdit(TData data)
        {
            TKey key = keySelector(data);
            if (!editLookup.ContainsKey(key))
            {
                editLookup.Add(key, editGenerator(data));
            }
            return editLookup[key];
        }
    }
}
