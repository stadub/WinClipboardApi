using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ClipboardHelper;
using ClipboardHelper.FormatProviders;
using ClipboardHelper.Watcher;
using ClipboardViewer.ViewModel;
using Utils;
using Utils.TypeMapping;
using Utils.TypeMapping.MappingInfo;
using Utils.TypeMapping.TypeMappers;
using Clipboard = ClipboardHelper.Clipboard;

namespace ClipboardViewer
{
    public static class TypeMappers
    {
        public static string FormatProviders = "FormatProviders";
    }

    public class Bootstraper:IDisposable
    {
        public ServiceLocator Container { get; private set; }

        public Bootstraper():this(new ServiceLocator())
        {
            
        }
        public Bootstraper(ServiceLocator container)
        {
            this.Container = container;
        }

        public void InitServiceLocator()
        {
            var accessibleFromats = Providers.GetCurrentProviders().ToList();

            Container.RegisterType<IClipboard, Clipboard>();
            Container.RegisterInstance<IEnumerable<Func<IClipbordFormatProvider>>, List<Func<IClipbordFormatProvider>>>(accessibleFromats);

            var watcher= new ClipbordWatcher();
            Container.RegisterInstance<IClipbordWatcher, ClipbordWatcher>(watcher);
            Container.RegisterInstance<IClipbordMessageProvider, ClipbordWatcher>(watcher);

            Container.RegisterInitializer(ClipboardFormatTypeMappes,TypeMappers.FormatProviders);

        }

        private ITypeMapperRegistry ClipboardFormatTypeMappes()
        {
            var mappers = new TypeMapperRegistry();
            InitFormatProvidersMapping(mappers);
            return mappers;
        }


        public void InitFormatProvidersMapping(ITypeMapperRegistry typeMappers)
        {
            typeMappers.Register<UnknownFormatProvider, UnknownFormatViewModel>();
            typeMappers.Register<NotImplementedStandartFormat, NotImplementedStandartFormatViewModel>();
            typeMappers.Register<FileDropProvider, FileDropViewModel>();
            typeMappers.Register<UnicodeTextProvider, UnicodeTextViewModel>();
            typeMappers.Register<UnicodeFileNameProvider, FileNameViewModel>();
            typeMappers.Register<HtmlFormatProvider, HtmlClipboardFormatViewModel>();

            var elementMapper = new TypeMapper<SkypeMessageTextLine, SkypeMessageTextLineViewModel>();
            var arrMapper =ArrayTypeMapper.Create(elementMapper);
            
            typeMappers.Register<SkypeFormatProvider, SkypeQuoteFormatViewModel>()
                .MapProperty((property, ints) => property.LegacyQuote, arrMapper);

        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)return;
            disposed = true;
            Container.Dispose();
        }
    }
}
