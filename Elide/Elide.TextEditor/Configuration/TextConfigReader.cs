using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Elide.Core;

namespace Elide.TextEditor.Configuration
{
    internal sealed class TextConfigReader : IExtReader
    {
        private readonly AbstractTextConfigService service;

        public TextConfigReader(AbstractTextConfigService service)
        {
            this.service = service;
        }

        public void Read(ExtSection section)
        {
            service.Configs = section.Entries.Select(e => new TextConfigInfo(
                e.Key,
                e.Element("display"),
                e.Element<TextConfigOptions>("options"))).ToList();

        }
    }
}
