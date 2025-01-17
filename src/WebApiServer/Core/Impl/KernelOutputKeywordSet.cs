﻿using LiteDB;
using System;
using System.Collections.Generic;

namespace NTMiner.Core.Impl {
    public class KernelOutputKeywordSet : SetBase, IKernelOutputKeywordSet {
        private readonly Dictionary<Guid, KernelOutputKeywordData> _dicById = new Dictionary<Guid, KernelOutputKeywordData>();
        private readonly Dictionary<Guid, List<IKernelOutputKeyword>> _dicByKernelOutputId = new Dictionary<Guid, List<IKernelOutputKeyword>>();
        private readonly string _connectionString;

        public KernelOutputKeywordSet(string dbFileFullName) {
            if (string.IsNullOrEmpty(dbFileFullName)) {
                throw new ArgumentNullException(nameof(dbFileFullName));
            }
            _connectionString = $"filename={dbFileFullName}";
            VirtualRoot.BuildCmdPath<AddOrUpdateKernelOutputKeywordCommand>(path: (message) => {
                InitOnece();
                DataLevel dataLevel = DataLevel.Global;
                if (_dicById.TryGetValue(message.Input.GetId(), out KernelOutputKeywordData exist)) {
                    exist.Update(message.Input);
                    exist.SetDataLevel(dataLevel);
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<KernelOutputKeywordData>();
                        col.Update(exist);
                    }
                }
                else {
                    KernelOutputKeywordData entity = new KernelOutputKeywordData().Update(message.Input);
                    entity.SetDataLevel(dataLevel);
                    _dicById.Add(entity.Id, entity);
                    if (!_dicByKernelOutputId.TryGetValue(entity.KernelOutputId, out List<IKernelOutputKeyword> list)) {
                        list = new List<IKernelOutputKeyword>();
                        _dicByKernelOutputId.Add(entity.KernelOutputId, list);
                    }
                    list.Add(entity);
                    using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                        var col = db.GetCollection<KernelOutputKeywordData>();
                        col.Insert(entity);
                    }
                }
            }, location: this.GetType());
            VirtualRoot.BuildCmdPath<RemoveKernelOutputKeywordCommand>(path: (message) => {
                InitOnece();
                if (message == null || message.EntityId == Guid.Empty) {
                    return;
                }
                if (!_dicById.ContainsKey(message.EntityId)) {
                    return;
                }
                KernelOutputKeywordData entity = _dicById[message.EntityId];
                _dicById.Remove(entity.GetId());
                if (_dicByKernelOutputId.TryGetValue(entity.KernelOutputId, out List<IKernelOutputKeyword> list)) {
                    list.Remove(entity);
                }
                using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                    var col = db.GetCollection<KernelOutputKeywordData>();
                    col.Delete(message.EntityId);
                }
            }, location: this.GetType());
        }

        protected override void Init() {
            using (LiteDatabase db = new LiteDatabase(_connectionString)) {
                var col = db.GetCollection<KernelOutputKeywordData>();
                foreach (var item in col.FindAll()) {
                    if (!_dicById.ContainsKey(item.GetId())) {
                        item.SetDataLevel(DataLevel.Profile);
                        _dicById.Add(item.GetId(), item);
                        if (!_dicByKernelOutputId.TryGetValue(item.KernelOutputId, out List<IKernelOutputKeyword> list)) {
                            list = new List<IKernelOutputKeyword>();
                            _dicByKernelOutputId.Add(item.KernelOutputId, list);
                        }
                        list.Add(item);
                    }
                }
            }
        }

        public List<IKernelOutputKeyword> GetKeywords(Guid kernelOutputId) {
            InitOnece();
            if (_dicByKernelOutputId.TryGetValue(kernelOutputId, out List<IKernelOutputKeyword> list)) {
                return list;
            }
            return new List<IKernelOutputKeyword>();
        }

        public IEnumerable<IKernelOutputKeyword> AsEnumerable() {
            InitOnece();
            return _dicById.Values;
        }
    }
}
