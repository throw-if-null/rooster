﻿using System;
using System.Collections.ObjectModel;

namespace Rooster.Adapters.Kudu
{
    public class KuduAdapterOptions
    {
        public string Name { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public Uri BaseUri { get; set; }

        public Collection<string> Tags { get; set; } = new Collection<string>();
    }
}