using System;
using System.Collections.Generic;
using System.Linq;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Watcher;
using Utils;

namespace ClipboardViewer
{
    public class Bootstraper
    {
        public Bootstraper(ServiceLocator container)
        {
            var accessibleFromats = Providers.GetCurrentProviders().ToList();
            container.RegisterInitializer<IClipboard>(Clipboard.CreateReadOnly);
            container.RegisterInstance<IEnumerable<Func<IClipbordFormatProvider>>, List<Func<IClipbordFormatProvider>>>(accessibleFromats);
            container.RegisterInstance<TypeMapper, TypeMapper>(new TypeMapper());
            container.RegisterInstance<IClipbordWatcher, ClipbordWatcher>(new ClipbordWatcher());
        }
    }
}
