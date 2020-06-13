﻿using MongoDB.Bson.Serialization.Attributes;

namespace Rooster.DataAccess.KuduInstances.Entities
{
    public interface IKuduInstance
    {
    }

    public abstract class KuduInstance<T> : IKuduInstance
    {
        [BsonId]
        public T Id { get; set; }

        public string Name { get; set; }
    }
}